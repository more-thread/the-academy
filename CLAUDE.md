# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

TRS ("Training Registrar System", repo name `the_academy`) is an ASP.NET Core 7 MVC + Razor Pages application that manages employee training programs, courses, schedules, registrations, approvals, and feedback. It's a single-project internal enterprise app (`TRS.csproj` / `TRS.sln`), backed by SQL Server via EF Core, with a Kendo UI (Telerik) + Bootstrap + jQuery frontend.

There is no separate frontend app and no test project — it's a classic server-rendered MVC app with Razor views under `Views/<ControllerName>/`.

## Commands

Build:
```
dotnet build
```

**This fails out of the box** on a machine with only the default nuget.org source, with `NU1102: Unable to find package Telerik.UI.for.AspNet.Core`. The project depends on the commercial `Telerik.UI.for.AspNet.Core` package (currently pinned to `2023.2.606` in `TRS.csproj`), which is not on public nuget.org. There is no `NuGet.Config` checked into the repo, so restore needs one of:
- Telerik's private feed (`https://nuget.telerik.com/v3/index.json`, authenticated with `api-key` / a Telerik account API key), or
- A local NuGet source pointing at manually-downloaded `.nupkg` file(s) for the package, e.g. `dotnet nuget add source <folder-with-nupkg> -n TelerikLocal`. This is machine-specific (registered in the global NuGet config, not the repo) — if `dotnet build` fails with `NU1102` for this package again on a fresh machine, check `dotnet nuget list source` for a Telerik source first before assuming it's unsolved.

Note `wwwroot/kendo/kendo-ui-license.js` (`KendoLicensing.setScriptKey(...)`) is a *separate*, client-side-only license key that unlocks the Kendo UI JS widgets in the browser — it does not provide NuGet feed access and is unrelated to this restore step.

Run (once restore succeeds):
```
DOTNET_ROLL_FORWARD=LatestMajor dotnet run --launch-profile http
```
The project targets `net7.0`, which may not be installed as a runtime (only the SDK matters for `dotnet build`, but `dotnet run`/`dotnet TRS.dll` need the matching runtime). If you only have .NET 8/9 runtimes installed, `DOTNET_ROLL_FORWARD=LatestMajor` lets the app run on whatever newer major runtime is present, with no project changes needed. Without it you'll see `You must install or update .NET to run this application` even though `dotnet build` succeeded.

Local URLs are defined in `Properties/launchSettings.json` — the `http` profile listens on `http://localhost:12345`, `https` on `https://localhost:7001`/`http://localhost:5001`. The app starts and serves static files/Razor views fine with no backing services, but the default route (`Home/Crypto/{encryptedId}`, the SSO entry point) redirects to `/Home/Error` when there's no valid encrypted id or reachable DB — that's expected, not a failure. Real login and any data-backed page need a reachable SQL Server (connection string `devCon` in `appsettings.json`, currently blank — set via `dotnet user-secrets` for local dev, `UserSecretsId` is already configured in `TRS.csproj`) and, for full login flow, the internal LDAP/HR environment.

EF Core migrations (uses `TRS.Data.AppDBContext`):
```
dotnet ef migrations add <Name>
dotnet ef database update
```

No test suite exists in this repo.

## Architecture

**Layering**: `Controllers/` → `Interfaces/I*Service` → `Services/*Service` → `Data/AppDBContext` (EF Core) → `Models/`. Each feature area (TrainingSchedule, TrainingProgramAndCourse, TrainingCoordinator, TrainingRegistration, TrainingRegistrationApproval, TrainingFeedback, TraineeRegistration, TrainingMonitoring, TrainingProfile, ActivityLog) has a matching Controller, Service interface/implementation, and often a `ViewModels/` class that bundles the data a Razor view needs (e.g. `TrainingScheduleViewModel` bundles `FormControl`, `TrainingScheduleDetails`, and `JobClasses` together for one partial view). `DTOs/` are only used for binding config sections (`FtpSettingDto`, `LdapSettingDto`, `SessionDto`), not for API payloads.

**Data access is split between two patterns**:
- Most entity CRUD goes through normal EF Core LINQ against `AppDBContext` (`Services/*Service.cs`), with `t*`-prefixed `DbSet`s for transactional tables (`tTrainingSchedule`, `tTrainingRegistration`, `tUserLogs`, `tTrainingFeedback`) and `m*`-prefixed for master/reference tables (`mTrainingProgram`, `mTrainingCourse`, `mJobClass`, `mTrainingCoordinator`, `mTrainingFeedbackQuestions`).
- Cross-cutting concerns (permissions, user lookup, email sending) go through `Global/GlobalService.cs`, which calls SQL Server stored procedures directly via `FromSqlRaw`/`ExecuteSqlRaw` (e.g. `EXEC TRS.sp_Global_GetUserForms`, `sp_Global_GetUserInfo`, `sp_Global_SendEmail`). `VwHrEmployeeInfo`/`VwHrRegion` are keyless entities mapped to read-only SQL views (`vw_HR_EmployeeInfo`, `vw_HR_Regions`) owned by an external HR system.

**Schema change management is also split in two places**: standard EF Core migrations live in `Migrations/`, but stored procedures, views, and one-off schema changes are shipped as raw `.sql` scripts under `ITRequest/<ticket-number>/`, tied to IT change-request ticket numbers (e.g. `ITRequest/000473-1023/1st step create database agootrainingregistrarsystem2024.sql`). When changing a stored proc or view, look here rather than in `Migrations/`.

**Authentication/session flow is SSO-style, not a login form**:
1. An external portal links into this app at the default route `Home/Crypto/{encryptedEmployeeId}` (see `MapControllerRoute` in `Program.cs` — `Crypto` is the default action, not `Index`).
2. `HomeController.Crypto` strips a 4-character prefix off the route value, then `EmployeeAuthenticationService.DecryptEmployeeId` AES-decrypts it (key from `Authentication:EncryptionKey` config, falling back to a hardcoded default) to get an employee number.
3. `GlobalService.GetUserInfo` (stored proc) loads the user, and `SessionService.CreateUserSessionAsync` writes user fields into ASP.NET Core's server-side `Session` (backed by `AddDistributedMemoryCache()` — in-process only, so this won't survive across multiple server instances without sticky sessions).
4. `HomeController.Index` then loads permissions via `GlobalService.GetUserAccess` (stored proc) and populates `FormService.GetForms`/`MenuService.GetMenuItem`.

**Per-request authorization** is enforced with action filter attributes, not global middleware: `[ValidateSession]` (checks `ISessionService.IsSessionValid()`) and `[ValidateAccess(ControllerName = "...")]` (additionally checks `FormService.FormList` for a matching `Controller`+`EmployeeNo` entry) are applied directly on controllers/actions. `Middlewares/SessionMiddleware.cs` and `Middlewares/AccessMiddleware.cs` exist but are **not** wired up in `Program.cs` — they're dead code; the attributes are the real enforcement path.

**Gotcha**: `Global/FormService.cs` holds `FormList` as a `static` field — permissions for every user who has visited `Home/Index` live in one process-wide static list simultaneously (filtered by `EmployeeNo` on read/write, not partitioned by session). Keep this in mind when reasoning about concurrency or cross-user data leakage bugs in permission checks.

`Middlewares/ImpersonationMiddleware.cs` uses P/Invoke (`LogonUser` from `advapi32.dll`) to impersonate a Windows service account for LDAP/FTP access — it's Windows-only and currently commented out in `Program.cs`.

## MOC documents (`Docs/TRS MOC/`)

A **MOC** (Method of Confirmation) is the QA/UAT sign-off checklist for a change — one `.xlsx` per TRS form, named `<FormID>_<FORM NAME>_MOC.xlsx`. Existing forms:

| Form ID | Form name | File |
| --- | --- | --- |
| `TRTT-0002` | TRAINING REGISTRATION | `Docs/TRS MOC/TRTT-0002_TRAINING REGISTRATION_MOC.xlsx` |
| `TRTT-0003` | TRAINEE REGISTRATION | `Docs/TRS MOC/TRTT-0003_TRAINEE REGISTRATION_MOC.xlsx` |
| `TRTT-0004` | TRAINING MONITORING | `Docs/TRS MOC/TRTT-0004_TRAINING MONITORING_MOC.xlsx` |

`Docs/TRS MOC/sample format.xlsx` is the canonical template and defines the structure below. New forms continue the sequence — `TRTT-0005` is the next free id. If the form's real id is documented elsewhere (an IT request, an existing ULIISAdmin entry), use that and say so rather than guessing.

### Layout

One sheet. Header cells:

- `A3` = `Form ID:`, `B3` = the form id (e.g. `TRTT-0004`)
- `A4` = `Form Name:`, `B4` = the form name in UPPERCASE (e.g. `TRAINING MONITORING`)

Then repeating blocks, each separated by exactly one blank row:

```
row 7    No | Checkpoints | IT Sub Request No. | IT in Charge    <- header
row 8    1  | <text>      | 000750-1025-001    | JSR
row 9    (blank)
row 10   No | Checkpoints | IT Sub Request No. | IT in Charge    <- header repeats
row 11   1  | <text>      | 000692-1125-001    | JSR
row 12   2  | <text>      | 000229-0126-001    | JSR
```

`No` restarts at `1` in every block. A blank `IT Sub Request No.` cell means "same sub-request as the row above". Note the existing `TRTT-*` files omit the repeated header row; the template includes it, and new blocks follow the template.

`Checkpoints` is free text, often multi-line: what a tester should verify, plus what changed underneath — affected stored procedures and views, files touched, or an explicit "No stored procedure or database schema changes". Match the voice of the surrounding entries.

A sub-request may also get a **detail sheet** named exactly after it (e.g. `000901-1025-001`) holding numbered notes and screenshots, linked from the `IT Sub Request No.` cell. These are optional — don't create one.

### When resolving a GitHub issue

Issue titles carry both keys, e.g. `000111-0726-002 TRAINING PROFILE viewing access` → sub-request `000111-0726-002`, form `TRAINING PROFILE`. Once the issue's work is done, update that form's MOC as part of the change:

1. Resolve the form name from the issue title to a file in `Docs/TRS MOC/`.
2. **No MOC for that form yet?** Copy `sample format.xlsx` to `<FormID>_<FORM NAME>_MOC.xlsx`, delete the template's example blocks, and set `B3`/`B4`.
3. Append a new block: one blank row, the header row, then rows numbered from `1` — one row per distinct thing a tester must confirm.
4. Fill `IT Sub Request No.` from the issue title and `IT in Charge` with `JSR`.
5. Mention the MOC entry in the PR description so the reviewer can check it.

Add MOC entries when resolving an issue — not for every incidental commit.

### Editing the workbooks

`openpyxl` is not installed, and the system Python is PEP 668 "externally managed" — `pip install --user` fails. Use a throwaway venv:

```
python3 -m venv /tmp/moc-venv && /tmp/moc-venv/bin/pip install openpyxl
/tmp/moc-venv/bin/python your_script.py
```

```python
from openpyxl import load_workbook
wb = load_workbook("Docs/TRS MOC/TRTT-0004_TRAINING MONITORING_MOC.xlsx")
ws = wb.active
r = ws.max_row + 2                                    # one blank row after the last block
ws.cell(r, 1, "No"); ws.cell(r, 2, "Checkpoints")
ws.cell(r, 3, "IT Sub Request No."); ws.cell(r, 4, "IT in Charge")
ws.cell(r + 1, 1, 1)
ws.cell(r + 1, 2, "<what the tester verifies + what changed underneath>")
ws.cell(r + 1, 3, "000111-0726-002"); ws.cell(r + 1, 4, "JSR")
wb.save("Docs/TRS MOC/TRTT-0004_TRAINING MONITORING_MOC.xlsx")
```

An `openpyxl` round-trip drops embedded images from existing detail sheets. That's accepted here, since detail sheets and screenshots are optional.

## GitHub Actions / `@claude`

`.github/workflows/claude.yml` runs the `@claude` GitHub App on issue/PR comments containing `@claude`. It authenticates via `claude_code_oauth_token: ${{ secrets.CLAUDE_CODE_OAUTH_TOKEN }}` (a token from `claude setup-token`, tied to a Claude subscription) rather than an Anthropic Console API key.
