# NestHub Deployment Guide

This guide covers provisioning SQL Server, hosting the API and Admin portal under IIS, and
producing production-ready mobile binaries for Android and iOS.

## 1. SQL Server Provisioning

### 1.1 Requirements

- SQL Server 2019+ (Developer, Standard, or Enterprise), or Azure SQL Database.
- A login/user with `db_owner` on the target database (or sufficient rights to create it).

### 1.2 Schema instantiation

Two equivalent paths are provided — use whichever fits your ops process:

**Option A — hand-authored script (DBA-friendly, idempotent):**

```powershell
sqlcmd -S <server> -U <user> -P <password> -i database\Schema.sql
```

`database\Schema.sql` creates the `NestHub` database and all 10 tables (`Societies`, `Users`,
`Residents`, `Vendors`, `Services`, `Reviews`, `SosRequests`, `SosClaims`, `AnalyticsLogs`,
`Categories`) with their constraints and indexes. It is safe to re-run — every `CREATE` is
guarded by an existence check.

**Option B — EF Core migrations (source of truth for ongoing schema changes):**

```powershell
dotnet tool install --global dotnet-ef
dotnet ef database update `
  --project src\NestHub.Infrastructure\NestHub.Infrastructure.csproj `
  --startup-project src\NestHub.API\NestHub.API.csproj `
  --connection "Server=<server>;Database=NestHub;User Id=<user>;Password=<password>;TrustServerCertificate=True;"
```

Going forward, schema changes should be made via EF Core migrations (`dotnet ef migrations add
<Name>`) and then reflected back into `database\Schema.sql` for the DBA-facing copy.

### 1.3 Indexing

All indexes needed by the current query patterns are created by the script/migrations:

| Table | Index | Purpose |
|---|---|---|
| `Users` | `UX_Users_PhoneNumber` (unique) | Login lookup, uniqueness enforcement |
| `Residents` | `UX_Residents_UserId` (unique), `IX_Residents_SocietyId` | Profile lookup, society roster |
| `Vendors` | `UX_Vendors_UserId` (unique) | Profile lookup |
| `Services` | `IX_Services_VendorId`, `IX_Services_Category` | Catalogue browsing, category search |
| `Reviews` | `IX_Reviews_VendorId_SocietyId` | Neighbor-Verified Reviews feed |
| `SosRequests` | `IX_SosRequests_SocietyId_Category_Status` | Vendor SOS lead matching |
| `SosClaims` | `UX_SosClaims_SosRequestId_VendorId` (unique) | One-claim-per-vendor enforcement |
| `AnalyticsLogs` | `IX_AnalyticsLogs_VendorId_CreatedDateTime` | Vendor analytics dashboard range queries |
| `Categories` | `UX_Categories_Name` (unique) | Category management uniqueness |

### 1.4 Connection strings

Set the `ConnectionStrings:NestHubDatabase` value in `appsettings.Production.json` (or an
environment variable `ConnectionStrings__NestHubDatabase`) for both `NestHub.API` and
`NestHub.Admin`. Do not commit production connection strings or credentials to source control.

## 2. IIS Hosting (NestHub.API and NestHub.Admin)

Both are standard ASP.NET Core 8.0 apps and are hosted the same way.

### 2.1 Prerequisites on the IIS server

1. Install the **.NET 8.0 Hosting Bundle** (includes the ASP.NET Core Module v2 for IIS):
   `https://dotnet.microsoft.com/download/dotnet/8.0` → "Hosting Bundle".
2. Restart IIS (or the server) after installing the hosting bundle so `aspnetcore_module_v2.dll`
   is picked up: `net stop was /y && net start w3svc`.
3. Enable the **Web Server (IIS)** role with **ASP.NET 4.8** and **CGI** feature (IIS uses the CGI
   feature's process-activation plumbing for the ASP.NET Core Module).

### 2.2 Publish

```powershell
dotnet publish src\NestHub.API\NestHub.API.csproj -c Release -o C:\Deploy\NestHub.API
dotnet publish src\NestHub.Admin\NestHub.Admin.csproj -c Release -o C:\Deploy\NestHub.Admin
```

Both projects have `<AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>` set, so publish
generates a `web.config` configured for in-process hosting automatically — no manual editing
required for the common case.

### 2.3 IIS site + Application Pool configuration

For **each** app (API and Admin), repeat:

1. **Application Pool**: Create a new pool (e.g. `NestHubApiPool` / `NestHubAdminPool`).
   - **.NET CLR version**: `No Managed Code` (the ASP.NET Core Module hosts the CLR itself).
   - **Managed pipeline mode**: `Integrated`.
   - **Identity**: a dedicated service account (or `ApplicationPoolIdentity`) with read access to
     the publish folder and network access to SQL Server.
   - **Idle Time-out**: set to `0` for the API pool if you need it always warm (SignalR
     connections and background work benefit from this); leave default for Admin.
   - **Start Mode**: `AlwaysRunning`, and enable `Preload Enabled` on the corresponding site, to
     avoid cold-start latency on the first request after a recycle.
2. **Site**: Add a new site (or an application under an existing site) pointing its physical path
   at the publish folder, bound to the desired host header/port, and assigned to the pool created
   above.
3. **HTTPS**: Bind a TLS certificate (443) — do not run either app over plain HTTP in production,
   since JWT bearer tokens and admin cookies must not travel unencrypted.
4. **Environment variables**: Set `ASPNETCORE_ENVIRONMENT=Production` and the connection
   string/JWT secret via IIS's "Configuration Editor" (`system.webServer/aspNetCore/environmentVariables`)
   or via machine-level environment variables — never leave the placeholder JWT secret from
   `appsettings.json` in production.
5. Grant the Application Pool identity **Modify** permission on the publish folder (IIS needs to
   write to `logs\` if stdout logging is enabled in `web.config`).

### 2.4 SignalR note (NestHub.API only)

The `/hubs/sos` endpoint uses WebSockets. Ensure the **WebSocket Protocol** Windows feature is
enabled (`Turn Windows features on or off` → `Internet Information Services` →
`World Wide Web Services` → `Application Development Features` → `WebSocket Protocol`), otherwise
SignalR falls back to slower long-polling.

### 2.5 Seeding demo data on the IIS-hosted API

`NestHub.API` supports a `--seed` command-line switch (see `DatabaseSeeder` in
`NestHub.Infrastructure\Persistence\Seed`) that populates realistic demo data across every table
— but IIS itself has no mechanism for passing custom arguments to the app it hosts (the ASP.NET
Core Module always starts it the normal way, listening for requests). To seed a deployed
environment, run the **published DLL directly from a console on the server**, pointing it at the
same configuration the IIS site uses. This is a one-off process that seeds the database and exits
immediately — it never binds a port, so it does not conflict with the IIS-hosted instance already
listening on that site.

```powershell
cd C:\Deploy\NestHub.API

# Point at the same environment/config the IIS site uses. If these are already set as
# machine-level environment variables (as configured in step 2.3.4), you can skip re-setting them.
$env:ASPNETCORE_ENVIRONMENT = "Production"
$env:ConnectionStrings__NestHubDatabase = "Server=<server>;Database=NestHub;User Id=<user>;Password=<password>;TrustServerCertificate=True;"

dotnet NestHub.API.dll --seed
```

Expected output ends with `Database seeded.` The seeder is idempotent — it checks whether any
`Societies` rows already exist and no-ops if so, so it's safe to leave this command in a runbook
without accidentally duplicating data on a second run.

> **Do not run `--seed` against a database that already has real user data** — while it won't
> touch existing rows (it only inserts, and only when the `Societies` table is empty), it's meant
> for empty staging/demo/UAT databases, not production once real residents and vendors have
> signed up. For a genuinely empty environment (fresh install, staging refresh, demo tenant) it's
> the fastest way to get a fully populated, walkable instance — see `TESTING.md` for the seeded
> login credentials and a feature-by-feature test script that assumes this data is present.

## 3. Mobile Publishing (.NET MAUI)

### 3.1 Target framework note

`NestHub.Mobile` targets `net9.0-android`, `net9.0-ios`, and `net9.0-maccatalyst` rather than
`net8.0-*`. At the time of this build, the installed MAUI tooling reports `net8.0-ios` and
`net8.0-maccatalyst` as out-of-support (Apple's minimum tooling requirements have moved ahead of
what the .NET 8 MAUI workload supports), so the mobile head must run on net9 even though the rest
of the solution (Domain/Application/Infrastructure/API/Admin) targets net8.0 as specified. This
is a platform-imposed constraint, not a design choice — check `dotnet workload list` before
downgrading.

### 3.2 Android — `.apk` / `.aab`

**Prerequisites**: Android SDK Platform matching the target API level installed (this project
build required a one-time `dotnet build -t:InstallAndroidDependencies`), and a signing keystore.

Generate a signing keystore once (keep it and its password secret and backed up — losing it means
you can never update the app on the Play Store under the same listing):

```powershell
keytool -genkeypair -v -keystore nesthub-release.keystore -alias nesthub -keyalg RSA -keysize 2048 -validity 10000
```

Build a Play Store **App Bundle** (preferred distribution format):

```powershell
dotnet publish src\NestHub.Mobile\NestHub.Mobile.csproj -f net9.0-android -c Release `
  -p:AndroidPackageFormat=aab `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore=nesthub-release.keystore `
  -p:AndroidSigningKeyAlias=nesthub `
  -p:AndroidSigningKeyPass=<keystore-password> `
  -p:AndroidSigningStorePass=<keystore-password>
```

Or a directly-installable **APK** (sideloading / internal distribution):

```powershell
dotnet publish src\NestHub.Mobile\NestHub.Mobile.csproj -f net9.0-android -c Release `
  -p:AndroidPackageFormat=apk `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore=nesthub-release.keystore `
  -p:AndroidSigningKeyAlias=nesthub `
  -p:AndroidSigningKeyPass=<keystore-password> `
  -p:AndroidSigningStorePass=<keystore-password>
```

Output lands under `src\NestHub.Mobile\bin\Release\net9.0-android\publish\`.

### 3.3 iOS — `.ipa`

Building and signing an iOS `.ipa` **requires a macOS build host** (Apple's toolchain — Xcode and
the iOS SDK — has no Windows equivalent). Windows-based development can still write and
compile-check the shared MAUI code, but the final archive/signing step must run on a Mac, either
physically, via Visual Studio's "Pair to Mac", or on a macOS CI runner (e.g. GitHub Actions
`macos-latest`, Azure Pipelines `macOS` pool, Codemagic, or App Center Build).

On a macOS host, with an Apple Developer account, a Distribution certificate, and a matching
provisioning profile installed:

```bash
dotnet publish src/NestHub.Mobile/NestHub.Mobile.csproj -f net9.0-ios -c Release \
  -p:ArchiveOnBuild=true \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:CodesignKey="Apple Distribution: <Your Team Name> (<TeamID>)" \
  -p:CodesignProvision="<Provisioning Profile Name>"
```

The resulting `.ipa` is written to `src/NestHub.Mobile/bin/Release/net9.0-ios/ios-arm64/publish/`
and can be uploaded via Transporter or `xcrun altool`/`notarytool` to App Store Connect.

### 3.4 CI/CD sketch

A typical pipeline: build/test Domain+Application+Infrastructure+API+Admin on any runner (Windows
or Linux); build the Android artifact on a Windows or Linux runner with the Android workload
installed; build the iOS artifact on a macOS runner. Gate all mobile publishes on the same
`NestHub.sln` build succeeding first, since Mobile references the shared `NestHub.Application`
project directly.

## 4. Configuration checklist before going live

- [ ] Replace the placeholder `Jwt:Secret` in `appsettings.json` with a long, random value stored
      as a secret (environment variable / Key Vault / IIS Configuration Editor), not in source control.
- [ ] Point `ConnectionStrings:NestHubDatabase` at the production SQL Server instance.
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production` for both API and Admin app pools.
- [ ] Confirm HTTPS bindings and valid certificates on both sites.
- [ ] Update `NestHub.Mobile`'s `MauiProgram.ApiBaseAddress` (currently defaulted for local
      Android-emulator development) to the deployed API's public HTTPS URL before publishing.
- [ ] Register at least one Admin user directly against the database (or temporarily relax the
      registration endpoint) since there is no public "become an admin" flow by design.
