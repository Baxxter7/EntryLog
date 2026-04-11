# External Integrations

**Analysis Date:** 2026-03-30

## APIs & External Services

**Email delivery:**
- Mailtrap - Transactional email sending for account recovery in `EntryLog.Business/Mailtrap/MailtrapApiService.cs` and `EntryLog.Business/Services/AppUserServices.cs`.
  - SDK/Client: named `HttpClient` registered in `EntryLog.Business/BusinessDependencies.cs` as `ApiNames.MailtrapIO`.
  - Auth: `MailtrapApiOptions:ApiToken` from `EntryLog.Api/appsettings.json` and `EntryLog.Web/appsettings.json`.

**Image hosting:**
- ImageBB - Stores uploaded work-session and face ID images in `EntryLog.Business/ImageBB/ImageBBService.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`, and `EntryLog.Business/Services/FaceIdService.cs`.
  - SDK/Client: named `HttpClient` registered in `EntryLog.Business/BusinessDependencies.cs` as `ApiNames.ImageBB`.
  - Auth: `ImageBBOptions:ApiToken` from `EntryLog.Api/appsettings.json` and `EntryLog.Web/appsettings.json`.

**Client-side face recognition assets:**
- face-api.js CDN - Browser-side facial detection and descriptor generation loaded from `EntryLog.Web/Views/Shared/_MainLayout.cshtml` and consumed by `EntryLog.Web/wwwroot/js/faceid.index.js` and `EntryLog.Web/wwwroot/js/work.session.js`.
  - SDK/Client: external script `face-api.js@0.22.2` plus local model files under `EntryLog.Web/wwwroot/lib/face-api-js/models/`.
  - Auth: Not applicable.

**Mapping and geocoding:**
- OpenStreetMap tile service - Map tiles rendered in `EntryLog.Web/wwwroot/js/main.index.js`.
  - SDK/Client: Leaflet-style `L.map` / `L.tileLayer` usage in `EntryLog.Web/wwwroot/js/main.index.js`.
  - Auth: none detected.
- Nominatim reverse geocoding - Browser fetch for latitude/longitude to address lookup in `EntryLog.Web/wwwroot/js/work.session.js`.
  - SDK/Client: native `fetch` in `EntryLog.Web/wwwroot/js/work.session.js`.
  - Auth: none detected.

**Browser device APIs:**
- Camera / media devices - Face capture in `EntryLog.Web/wwwroot/js/faceid.index.js` and `EntryLog.Web/wwwroot/js/work.session.js`.
  - SDK/Client: `navigator.mediaDevices.getUserMedia`.
  - Auth: browser permission prompt.
- Geolocation - User position capture in `EntryLog.Web/wwwroot/js/location.js` and `EntryLog.Web/wwwroot/js/work.session.js`.
  - SDK/Client: `navigator.geolocation.getCurrentPosition`.
  - Auth: browser permission prompt.

## Data Storage

**Databases:**
- SQL Server - Legacy employee lookup and relational data access in `EntryLog.Data/SqlLegacy/Contexts/EmployeesDbContext.cs` and `EntryLog.Data/SqlLegacy/Repositories/EmployeeRepository.cs`.
  - Connection: `ConnectionStrings:EmployeeDB` from `EntryLog.Api/appsettings*.json` and `EntryLog.Web/appsettings*.json`.
  - Client: Entity Framework Core SQL Server via `EntryLog.Data/DataDependencies.cs`.
- MongoDB - Application user and work-session persistence in `EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs` and `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs`.
  - Connection: `EntryLogDbOptions:ConnectionUri` and `EntryLogDbOptions:DatabaseName` from `EntryLog.Api/appsettings*.json` and `EntryLog.Web/appsettings*.json`.
  - Client: `MongoDB.Driver` configured in `EntryLog.Data/DataDependencies.cs`.

**File Storage:**
- Remote image hosting via ImageBB in `EntryLog.Business/ImageBB/ImageBBService.cs`.
- Local static asset storage for UI/vendor files and face models under `EntryLog.Web/wwwroot/`.

**Caching:**
- Server-side session only; no distributed cache or cache service detected. Session middleware is enabled in `EntryLog.Web/Program.cs`.

## Authentication & Identity

**Auth Provider:**
- Custom application auth.
  - Implementation: cookie authentication for MVC in `EntryLog.Web/Program.cs` and `EntryLog.Web/Extensions/HttpContextExtensions.cs`; password hashing via Argon2 in `EntryLog.Business/Cryptography/Argon2PasswordHasherService.cs`; short-lived JWT generation/validation for face-reference access in `EntryLog.Business/JWT/CustomBearerAuthentication.cs` and `EntryLog.Business/Services/FaceIdService.cs`.

## Monitoring & Observability

**Error Tracking:**
- None detected. No Sentry, Application Insights, or similar integration was found.

**Logs:**
- Default ASP.NET Core logging configuration via `Logging:LogLevel` in `EntryLog.Api/appsettings*.json` and `EntryLog.Web/appsettings*.json`.
- Browser-side diagnostics use `console.log` / `console.error` in `EntryLog.Web/wwwroot/js/faceid.index.js`, `EntryLog.Web/wwwroot/js/work.session.js`, and `EntryLog.Web/wwwroot/js/main.index.js`.

## CI/CD & Deployment

**Hosting:**
- Self-hosted or platform-hosted ASP.NET Core apps; concrete hosting platform config is not detected. App startup is defined in `EntryLog.Api/Program.cs` and `EntryLog.Web/Program.cs`.

**CI Pipeline:**
- None detected. No `.github/workflows/`, Dockerfiles, or pipeline manifests were found in the repo.

## Environment Configuration

**Required env vars:**
- Not environment-variable driven in the current repo state. Runtime secrets and endpoints are read from appsettings files instead.
- Required configuration keys are `ConnectionStrings:EmployeeDB`, `EntryLogDbOptions:ConnectionUri`, `EntryLogDbOptions:DatabaseName`, `ImageBBOptions:ApiUrl`, `ImageBBOptions:ApiToken`, `ImageBBOptions:ExpirationSeconds`, `MailtrapApiOptions:ApiUrl`, `MailtrapApiOptions:ApiToken`, `MailtrapApiOptions:FromEmail`, `MailtrapApiOptions:FromName`, `MailtrapApiOptions:Templates`, `Argon2PasswordHashOptions`, `EncryptionKeyValues`, and `JwtConfiguration:secret` as bound in `EntryLog.Business/BusinessDependencies.cs` and `EntryLog.Data/DataDependencies.cs`.

**Secrets location:**
- Sensitive configuration is currently committed in `EntryLog.Api/appsettings.json`, `EntryLog.Api/appsettings.Development.json`, `EntryLog.Web/appsettings.json`, and `EntryLog.Web/appsettings.Development.json`.
- No `.env` files were detected in the repository root.

## Webhooks & Callbacks

**Incoming:**
- None detected. No webhook receiver endpoints or callback controllers were found.

**Outgoing:**
- `POST /api/send` to Mailtrap from `EntryLog.Business/Mailtrap/MailtrapApiService.cs`.
- `POST /1/upload?...` to ImageBB from `EntryLog.Business/ImageBB/ImageBBService.cs`.
- `GET {imageUrl}` to retrieve hosted face images from `EntryLog.Business/Services/FaceIdService.cs`.
- `GET https://nominatim.openstreetmap.org/reverse?...` from `EntryLog.Web/wwwroot/js/work.session.js`.
- `GET https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png` from `EntryLog.Web/wwwroot/js/main.index.js`.

---

*Integration audit: 2026-03-30*
