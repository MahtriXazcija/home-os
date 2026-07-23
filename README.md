# Home OS

A personal "home operating system" — tasks, kanban, calendar, reminders,
notes, finance, and life admin in one connected app, shared across a
household, with email notifications. See [`docs/plan.md`](docs/plan.md) for
the build plan and [`docs/app-sdk.md`](docs/app-sdk.md) for the extensibility
model that lets new apps plug into the platform.

**Live:** https://home-os-wine.vercel.app

## Stack

React + TypeScript (frontend) · ASP.NET Core 9 (backend) · PostgreSQL (Neon)

## Running locally

```bash
# backend
cd backend/src/HomeOS.Api
dotnet user-secrets set "ConnectionStrings:HomeOsDb" "<your Neon connection string>"
dotnet run --urls http://localhost:5080

# frontend (separate terminal)
cd frontend
npm install
npm run dev
```

## Deployment

- Frontend → Vercel, auto-deploys from `main`, root directory `frontend`
- Backend → Render (Docker), auto-deploys from `main`, root directory `backend`
- Database → Neon (Postgres)
