---
name: build-home-os-app
description: Scaffold a new app on the Home OS platform (built-in module or third-party plug-in) — manifest, backend slice, UI extension points, event bus wiring. Use when the user asks to add a new module/app to Home OS, e.g. "add a meal planner app" or "build a new Home OS app for X".
---

# Build a Home OS app

Home OS is a platform, not a fixed set of screens (see `docs/app-sdk.md`). A
new app must become a first-class citizen without editing any existing app's
code. Follow this sequence — do not skip steps or reorder them.

## 0. Ask if unclear

If the app's purpose, name, or which existing data it should build on isn't
obvious from the request, ask the user before scaffolding. Do not guess a
scope larger than what was asked.

## 1. Survey what already exists — reuse before building

Read `docs/app-sdk.md` (event catalog + capability APIs) and skim
`backend/src/HomeOS.Domain` for entities already modeling what this app
needs. A meal-planner app does not get its own task system — it calls the
existing Tasks capability. If the thing the new app needs isn't modeled yet,
say so explicitly rather than silently duplicating a smaller version of it.

## 2. Write the manifest first

Create `backend/src/HomeOS.Api/Apps/<app-id>/manifest.json` following the
schema in `docs/app-sdk.md` §1: `id`, `name`, `version`, `icon`,
`description`, `permissions`, `publishes`, `subscribes`, `ui`. This is the
contract for everything that follows — get it right before writing code.

## 3. Backend slice

Under `backend/src/HomeOS.Api/Apps/<app-id>/`:
- Entities specific to this app go in this folder, not in `HomeOS.Domain`,
  unless the household's core data model genuinely needs to change (rare —
  ask first).
- MediatR command/query handlers reuse existing capability interfaces
  (`IReminderService`, `INotificationService`, etc. — see `docs/app-sdk.md`
  §3) instead of reimplementing scheduling, email, or permission checks.
- Raise domain events for anything declared in the manifest's `publishes`
  list; subscribe via a MediatR `INotificationHandler` for anything in
  `subscribes`.
- Guard every subscription and capability call so the app still boots if a
  dependency is absent — check, don't assume.

## 4. Register UI extension points

Do not edit `frontend/src/components/Layout.tsx` or `Dashboard.tsx`
directly. Instead:
- Add the app's route under `frontend/src/apps/<app-id>/`.
- Register its `dashboardWidget`, `navEntry`, `commandPaletteActions`, and
  `searchProvider` (if declared in the manifest) through the app registry
  (`frontend/src/apps/registry.ts` — create it on first use if it doesn't
  exist yet, following the pattern: an array of manifests that `Layout.tsx`
  and `Dashboard.tsx` read from, never a hardcoded list per app).

## 5. Permissions

List only the capability scopes the app actually calls in `permissions`.
Do not request broader access "in case it's needed later."

## 6. Verify graceful degradation

Temporarily comment out or stub one capability/event this app depends on
and confirm the app hides its affected UI instead of throwing. This check
is not optional — it's what "cooperate without knowing about each other"
means in practice.

## 7. Report back

Summarize: what the app does, which existing data/capabilities it reused
vs. what's new, which permissions it requested and why, and confirm it
installs/uninstalls cleanly (no orphaned rows, no dangling nav entry).
