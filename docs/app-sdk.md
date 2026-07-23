# Home OS App SDK

This is the contract every app on Home OS — built-in or third-party, written by a
person or by an agent — depends on. It exists so a new app can be added without
touching the code of any existing app. See [`plan.md`](./plan.md) for the wider
architecture; this document is the part of it meant to be read by whoever (or
whatever) is building app #9.

Status: design reference for Phase 5 (platform layer). Phases 1–4 build the
concrete data each event/capability below is described in terms of.

## 1. The app manifest

Every app registers itself with a manifest. Installing an app is writing this
JSON to the `AppInstallation` table — nothing is special-cased in code for a
built-in app vs. a third-party one.

```jsonc
{
  "id": "meal-planner",                 // unique, kebab-case
  "name": "Meal Planner",
  "version": "1.0.0",
  "icon": "utensils",                   // icon-set key, not a file upload
  "description": "Plan meals for the week and turn them into shopping-list tasks.",

  "permissions": [                      // capability scopes this app asks for
    "reminders:create",
    "notifications:send",
    "tasks:read",
    "tasks:create",
    "members:read"
  ],

  "publishes": [                        // events this app raises
    "mealplanner.meal.planned"
  ],
  "subscribes": [                       // events this app reacts to
    "task.completed"
  ],

  "ui": {                               // extension points — see section 4
    "dashboardWidget": "MealPlannerTodayWidget",
    "navEntry": { "label": "Meals", "route": "/meals" },
    "commandPaletteActions": ["plan-meal", "add-to-shopping-list"],
    "searchProvider": true
  }
}
```

A household member installs an app by granting exactly the `permissions` it
asks for (section 3). Nothing beyond that scope is reachable. Uninstalling
removes the manifest row and revokes the grants; the app's own data rows stay
owned by the app and are simply no longer surfaced anywhere.

## 2. Event catalog

Modules never call each other's code directly — they raise events on the bus
(`HomeOS.Domain.Common.IDomainEvent`, `HomeOS.Application` handlers) and any
app can subscribe. Naming convention: `<module>.<entity>.<pastTenseVerb>`.

| Event | Raised by | Typical payload |
|---|---|---|
| `task.created` | Tasks | taskId, title, dueDate, assigneeId |
| `task.completed` | Tasks | taskId, completedByMemberId |
| `task.overdue` | Tasks (scheduled check) | taskId, dueDate |
| `board.card.moved` | Kanban | cardId, fromColumn, toColumn |
| `calendarEvent.created` | Calendar | eventId, start, end |
| `reminder.fired` | Reminders | reminderId, targetMemberId, sourceRef |
| `note.created` | Notes | noteId, linkedTo? |
| `bill.dueSoon` | Finance | billId, amount, dueDate |
| `bill.paid` | Finance | billId, paidByMemberId |
| `document.renewalDue` | Life admin | documentId, renewalDate |
| `member.invited` | Household | memberId, invitedByMemberId |

A third-party app adds its own events to this table when it's installed
(e.g. `mealplanner.meal.planned` above) — the catalog is open-ended, not a
fixed enum owned by the core team.

## 3. Capability APIs

These exist once, on the platform, so no app rebuilds them:

- **Reminders** — `POST /api/reminders` — schedule a one-off or recurring
  reminder targeted at a member, from any source (`sourceType` + `sourceId`).
- **Notifications** — in-app + email, per-category opt-in per member. An app
  sends through this instead of holding its own SMTP credentials.
- **Sharing** — resolve what's private / shared-with-household / shared with
  specific members for any object an app owns, using the same rules as
  built-in modules.
- **Members** — read household roster, assign an object to a member.
- **Linking** — attach an app's object to an existing one (a note to a task,
  a meal plan to a shopping list) via a generic `(entityType, entityId)`
  reference, without either side knowing the other's schema.
- **Search** — register a search provider; the global search box calls every
  installed app's provider and merges results.

## 4. UI extension points

Declared in the manifest's `ui` block, rendered by registries on the
frontend — a new app never edits `Layout.tsx` or the dashboard directly:

- `dashboardWidget` — component registered into the dashboard's widget slot.
- `navEntry` — adds itself to the sidebar (see `frontend/src/components/Layout.tsx`).
- `commandPaletteActions` — actions surfaced in the ⌘K command palette.
- `searchProvider` — participates in global search (section 3).

## 5. Guidance for an agent building a new app

1. Check what already exists before adding anything — an app that needs
   tasks or reminders calls the existing capability API, it does not build
   its own.
2. Write the manifest first (section 1). It is the single source of truth
   for what the app touches.
3. Register UI extension points so the app shows up on the dashboard, in
   nav, in search, and in the command palette from day one — never require
   a change to another app's file to become visible.
4. Subscribe to events instead of calling other modules; publish events for
   anything another app might reasonably want to react to later.
5. Request only the permissions actually used. The household can review and
   revoke every grant individually.
6. Assume any dependency might not be installed — degrade gracefully
   (hide the widget, skip the subscription) rather than throwing.
7. Uninstalling must leave nothing broken: no orphaned foreign keys in core
   tables, no dangling nav entries.
