# TRS — Training Registrar System

TRS is an ASP.NET Core 7 MVC + Razor Pages application for Universal Leaf that manages employee training programs, courses, schedules, registrations, approvals, and feedback.

It's a single-project internal enterprise app (`TRS.csproj` / `TRS.sln`), backed by SQL Server via EF Core, with a Kendo UI (Telerik) + Bootstrap + jQuery frontend. There is no separate frontend app and no test project — it's a classic server-rendered MVC app with Razor views under `Views/<ControllerName>/`.

## Requirements

- .NET 7 SDK (build only needs the SDK; running needs a matching .NET runtime — see [Run](#run))
- SQL Server (for data-backed pages and login)
- Access to a Telerik UI for ASP.NET Core NuGet feed (see [Build](#build))
- Internal LDAP/HR environment (for full SSO login flow)

## Build

```
dotnet build
```

This project depends on the commercial `Telerik.UI.for.AspNet.Core` package (currently pinned to `2023.2.606` in `TRS.csproj`), which is not on public nuget.org. Restore requires one of:

- Telerik's private feed (`https://nuget.telerik.com/v3/index.json`, authenticated with an `api-key` / Telerik account API key), or
- A local NuGet source pointing at a manually-downloaded `.nupkg`, e.g. `dotnet nuget add source <folder-with-nupkg> -n TelerikLocal`.

This is machine-specific and registered in the global NuGet config, not the repo. If `dotnet build` fails with `NU1102` for this package, check `dotnet nuget list source` for a Telerik source before assuming it's unconfigured.

Note: `wwwroot/kendo/kendo-ui-license.js` is a separate, client-side-only license key that unlocks Kendo UI JS widgets in the browser. It does not provide NuGet feed access.

## Run

```
DOTNET_ROLL_FORWARD=LatestMajor dotnet run --launch-profile http
```

The project targets `net7.0`. If only newer .NET runtimes (8/9) are installed, `DOTNET_ROLL_FORWARD=LatestMajor` lets the app run without project changes; otherwise you'll see `You must install or update .NET to run this application`.

Local URLs are defined in `Properties/launchSettings.json`:
- `http` profile: `http://localhost:12345`
- `https` profile: `https://localhost:7001` / `http://localhost:5001`

The app serves static files/Razor views with no backing services, but the default route (`Home/Crypto/{encryptedId}`, the SSO entry point) redirects to `/Home/Error` without a valid encrypted id or reachable DB — that's expected. Real login and data-backed pages need a reachable SQL Server (connection string `devCon` in `appsettings.json`, set via `dotnet user-secrets` for local dev) and, for full login, the internal LDAP/HR environment.

## Database migrations

Uses `TRS.Data.AppDBContext`:

```
dotnet ef migrations add <Name>
dotnet ef database update
```

Stored procedures, views, and one-off schema changes are shipped separately as raw `.sql` scripts under `ITRequest/<ticket-number>/`, tied to IT change-request tickets.

## Architecture

**Layering**: `Controllers/` → `Interfaces/I*Service` → `Services/*Service` → `Data/AppDBContext` (EF Core) → `Models/`. Each feature area (TrainingSchedule, TrainingProgramAndCourse, TrainingCoordinator, TrainingRegistration, TrainingRegistrationApproval, TrainingFeedback, TraineeRegistration, TrainingMonitoring, TrainingProfile, ActivityLog) has a matching Controller, Service interface/implementation, and often a `ViewModels/` class bundling the data a Razor view needs.

**Authentication** is SSO-style: an external portal links in at `Home/Crypto/{encryptedEmployeeId}`, the id is AES-decrypted to an employee number, user info and permissions are loaded via stored procedures, and session state is written to ASP.NET Core's server-side `Session`. Per-request authorization is enforced via `[ValidateSession]` / `[ValidateAccess]` action filter attributes.

See `CLAUDE.md` for full architecture notes, including data-access patterns and known gotchas.

## Testing

No automated test suite exists in this repo.
