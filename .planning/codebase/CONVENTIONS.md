# Coding Conventions

**Analysis Date:** 2026-03-30

## Naming Patterns

**Files:**
- Keep one primary type per file and match the filename to that type, e.g. `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Interfaces/IAppUserServices.cs`, `EntryLog.Entities/POCOEntities/AppUser.cs`.
- Test files use `{Subject}Tests.cs`, grouped by production area, e.g. `tests/EntryLog.Tests/Business/Services/FaceIdServiceTests.cs` and `tests/EntryLog.Tests/Web/Controllers/AccountControllerTests.cs`.
- DTO files use `*Dto.cs`, e.g. `EntryLog.Business/DTOs/CreateWorkSessionDto.cs`, `EntryLog.Business/DTOs/LoginResponseDto.cs`.

**Functions:**
- Public and private methods use PascalCase, including async methods with `Async` suffix: `RegisterEmployeeAsync`, `GenerateReferenceImageTokenAsync`, `ValidateEmployeeUserAsync` in `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Services/FaceIdService.cs`, and `EntryLog.Business/Services/WorkSessionServices.cs`.
- Test method names follow `Method_Scenario_ExpectedResult`, e.g. `OpenJobSessionAsync_InvalidDescriptor_ReturnsFalse` in `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs`.
- Builder helper methods use fluent `WithX` names, e.g. `WithCode`, `WithFaceId`, `WithStatus` in `tests/EntryLog.Tests/Helpers/AppUserBuilder.cs` and `tests/EntryLog.Tests/Helpers/WorkSessionBuilder.cs`.

**Variables:**
- Private fields use `_camelCase`, e.g. `_appUserRepository` in `EntryLog.Business/Services/AppUserServices.cs` and `_faceIdService` in `EntryLog.Web/Controllers/FaceIdController.cs`.
- Local variables generally use `camelCase` with explicit domain names such as `recoveryTokenPlain`, `imageBBUrl`, `claimsPrincipal`, and `faceIdDto`.
- Tuple deconstruction uses semantic names like `(bool success, string message, LoginResponseDto? data)` in `EntryLog.Business/Services/AppUserServices.cs` and controller actions in `EntryLog.Api/Controllers/AccountController.cs`.
- Existing exceptions to casing should be preserved when editing adjacent code: `CreateWorkSessionDto.descriptor` in `EntryLog.Business/DTOs/CreateWorkSessionDto.cs` and `AddEmployeeFaceIdDto.image` in `EntryLog.Business/DTOs/AddEmployeeFaceIdDto.cs` are lower-case members already part of the current contract.

**Types:**
- Interfaces use `I` prefix, e.g. `IAppUserServices`, `IWorkSessionRepository`, `IJwtService` in `EntryLog.Business/Interfaces` and `EntryLog.Data/Interfaces`.
- Service implementations use `*Service` or `*Services`, e.g. `FaceIdService`, `AppUserServices`, `WorkSessionServices` in `EntryLog.Business/Services`.
- DTOs are `record`/`record class` types in `EntryLog.Business/DTOs`, while persistence models stay mutable POCO `class` types in `EntryLog.Entities/POCOEntities`.

## Code Style

**Formatting:**
- No repo-specific formatter config was detected: no `.editorconfig`, `.eslintrc*`, `.prettierrc*`, or equivalent files were found at `C:\Users\NEW\source\repos\EntryLog`.
- Use 4-space indentation and keep current local spacing/alignment.
- Prefer file-scoped namespaces where the file already uses them, e.g. `EntryLog.Business/Services/FaceIdService.cs` and `EntryLog.Web/Controllers/WorkSessionController.cs`.
- Preserve block namespaces where the local file already uses them, e.g. `EntryLog.Web/Controllers/AccountController.cs`.

**Linting:**
- No Roslyn analyzer package, StyleCop package, or standalone lint config was detected in `EntryLog.Api/EntryLog.Api.csproj`, `EntryLog.Web/EntryLog.Web.csproj`, `EntryLog.Business/EntryLog.Business.csproj`, `EntryLog.Data/EntryLog.Data.csproj`, or `tests/EntryLog.Tests/EntryLog.Tests.csproj`.
- `net8.0`, nullable reference types, and implicit usings are enabled in `tests/EntryLog.Tests/EntryLog.Tests.csproj`; the same runtime baseline is used across project files.
- In practice, consistency is enforced by matching nearby code rather than by tooling rules.

## Import Organization

**Order:**
1. Project/solution namespaces first, e.g. `using EntryLog.Business.DTOs;` in `EntryLog.Business/Services/WorkSessionServices.cs`.
2. Framework/library namespaces after project imports, e.g. `using Microsoft.AspNetCore.Mvc;` in `EntryLog.Web/Controllers/FaceIdController.cs`.
3. No blank-line-separated ordering convention is enforced beyond keeping all `using` directives at the top of the file.

**Path Aliases:**
- Not used. Imports use full C# namespaces such as `EntryLog.Business.Interfaces`, `EntryLog.Data.Interfaces`, and `EntryLog.Web.Extensions`.

## Error Handling

**Patterns:**
- Application services return tuples instead of throwing for expected validation/business failures:
  - `Task<(bool success, string message)>` in `EntryLog.Business/Interfaces/IAppUserServices.cs`
  - `Task<(bool success, string message, GetWorkSessionDto? data)>` in `EntryLog.Business/Services/WorkSessionServices.cs`
  - `Task<(bool success, string message, EmployeeFaceIdDto? data)>` in `EntryLog.Business/Services/FaceIdService.cs`
- Use guard clauses first and return immediately on invalid input, e.g. token/password checks in `EntryLog.Business/Services/AppUserServices.cs:34-60` and employee/session checks in `EntryLog.Business/Services/WorkSessionServices.cs:126-155`.
- Catch infrastructure failures close to the I/O boundary and convert them to user-facing messages, e.g. image upload failures in `EntryLog.Business/Services/FaceIdService.cs:81-104` and `EntryLog.Business/Services/WorkSessionServices.cs:58-69,162-173`.
- Throw only for programmer/invariant issues in lower-level utilities, e.g. `ArgumentOutOfRangeException` in `EntryLog.Data/Specifications/Specification.cs` and `CryptographicException` in `EntryLog.Business/Cryptography/RsaAsymmetricEncryptionService.cs`.

## Logging

**Framework:** None detected.

**Patterns:**
- No `ILogger<T>`, Serilog, or structured logging setup was detected in `EntryLog.Api/Program.cs`, `EntryLog.Web/Program.cs`, `EntryLog.Business/BusinessDependencies.cs`, or `EntryLog.Data/DataDependencies.cs`.
- Do not introduce ad-hoc `Console.WriteLine` logging into established flows; there is no surrounding logging convention to align with.

## Comments

**When to Comment:**
- Comments are sparse. Prefer self-descriptive names and early-return logic over explanatory comments.
- Existing inline comments are short and sometimes Spanish, e.g. `//Configuraciones`, `//Servicios de infraestructura`, `//Servicios de aplicacion` in `EntryLog.Business/BusinessDependencies.cs` and `//Loguear al empleado` in `EntryLog.Web/Controllers/AccountController.cs`.
- Add comments only when the intent is not obvious from the code.

**JSDoc/TSDoc:**
- Not applicable. No XML documentation comment convention is in active use across `EntryLog.Api`, `EntryLog.Web`, `EntryLog.Business`, `EntryLog.Data`, or `tests/EntryLog.Tests`.

## Function Design

**Size:**
- Keep controller actions thin and delegate business rules to services, as in `EntryLog.Api/Controllers/AccountController.cs`, `EntryLog.Api/Controllers/JobSessionController.cs`, and `EntryLog.Web/Controllers/WorkSessionController.cs`.
- Service methods can be medium-sized when they own a full use case, but they still favor sequential guard clauses over deep nesting, e.g. `RegisterEmployeeAsync` and `OpenJobSessionAsync`.

**Parameters:**
- Prefer constructor injection for collaborators, e.g. `FaceIdService` in `EntryLog.Business/Services/FaceIdService.cs` and `AccountController` in `EntryLog.Web/Controllers/AccountController.cs`.
- Use DTO records for action/service inputs, e.g. `CreateEmployeeUserDto`, `CreateWorkSessionDto`, `CloseJobSessionDto`, and `AddEmployeeFaceIdDto` in `EntryLog.Business/DTOs`.
- Use `[FromBody]`, `[FromForm]`, and `[FromQuery]` explicitly on controller actions when binding non-trivial inputs, as shown in `EntryLog.Api/Controllers/AccountController.cs` and `EntryLog.Api/Controllers/JobSessionController.cs`.

**Return Values:**
- Business layer methods usually return tuples with `success` and `message`, optionally plus `data`.
- API controllers wrap tuple outputs in `Ok(new { success, message, ... })`, e.g. `EntryLog.Api/Controllers/AccountController.cs` and `EntryLog.Api/Controllers/JobSessionController.cs`.
- MVC controllers return `Json(new { success, message, data/path })` for AJAX-style POSTs and `View(...)`/`RedirectToAction(...)` for page navigation, e.g. `EntryLog.Web/Controllers/AccountController.cs` and `EntryLog.Web/Controllers/FaceIdController.cs`.

## Module Design

**Exports:**
- Keep cross-project contracts public and internal implementation details internal:
  - Public interfaces in `EntryLog.Business/Interfaces` and `EntryLog.Data/Interfaces`
  - Internal service implementations in `EntryLog.Business/Services`
  - Public DTO records in `EntryLog.Business/DTOs`
  - Public entities in `EntryLog.Entities/POCOEntities`
- Use static helper classes for mapping/extensions rather than instance mappers where that pattern already exists, e.g. `EntryLog.Business/Mappers/FaceIdMapper.cs`, `EntryLog.Business/Mappers/WorkSessionMapper.cs`, `EntryLog.Web/Extensions/HttpContextExtensions.cs`.

**Barrel Files:**
- Not used. Namespaces and project references are used directly; there are no aggregator files beyond DI registration roots like `EntryLog.Business/BusinessDependencies.cs` and `EntryLog.Data/DataDependencies.cs`.

## Dependency Injection Patterns

**Registration style:**
- Register dependencies through extension methods on `IServiceCollection`:
  - `EntryLog.Business/BusinessDependencies.cs`
  - `EntryLog.Data/DataDependencies.cs`
- Root apps compose layers in `EntryLog.Api/Program.cs` and `EntryLog.Web/Program.cs` using `.AddDataServices(builder.Configuration).AddBusinessServices(builder.Configuration)`.

**Lifetime choices:**
- `AddScoped` is the default for repositories and application services, e.g. `IAppUserServices`, `IWorkSessionServices`, `IFaceIdService`, `IAppUserRepository`, and `IWorkSessionRepository`.
- `AddSingleton` is reserved for stateless crypto service `IEncryptionService` in `EntryLog.Business/BusinessDependencies.cs`.
- Named `HttpClient` registrations are configured in `EntryLog.Business/BusinessDependencies.cs` for external APIs and plain `IHttpClientFactory` is injected into `EntryLog.Business/Services/FaceIdService.cs`.

**Configuration binding:**
- Bind options from configuration sections with `services.Configure<T>(configuration.GetSection(nameof(T)))`, e.g. `Argon2PasswordHashOptions`, `EncryptionKeyValues`, `MailtrapApiOptions`, `ImageBBOptions`, and `JwtConfiguration` in `EntryLog.Business/BusinessDependencies.cs`.

## Response and Validation Patterns

**HTTP responses:**
- Use anonymous-object payloads with lowercase `success` and `message` keys in both API and MVC controllers.
- For successful MVC authentication flows, return a `path` property instead of embedding redirect logic in JavaScript consumers, as in `EntryLog.Web/Controllers/AccountController.cs:44-48` and `:81-85`.
- For unauthorized authenticated-endpoint failures, return `Unauthorized()` rather than tuple-wrapping the error, as in `EntryLog.Web/Controllers/FaceIdController.cs:43-50` and `:56-62`.

**Validation:**
- Validation is mostly manual and inline; no FluentValidation or data-annotation-heavy validation layer was detected.
- Expect services to validate parseability (`int.TryParse`), required strings, collection sizes, and repository existence checks before mutating state, e.g. `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Services/FaceIdService.cs`, and `EntryLog.Business/Services/WorkSessionServices.cs`.
- MVC POST actions that render pages use `[ValidateAntiForgeryToken]` in `EntryLog.Web/Controllers/AccountController.cs`; JSON/form endpoints like `EntryLog.Web/Controllers/FaceIdController.cs` and `EntryLog.Web/Controllers/WorkSessionController.cs` currently do not add anti-forgery validation.

---

*Convention analysis: 2026-03-30*
