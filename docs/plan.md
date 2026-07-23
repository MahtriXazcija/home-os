# Home OS — build plan (engineering reference)

Full visual version (competitor research, wireframes, hosting rationale) was
published separately as the pitch document; this file is the condensed
version that lives with the code.

## Stack

| Layer | Tech |
|---|---|
| Frontend | React 18 + TypeScript + Vite, TanStack Query, react-router |
| Backend | ASP.NET Core 9 Web API, MediatR (CQRS + event bus), FluentValidation |
| Data | PostgreSQL via EF Core / Npgsql |
| Realtime | SignalR |
| Auth | ASP.NET Core Identity + JWT |
| Background jobs | Hangfire (reminders, digest email) |
| Email | Brevo (free tier) |

## Repo layout

```
backend/
  src/HomeOS.Domain/          entities, domain events (no dependencies)
  src/HomeOS.Application/     MediatR handlers, DTOs, validators
  src/HomeOS.Infrastructure/  EF Core, Identity, Hangfire, email
  src/HomeOS.Api/             controllers, SignalR hubs, composition root
frontend/
  src/components/             shared UI (Layout, nav)
  src/pages/                  one folder per module
  src/api/                    typed fetch client
docs/
  app-sdk.md                  manifest schema, event catalog, capability APIs
  plan.md                     this file
```

## Phases

0. **Foundations** — empty solution + React app, repo structure, Postgres on
   Neon, deploy an empty "hello world" to Vercel + Azure so a shareable link
   exists from day one. *(current phase)*
1. **Auth, Household, Members** — registration/login, household creation,
   invites, private/shared model. Everything else depends on this.
2. **Tasks, Kanban, Calendar** — the core trio, already cross-linked (a task
   with a due date is a card and a calendar entry).
3. **Reminders + email** — reminders triggerable from any module, in-app and
   email notification with per-member, per-category opt-in.
4. **Notes, Finance, Life admin** — remaining three modules, linked into
   existing entities (a note on a task, a bill triggering a reminder).
5. **Platform layer** — event bus, app registry, install-time permission
   grants, command palette + search as registries other apps plug into.
6. **Proof-of-concept third-party app** — e.g. Meal Planner, built only
   through the manifest + existing capability APIs (see `app-sdk.md`) —
   the live demonstration that the platform claim is real.
7. **Polish** — light/dark theme, responsive layout, edge cases, share the
   link with reviewers.

## Hosting (all free tier)

- Frontend → Vercel — **live: https://home-os-wine.vercel.app**
- API → Render (Docker) — **live: https://home-os-mkrb.onrender.com**
  (switched from the originally planned Azure App Service to avoid the
  card-verification step; trade-off is a ~30–50s cold start after 15 min
  idle on the free tier)
- Database → Neon (serverless Postgres)
- Email → Brevo (300/day free) — not wired up yet, ships in Phase 3
