# TimeSheet Tracker — Angular 14 + ASP.NET Core + SQL Server

A full-stack portfolio application with an Angular 14 single-page application, ASP.NET Core REST API, Entity Framework Core, SQL Server, and ASP.NET Core Identity cookie authentication.

## Technology stack

- Frontend: Angular 14.2, TypeScript 4.8, RxJS 7.5, Angular Router, Reactive Forms
- Backend: C# / ASP.NET Core Web API
- Database: SQL Server / Entity Framework Core
- Authentication: ASP.NET Core Identity with secure HttpOnly authentication cookies
- Logging: Serilog
- UI: Responsive custom dashboard styles

## Prerequisites

- Visual Studio 2022 or later with ASP.NET and web development workload
- .NET 10 SDK for the included backend project
- SQL Server LocalDB or SQL Server Developer Edition
- Node.js 16.10 or later (Node 16.20 LTS is a suitable legacy-compatible choice)
- npm 8 or later

**Angular 14 is an older, end-of-support release.** It is used here because the project requirement specifies Angular 14. For a new production system, choose a currently supported Angular version and plan a tested upgrade path.

## Solution structure

```text
TimeSheetTracker-Angular/
├── TimeSheetTracker.sln
├── src/
│   └── TimeSheetTracker.Web/        # ASP.NET Core API + SQL persistence
└── client/
    └── timesheet-tracker-ui/        # Angular 14 application
```

## Configure SQL Server

Open `src/TimeSheetTracker.Web/appsettings.json` and update `ConnectionStrings:DefaultConnection`.

Default LocalDB:
```text
Server=(localdb)\\MSSQLLocalDB;Database=TimeSheetTrackerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

## Run the backend

1. Open `TimeSheetTracker.sln` in Visual Studio.
2. Set `TimeSheetTracker.Web` as startup project.
3. Verify SQL Server LocalDB is installed.
4. Run with the `TimeSheetTracker.Web` launch profile. The API listens on `https://localhost:7241` and `http://localhost:5241`.
5. Development initialization creates the database, roles, demo users, and sample projects.

## Run the Angular frontend

Open a terminal:

```bash
cd client/timesheet-tracker-ui
npm install
npm start
```

Open `http://localhost:4200`.

The Angular dev server proxies `/api` requests to `https://localhost:7241`. Trust the local ASP.NET Core HTTPS development certificate if the browser rejects it:

```bash
dotnet dev-certs https --trust
```

Start the backend before using the frontend. Both processes must remain running during the demo.

## Demo accounts

Password for all demo users: `Demo!Pass12345`

| Role | Email |
|---|---|
| Admin | admin@timesheet.local |
| Manager | manager@timesheet.local |
| Employee | employee@timesheet.local |

Demo accounts are seeded only in Development. Never use these credentials outside a local demo.

## Current API endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| POST | `/api/auth/login` | Sign in and issue Identity cookie |
| GET | `/api/auth/me` | Current authenticated user |
| POST | `/api/auth/logout` | Sign out |
| GET | `/api/timesheets` | List current user's sheets; Manager/Admin see recent sheets |
| POST | `/api/timesheets` | Create or retrieve Monday-start weekly sheet |
| GET | `/api/timesheets/{id}` | Get a timesheet and its entries |
| GET | `/api/timesheets/projects` | Get projects assigned to current employee |
| POST | `/api/timesheets/{id}/entries` | Add a time entry |
| POST | `/api/timesheets/{id}/submit` | Submit for approval |
| POST | `/api/timesheets/{id}/review` | Manager/Admin approval or rejection |

The Angular application uses same-origin `/api` paths and an HTTP interceptor that sends credentials. Authentication is cookie-based rather than storing bearer tokens in browser local storage. Antiforgery protection should be added for cookie-authenticated state-changing API requests before production deployment; the existing MVC forms already use antiforgery validation.

## Important implementation notes

- The Angular 14 application uses NgModules, conventional `*ngIf` / `*ngFor` templates, constructor dependency injection, and RxJS Observables. It deliberately avoids Angular 16+ Signals and Angular 17+ built-in control-flow syntax.
- The existing ASP.NET Core MVC pages remain in the backend project but are not the main UI. Angular is the intended user interface.
- API authentication uses ASP.NET Core Identity cookies; API requests receive 401/403 responses rather than MVC HTML redirects.
- Backend business rules remain in `TimesheetService`; controllers are HTTP adapters.
- Development DB bootstrap uses `EnsureCreated`. Use EF Core migrations for schema evolution and production.
- Some dashboard metric cards currently contain illustrative UI values; the recent-timesheet list is API-backed. Connect the metrics to dedicated aggregate endpoints before claiming all dashboard KPIs are live.
- Manager/Admin review currently lacks manager-to-team scoping. Implement an explicit reporting hierarchy before using real employee data.
- The API currently returns anonymous projection objects for read endpoints. For a larger application, use dedicated response DTOs consistently.
- This project has not been compiled or executed in this environment because the .NET and Node toolchains are not available here. Run the verification steps below on your development machine before presenting it.

## Local verification checklist

```bash
# Backend
dotnet restore
dotnet build

# Frontend
cd client/timesheet-tracker-ui
npm install
npm run build
```

Then test employee entry creation, week validation, submit, manager approval/rejection, role authorization, and unauthorized API responses manually. Add automated unit and SQL Server integration tests before describing the system as production-ready.

