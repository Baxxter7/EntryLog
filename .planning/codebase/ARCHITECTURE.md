# Architecture

**Analysis Date:** 2026-03-30

## Pattern Overview

**Overall:** Layered ASP.NET Core solution with two presentation hosts, a shared application/business layer, a shared data-access layer, and a standalone domain model layer.

**Key Characteristics:**
- Two independent entry applications reuse the same service and repository graph: `EntryLog.Api/Program.cs` and `EntryLog.Web/Program.cs`.
- Dependency direction is one-way through project references: `EntryLog.Api/EntryLog.Api.csproj` and `EntryLog.Web/EntryLog.Web.csproj` → `EntryLog.Business/EntryLog.Business.csproj` → `EntryLog.Data/EntryLog.Data.csproj` → `EntryLog.Entities/EntryLog.Entities.csproj`.
- Persistence is split by concern: employee master data comes from SQL via `EntryLog.Data/SqlLegacy/`, while users, Face ID state, and work sessions live in MongoDB via `EntryLog.Data/MongoDB/`.

## Layers

**Presentation - API:**
- Purpose: Expose JSON/form-based endpoints for account and work-session operations.
- Location: `EntryLog.Api/`
- Contains: Bootstrap in `EntryLog.Api/Program.cs` and controllers in `EntryLog.Api/Controllers/AccountController.cs` and `EntryLog.Api/Controllers/JobSessionController.cs`.
- Depends on: `EntryLog.Business` service interfaces and DTOs.
- Used by: External API clients.

**Presentation - MVC Web:**
- Purpose: Serve authenticated employee UI, views, and browser-oriented endpoints.
- Location: `EntryLog.Web/`
- Contains: Bootstrap in `EntryLog.Web/Program.cs`, controllers in `EntryLog.Web/Controllers/*.cs`, Razor views in `EntryLog.Web/Views/**`, claim/cookie helpers in `EntryLog.Web/Extensions/*.cs`, and UI view models in `EntryLog.Web/Models/*.cs`.
- Depends on: `EntryLog.Business` service interfaces, ASP.NET Core cookie auth, and local view models.
- Used by: Browser users signing in and managing Face ID/work sessions.

**Application / Business:**
- Purpose: Orchestrate use cases, validation, DTO shaping, image upload, email sending, cryptography, JWT issuance, and request-context extraction.
- Location: `EntryLog.Business/`
- Contains: Dependency registration in `EntryLog.Business/BusinessDependencies.cs`, use-case services in `EntryLog.Business/Services/*.cs`, contracts in `EntryLog.Business/Interfaces/*.cs`, DTOs in `EntryLog.Business/DTOs/*.cs`, mapping helpers in `EntryLog.Business/Mappers/*.cs`, pagination/filter/spec helpers in `EntryLog.Business/Pagination/`, `EntryLog.Business/QueryFilters/`, and `EntryLog.Business/Specs/`.
- Depends on: Repository interfaces from `EntryLog.Data/Interfaces/*.cs`, entity types from `EntryLog.Entities/POCOEntities/*.cs`, ASP.NET abstractions, and external-service adapters implemented inside `EntryLog.Business/ImageBB/`, `EntryLog.Business/Mailtrap/`, `EntryLog.Business/Cryptography/`, and `EntryLog.Business/JWT/`.
- Used by: Both presentation projects.

**Data Access:**
- Purpose: Translate application requests into MongoDB and SQL operations.
- Location: `EntryLog.Data/`
- Contains: Service registration in `EntryLog.Data/DataDependencies.cs`, repository interfaces in `EntryLog.Data/Interfaces/*.cs`, Mongo repositories in `EntryLog.Data/MongoDB/Repositories/*.cs`, Mongo serializers in `EntryLog.Data/MongoDB/Serializers/*.cs`, SQL EF Core context/configs in `EntryLog.Data/SqlLegacy/Contexts/EmployeesDbContext.cs` and `EntryLog.Data/SqlLegacy/Configs/*.cs`, and specification infrastructure in `EntryLog.Data/Specifications/*.cs` plus `EntryLog.Data/Evaluators/SpecificationEvaluator.cs`.
- Depends on: `EntryLog.Entities` only.
- Used by: `EntryLog.Business`.

**Domain Model:**
- Purpose: Hold persistence-oriented entity and enum definitions shared across the solution.
- Location: `EntryLog.Entities/`
- Contains: POCOs in `EntryLog.Entities/POCOEntities/*.cs` and enums in `EntryLog.Entities/Enums/*.cs`.
- Depends on: No internal project.
- Used by: `EntryLog.Data`, transitively by `EntryLog.Business`.

**Test Harness:**
- Purpose: Validate business, data-specification, and web-controller behavior without changing production layering.
- Location: `tests/EntryLog.Tests/`
- Contains: Test suites mirrored by subsystem under `tests/EntryLog.Tests/Business/`, `tests/EntryLog.Tests/Data/`, and `tests/EntryLog.Tests/Web/`, plus builders in `tests/EntryLog.Tests/Helpers/*.cs`.
- Depends on: `EntryLog.Business`, `EntryLog.Data`, `EntryLog.Entities`, and `EntryLog.Web` via `tests/EntryLog.Tests/EntryLog.Tests.csproj`.
- Used by: `dotnet test EntryLog.slnx`.

## Data Flow

**Account registration and login flow:**

1. Requests enter through `EntryLog.Api/Controllers/AccountController.cs` or `EntryLog.Web/Controllers/AccountController.cs`.
2. Controllers call `IAppUserServices` implemented by `EntryLog.Business/Services/AppUserServices.cs`.
3. `AppUserServices` reads employee records through `EntryLog.Data/Interfaces/IEmployeeRepository.cs` (`EntryLog.Data/SqlLegacy/Repositories/EmployeeRepository.cs`) and user records through `EntryLog.Data/Interfaces/IAppUserRepository.cs` (`EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs`).
4. Password hashing, recovery-token encryption, email dispatch, and URL generation are delegated to `EntryLog.Business/Cryptography/Argon2PasswordHasherService.cs`, `EntryLog.Business/Cryptography/RsaAsymmetricEncryptionService.cs`, `EntryLog.Business/Mailtrap/MailtrapApiService.cs`, and `EntryLog.Business/Infrastructure/UriService.cs`.
5. API endpoints return anonymous JSON payloads; MVC endpoints additionally persist cookie claims through `EntryLog.Web/Extensions/HttpContextExtensions.cs`.

**Work-session check-in/check-out flow:**

1. Requests enter through `EntryLog.Api/Controllers/JobSessionController.cs` or `EntryLog.Web/Controllers/WorkSessionController.cs`.
2. Payloads are normalized into `EntryLog.Business/DTOs/CreateWorkSessionDto.cs`, `EntryLog.Business/DTOs/CloseJobSessionDto.cs`, and `EntryLog.Business/QueryFilters/WorkSessionQueryFilter.cs`.
3. `EntryLog.Business/Services/WorkSessionServices.cs` validates employee/user existence across SQL and Mongo repositories, parses face descriptors, uploads images via `EntryLog.Business/ImageBB/ImageBBService.cs`, and enriches request metadata with `EntryLog.Business/Infrastructure/UriService.cs`.
4. Work-session persistence goes through `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs` backed by serializer setup in `EntryLog.Data/MongoDB/Serializers/WorkSessionSerializer.cs`.
5. Query results are filtered with `EntryLog.Business/Specs/WorkSessionSpec.cs` + `EntryLog.Data/Specifications/Specification.cs`, then mapped to DTOs by `EntryLog.Business/Mappers/WorkSessionMapper.cs` and wrapped by `EntryLog.Business/Pagination/PaginatedResult.cs`.

**Face ID enrollment and reference-image flow:**

1. Browser requests enter `EntryLog.Web/Controllers/FaceIdController.cs`.
2. Authenticated user identity is read from cookie claims via `EntryLog.Web/Extensions/ClaimsPrincipalExtensions.cs`.
3. `EntryLog.Business/Services/FaceIdService.cs` loads both employee and user records, uploads the source image through `EntryLog.Business/ImageBB/ImageBBService.cs`, stores descriptor/image metadata inside `EntryLog.Entities/POCOEntities\FaceID.cs` on the `EntryLog.Entities/POCOEntities/AppUser.cs` document, and returns DTOs via `EntryLog.Business/Mappers/FaceIdMapper.cs`.
4. Short-lived reference-image tokens are generated and validated through `EntryLog.Business/JWT/CustomBearerAuthentication.cs` and `EntryLog.Business/JWT/JwtValidator.cs`.
5. Reference-image retrieval reuses the stored remote image URL and converts it to base64 inside `FaceIdService.GenerateBase64PngImageAsync` in `EntryLog.Business/Services/FaceIdService.cs`.

**State Management:**
- Request state is mostly stateless and passed through DI-scoped services.
- MVC authentication state is cookie-based using claims set in `EntryLog.Web/Extensions/HttpContextExtensions.cs` and read in `EntryLog.Web/Extensions/ClaimsPrincipalExtensions.cs`.
- Persistent business state is split between Mongo documents (`EntryLog.Entities/POCOEntities/AppUser.cs`, `EntryLog.Entities/POCOEntities/WorkSession.cs`) and SQL entities (`EntryLog.Entities/POCOEntities/Employee.cs`, `EntryLog.Entities/POCOEntities/Position.cs`).

## Key Abstractions

**Service interfaces:**
- Purpose: Keep presentation projects coupled to application contracts instead of implementations.
- Examples: `EntryLog.Business/Interfaces/IAppUserServices.cs`, `EntryLog.Business/Interfaces/IWorkSessionServices.cs`, `EntryLog.Business/Interfaces/IFaceIdService.cs`.
- Pattern: Public interfaces plus internal implementations registered in `EntryLog.Business/BusinessDependencies.cs`.

**Repository interfaces:**
- Purpose: Hide storage technology details from the business layer.
- Examples: `EntryLog.Data/Interfaces/IAppUserRepository.cs`, `EntryLog.Data/Interfaces/IWorkSessionRepository.cs`, `EntryLog.Data/Interfaces/IEmployeeRepository.cs`.
- Pattern: Interface in `EntryLog.Data/Interfaces/` with technology-specific implementation in `EntryLog.Data/MongoDB/Repositories/` or `EntryLog.Data/SqlLegacy/Repositories/`.

**DTO boundary:**
- Purpose: Keep HTTP payloads and UI contracts separate from persistence entities.
- Examples: `EntryLog.Business/DTOs/CreateEmployeeUserDto.cs`, `EntryLog.Business/DTOs/CreateWorkSessionDto.cs`, `EntryLog.Business/DTOs/GetWorkSessionDto.cs`, `EntryLog.Business/DTOs/LoginResponseDto.cs`.
- Pattern: Record-based transport models consumed by controllers and produced by services.

**Specification pipeline:**
- Purpose: Compose filtering, sorting, and paging for repository queries.
- Examples: `EntryLog.Business/Specs/WorkSessionSpec.cs`, `EntryLog.Data/Specifications/Specification.cs`, `EntryLog.Data/Evaluators/SpecificationEvaluator.cs`.
- Pattern: Mutable specification object configured in business code and executed against Mongo `IQueryable` in repository helpers.

**Serializer/config mapping layer:**
- Purpose: Bridge domain POCOs to two different persistence schemas.
- Examples: `EntryLog.Data/MongoDB/Serializers/AppUserSerializer.cs`, `EntryLog.Data/MongoDB/Serializers/WorkSessionSerializer.cs`, `EntryLog.Data/SqlLegacy/Configs/EmployeeConfig.cs`.
- Pattern: Central bootstrap at DI registration time followed by repository usage.

## Entry Points

**API host:**
- Location: `EntryLog.Api/Program.cs`
- Triggers: `dotnet run --project EntryLog.Api/EntryLog.Api.csproj`
- Responsibilities: Build the API container, register `AddDataServices` and `AddBusinessServices`, enable Swagger in development, and map controller endpoints.

**Web host:**
- Location: `EntryLog.Web/Program.cs`
- Triggers: `dotnet run --project EntryLog.Web/EntryLog.Web.csproj`
- Responsibilities: Configure cookie authentication, session, MVC routing, static-file serving, and the shared business/data dependency graph.

**API account endpoints:**
- Location: `EntryLog.Api/Controllers/AccountController.cs`
- Triggers: `POST api/account/*`
- Responsibilities: Registration, login, and account-recovery JSON APIs.

**API work-session endpoints:**
- Location: `EntryLog.Api/Controllers/JobSessionController.cs`
- Triggers: `POST api/jobsession/open`, `POST api/jobsession/close`, `POST api/jobsession/filter`
- Responsibilities: Open/close work sessions and query paginated history.

**MVC account flow:**
- Location: `EntryLog.Web/Controllers/AccountController.cs`
- Triggers: `/account/login`, `/account/registeremployeeuser`
- Responsibilities: Render login/registration views and convert successful login responses into auth cookies.

**MVC employee flow:**
- Location: `EntryLog.Web/Controllers/WorkSessionController.cs`, `EntryLog.Web/Controllers/FaceIdController.cs`, `EntryLog.Web/Controllers/MainController.cs`
- Triggers: Authenticated browser navigation and XHR/form posts.
- Responsibilities: Render employee pages, submit work-session operations, manage Face ID enrollment, and serve short-lived reference-image data.

## Error Handling

**Strategy:** Business services perform guard-clause validation and return tuple-based results instead of throwing domain-specific exceptions; controllers mostly serialize those tuples directly.

**Patterns:**
- Validate early and return `(false, message, null)` or `(false, message)` inside services such as `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`, and `EntryLog.Business/Services/FaceIdService.cs`.
- Wrap infrastructure calls in broad `try/catch` blocks where failure should be flattened into user-facing messages, such as image uploads and token parsing in `EntryLog.Business/Services/WorkSessionServices.cs`, `EntryLog.Business/Services/FaceIdService.cs`, and `EntryLog.Business/Services/AppUserServices.cs`.
- MVC authentication failures fall back to redirect/view/json behavior in `EntryLog.Web/Controllers/AccountController.cs`; API failures still return HTTP 200 with `{ success, message }` envelopes in `EntryLog.Api/Controllers/*.cs`.

## Cross-Cutting Concerns

**Logging:** Minimal explicit logging. The only injected logger observed is `EntryLog.Web/Controllers/HomeController.cs`; core business and data flows do not centralize structured logging.

**Validation:** Inline validation is concentrated inside application services and controller model binding. There is no separate validation layer such as FluentValidation; use-case methods in `EntryLog.Business/Services/*.cs` are the current validation boundary.

**Authentication:**
- Browser auth uses cookie authentication configured in `EntryLog.Web/Program.cs` and claims helpers in `EntryLog.Web/Extensions/*.cs`.
- Face ID reference-image access uses custom JWT generation/validation through `EntryLog.Business/JWT/CustomBearerAuthentication.cs`.
- The API host in `EntryLog.Api/Program.cs` does not configure authentication middleware beyond `UseAuthorization()`, so API controllers currently rely on open endpoints.

**Subsystem boundaries:**
- `EntryLog.Api/` should stay viewless and only speak DTOs/service interfaces.
- `EntryLog.Web/` owns Razor views and browser auth concerns; keep cookie claims and view models there.
- `EntryLog.Business/` is the orchestration boundary for new use cases, external HTTP clients, and mapping.
- `EntryLog.Data/` is the only layer that should know Mongo serializer details, EF Core config, collection names, or SQL table mapping.
- `EntryLog.Entities/` should remain persistence-agnostic POCOs/enums without service logic.

---

*Architecture analysis: 2026-03-30*
