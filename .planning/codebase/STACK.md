# Technology Stack

**Analysis Date:** 2026-03-30

## Languages

**Primary:**
- C# / .NET 8 - Backend and web application code in `EntryLog.Api/`, `EntryLog.Web/`, `EntryLog.Business/`, `EntryLog.Data/`, `EntryLog.Entities/`, and `tests/EntryLog.Tests/`.

**Secondary:**
- JavaScript - Browser-side behavior in `EntryLog.Web/wwwroot/js/` such as `EntryLog.Web/wwwroot/js/work.session.js`, `EntryLog.Web/wwwroot/js/faceid.index.js`, `EntryLog.Web/wwwroot/js/main.index.js`, and `EntryLog.Web/wwwroot/js/location.js`.
- Razor / HTML - MVC views and layout composition in `EntryLog.Web/Views/` such as `EntryLog.Web/Views/Shared/_MainLayout.cshtml`.
- JSON - Runtime configuration in `EntryLog.Api/appsettings*.json`, `EntryLog.Web/appsettings*.json`, and launch profiles in `EntryLog.Api/Properties/launchSettings.json` and `EntryLog.Web/Properties/launchSettings.json`.

## Runtime

**Environment:**
- .NET SDK / ASP.NET Core targeting `net8.0` in `EntryLog.Api/EntryLog.Api.csproj`, `EntryLog.Web/EntryLog.Web.csproj`, `EntryLog.Business/EntryLog.Business.csproj`, `EntryLog.Data/EntryLog.Data.csproj`, `EntryLog.Entities/EntryLog.Entities.csproj`, and `tests/EntryLog.Tests/EntryLog.Tests.csproj`.

**Package Manager:**
- NuGet via SDK-style project files.
- Lockfile: missing (no `packages.lock.json`, `Directory.Packages.props`, or `global.json` detected in the repo root).

## Frameworks

**Core:**
- ASP.NET Core Web API - API host in `EntryLog.Api/Program.cs` with controllers in `EntryLog.Api/Controllers/`.
- ASP.NET Core MVC - Server-rendered web app in `EntryLog.Web/Program.cs` with controllers in `EntryLog.Web/Controllers/` and views in `EntryLog.Web/Views/`.
- ASP.NET Core Dependency Injection / Options pattern - Service registration and config binding in `EntryLog.Business/BusinessDependencies.cs` and `EntryLog.Data/DataDependencies.cs`.

**Testing:**
- xUnit `2.9.3` - Test runner in `tests/EntryLog.Tests/EntryLog.Tests.csproj`.
- FluentAssertions `8.3.0` - Assertion style in `tests/EntryLog.Tests/EntryLog.Tests.csproj`.
- NSubstitute `5.3.0` - Mocking library in `tests/EntryLog.Tests/EntryLog.Tests.csproj`.
- coverlet.collector `6.0.4` - Coverage collection in `tests/EntryLog.Tests/EntryLog.Tests.csproj`.

**Build/Dev:**
- Swashbuckle.AspNetCore `6.6.2` - Swagger/OpenAPI generation in `EntryLog.Api/EntryLog.Api.csproj` and `EntryLog.Api/Program.cs`.
- ASP.NET Core launch profiles - Local development endpoints in `EntryLog.Api/Properties/launchSettings.json` and `EntryLog.Web/Properties/launchSettings.json`.

## Key Dependencies

**Critical:**
- `Konscious.Security.Cryptography.Argon2` `1.3.1` - Password hashing in `EntryLog.Business/Cryptography/Argon2PasswordHasherService.cs`.
- `MongoDB.Driver` `3.5.0` - MongoDB persistence in `EntryLog.Data/DataDependencies.cs`, `EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs`, and `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs`.
- `Microsoft.EntityFrameworkCore.SqlServer` `8.0.22` - SQL Server access for legacy employee data in `EntryLog.Data/DataDependencies.cs` and `EntryLog.Data/SqlLegacy/Contexts/EmployeesDbContext.cs`.

**Infrastructure:**
- `Microsoft.AspNetCore.App` framework reference - ASP.NET Core shared framework used by `EntryLog.Business/EntryLog.Business.csproj`.
- `Microsoft.EntityFrameworkCore` `8.0.22` - ORM base for SQL layer in `EntryLog.Data/EntryLog.Data.csproj`.
- `Swashbuckle.AspNetCore` `6.6.2` - Interactive API documentation in `EntryLog.Api/Program.cs`.

## Configuration

**Environment:**
- Runtime configuration is loaded from `EntryLog.Api/appsettings.json`, `EntryLog.Api/appsettings.Development.json`, `EntryLog.Web/appsettings.json`, and `EntryLog.Web/appsettings.Development.json`.
- Business options are bound in `EntryLog.Business/BusinessDependencies.cs` for `Argon2PasswordHashOptions`, `EncryptionKeyValues`, `MailtrapApiOptions`, `ImageBBOptions`, and `JwtConfiguration`.
- Data options are bound in `EntryLog.Data/DataDependencies.cs` for SQL connection string `ConnectionStrings:EmployeeDB` and Mongo settings under `EntryLogDbOptions`.
- Sensitive material is currently stored directly in appsettings files under keys such as `ConnectionStrings:EmployeeDB`, `EntryLogDbOptions:ConnectionUri`, `ImageBBOptions:ApiToken`, `MailtrapApiOptions:ApiToken`, `EncryptionKeyValues:PublicKey`, `EncryptionKeyValues:PrivateKey`, and `JwtConfiguration:secret`; refer to keys and file paths only, never copy values.

**Build:**
- Solution entry point: `EntryLog.slnx`.
- Project manifests: `EntryLog.Api/EntryLog.Api.csproj`, `EntryLog.Web/EntryLog.Web.csproj`, `EntryLog.Business/EntryLog.Business.csproj`, `EntryLog.Data/EntryLog.Data.csproj`, `EntryLog.Entities/EntryLog.Entities.csproj`, and `tests/EntryLog.Tests/EntryLog.Tests.csproj`.
- No repo-level Docker, CI workflow, or central package management files were detected.

## Platform Requirements

**Development:**
- .NET 8 SDK to build and run all projects referenced by `EntryLog.slnx`.
- SQL Server reachable through `ConnectionStrings:EmployeeDB` configured in `EntryLog.Api/appsettings.json` and `EntryLog.Web/appsettings.json`.
- MongoDB reachable through `EntryLogDbOptions` configured in `EntryLog.Api/appsettings.json` and `EntryLog.Web/appsettings.json`.
- Internet access for third-party HTTP integrations used by `EntryLog.Business/Mailtrap/MailtrapApiService.cs`, `EntryLog.Business/ImageBB/ImageBBService.cs`, and browser-side calls in `EntryLog.Web/wwwroot/js/work.session.js`.
- Browser camera and geolocation permissions for flows in `EntryLog.Web/wwwroot/js/faceid.index.js`, `EntryLog.Web/wwwroot/js/work.session.js`, and `EntryLog.Web/wwwroot/js/location.js`.

**Production:**
- Hosting target is ASP.NET Core web hosting for separate API and MVC apps, as implied by `EntryLog.Api/Program.cs` and `EntryLog.Web/Program.cs`.
- Local launch settings expose API on `http://localhost:5110` / `https://localhost:7026` and MVC on `http://localhost:5171` / `https://localhost:7179` via `EntryLog.Api/Properties/launchSettings.json` and `EntryLog.Web/Properties/launchSettings.json`.

---

*Stack analysis: 2026-03-30*
