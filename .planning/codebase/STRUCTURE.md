# Codebase Structure

**Analysis Date:** 2026-03-30

## Directory Layout

```text
EntryLog/
├── EntryLog.Api/           # ASP.NET Core Web API host and HTTP controllers
├── EntryLog.Web/           # ASP.NET Core MVC host, views, auth helpers, static assets
├── EntryLog.Business/      # Application services, DTOs, adapters, mapping, pagination
├── EntryLog.Data/          # Repository interfaces, MongoDB/SQL implementations, specs
├── EntryLog.Entities/      # Shared POCO entities and enums
├── tests/                  # xUnit test project and helper builders
├── .planning/codebase/     # Generated codebase-mapping documents
├── docs/superpowers/       # Planning/spec notes used by other agent workflows
├── .github/workflows/      # CI workflow directory exists but is currently empty
├── AGENTS.md               # Repo-specific agent guidance
├── Reame.md                # Root readme file (note current spelling)
└── EntryLog.slnx           # Solution entry point for all projects
```

## Directory Purposes

**EntryLog.Api:**
- Purpose: API-only host.
- Contains: `Program.cs` plus controllers in `EntryLog.Api/Controllers/`.
- Key files: `EntryLog.Api/Program.cs`, `EntryLog.Api/Controllers/AccountController.cs`, `EntryLog.Api/Controllers/JobSessionController.cs`.

**EntryLog.Web:**
- Purpose: Server-rendered employee web app.
- Contains: MVC controllers, Razor views, auth/session helpers, UI view models, and static files.
- Key files: `EntryLog.Web/Program.cs`, `EntryLog.Web/Controllers/AccountController.cs`, `EntryLog.Web/Controllers/WorkSessionController.cs`, `EntryLog.Web/Controllers/FaceIdController.cs`, `EntryLog.Web/Extensions/HttpContextExtensions.cs`, `EntryLog.Web/Views/Shared/_Layout.cshtml`.

**EntryLog.Business:**
- Purpose: Shared application layer used by both hosts.
- Contains: DI registration, service interfaces/implementations, DTOs, cryptography, external API adapters, JWT helpers, mappers, query filters, pagination, specs, and utilities.
- Key files: `EntryLog.Business/BusinessDependencies.cs`, `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`, `EntryLog.Business/Services/FaceIdService.cs`, `EntryLog.Business/JWT/CustomBearerAuthentication.cs`, `EntryLog.Business/ImageBB/ImageBBService.cs`.

**EntryLog.Data:**
- Purpose: Storage-facing implementations.
- Contains: Repository contracts, MongoDB repositories/serializers/config, SQL legacy EF Core context/configs, and specification infrastructure.
- Key files: `EntryLog.Data/DataDependencies.cs`, `EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs`, `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs`, `EntryLog.Data/SqlLegacy/Repositories/EmployeeRepository.cs`, `EntryLog.Data/Specifications/Specification.cs`.

**EntryLog.Entities:**
- Purpose: Shared data model.
- Contains: POCO entities and enums used by data/business code.
- Key files: `EntryLog.Entities/POCOEntities/AppUser.cs`, `EntryLog.Entities/POCOEntities/WorkSession.cs`, `EntryLog.Entities/POCOEntities/Employee.cs`, `EntryLog.Entities/Enums/RoleType.cs`, `EntryLog.Entities/Enums/SessionStatus.cs`.

**tests/EntryLog.Tests:**
- Purpose: Automated tests for business, data, and web layers.
- Contains: xUnit test suites plus reusable builders.
- Key files: `tests/EntryLog.Tests/EntryLog.Tests.csproj`, `tests/EntryLog.Tests/Business/Services/AppUserServicesTests.cs`, `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs`, `tests/EntryLog.Tests/Web/Controllers/AccountControllerTests.cs`, `tests/EntryLog.Tests/Helpers/AppUserBuilder.cs`.

**docs/superpowers:**
- Purpose: Planning/spec artifacts produced by other workflows.
- Contains: `docs/superpowers/plans/` and `docs/superpowers/specs/`.
- Key files: `docs/superpowers/plans/2026-03-21-unit-tests-plan.md`, `docs/superpowers/specs/2026-03-21-unit-tests-design.md`.

## Key File Locations

**Entry Points:**
- `EntryLog.Api/Program.cs`: API host startup.
- `EntryLog.Web/Program.cs`: MVC host startup.
- `EntryLog.slnx`: solution-level build/test entry.

**Configuration:**
- `EntryLog.Api/appsettings.json`: API host configuration source.
- `EntryLog.Web/appsettings.json`: MVC host configuration source.
- `EntryLog.Business/BusinessDependencies.cs`: business DI and option binding.
- `EntryLog.Data/DataDependencies.cs`: data DI and database bootstrap.

**Core Logic:**
- `EntryLog.Business/Services/AppUserServices.cs`: registration, login, recovery orchestration.
- `EntryLog.Business/Services/WorkSessionServices.cs`: work-session open/close/filter use cases.
- `EntryLog.Business/Services/FaceIdService.cs`: Face ID registration and reference-image token flows.
- `EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs`: Mongo user document access.
- `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs`: Mongo work-session access.
- `EntryLog.Data/SqlLegacy/Repositories/EmployeeRepository.cs`: SQL employee lookups.

**Testing:**
- `tests/EntryLog.Tests/Business/`: application-layer tests.
- `tests/EntryLog.Tests/Data/Specifications/SpecificationTests.cs`: specification infrastructure tests.
- `tests/EntryLog.Tests/Web/Controllers/`: MVC controller tests.
- `tests/EntryLog.Tests/Helpers/`: builders for test fixture setup.

## Naming Conventions

**Files:**
- Controllers use `*Controller.cs`: `EntryLog.Api/Controllers/AccountController.cs`.
- Business interfaces use `I*.cs`: `EntryLog.Business/Interfaces/IWorkSessionServices.cs`.
- Business implementations use `*Service.cs` or `*Services.cs`: `EntryLog.Business/Services/FaceIdService.cs`, `EntryLog.Business/Services/AppUserServices.cs`.
- Repository implementations use `*Repository.cs`: `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs`.
- DTOs use `*Dto.cs`: `EntryLog.Business/DTOs/LoginResponseDto.cs`.
- Tests use `*Tests.cs`: `tests/EntryLog.Tests/Web/Controllers/FaceIdControllerTests.cs`.

**Directories:**
- Top-level projects use PascalCase solution names: `EntryLog.Api/`, `EntryLog.Business/`, `EntryLog.Data/`.
- Within projects, folders are organized by role rather than feature: `EntryLog.Business/Services/`, `EntryLog.Business/DTOs/`, `EntryLog.Data/MongoDB/Repositories/`, `EntryLog.Web/Controllers/`.
- Test folders mirror production subsystems: `tests/EntryLog.Tests/Business/Services/`, `tests/EntryLog.Tests/Web/Controllers/`.

## Where to Add New Code

**New API endpoint:**
- Primary code: add a controller action under `EntryLog.Api/Controllers/` if the endpoint is API-only.
- Shared use-case logic: add or extend a service contract in `EntryLog.Business/Interfaces/` and implementation in `EntryLog.Business/Services/`.
- Tests: add controller tests under `tests/EntryLog.Tests/Web/Controllers/` only for MVC controllers; API controller tests are not yet present, so add new API tests in a mirrored folder such as `tests/EntryLog.Tests/Api/Controllers/` if you introduce that pattern.

**New MVC page or employee flow:**
- Controller/action: `EntryLog.Web/Controllers/`.
- Razor view: matching folder under `EntryLog.Web/Views/` such as `EntryLog.Web/Views/WorkSession/`.
- View model: `EntryLog.Web/Models/` when the model is UI-specific rather than shared API DTO state.
- Auth/claims helper changes: keep them in `EntryLog.Web/Extensions/`.

**New business use case:**
- Contract: `EntryLog.Business/Interfaces/`.
- Implementation: `EntryLog.Business/Services/`.
- DTOs: `EntryLog.Business/DTOs/`.
- Mapping: `EntryLog.Business/Mappers/` if entity-to-DTO projection is needed.
- DI registration: update `EntryLog.Business/BusinessDependencies.cs`.

**New repository or persistence behavior:**
- Interface: `EntryLog.Data/Interfaces/`.
- Mongo implementation: `EntryLog.Data/MongoDB/Repositories/`.
- SQL implementation: `EntryLog.Data/SqlLegacy/Repositories/` and `EntryLog.Data/SqlLegacy/Configs/` if EF mappings change.
- Serializer/config updates: `EntryLog.Data/MongoDB/Serializers/` or `EntryLog.Data/SqlLegacy/Configs/`.
- DI registration: update `EntryLog.Data/DataDependencies.cs`.

**New domain type:**
- Entity or value-holder class: `EntryLog.Entities/POCOEntities/`.
- Enum: `EntryLog.Entities/Enums/`.
- Follow-up persistence wiring: update Mongo serializers in `EntryLog.Data/MongoDB/Serializers/` or EF configs in `EntryLog.Data/SqlLegacy/Configs/` if the type is stored.

**Utilities:**
- Business-layer helpers with application meaning: `EntryLog.Business/Utils/` or `EntryLog.Business/Infrastructure/`.
- Web-only helper extensions: `EntryLog.Web/Extensions/`.
- Test-only builders/helpers: `tests/EntryLog.Tests/Helpers/`.

## Special Directories

**`EntryLog.Web/Views/`:**
- Purpose: Razor UI templates grouped by controller.
- Generated: No.
- Committed: Yes.

**`EntryLog.Web/wwwroot/`:**
- Purpose: Static assets for the MVC site.
- Generated: Mixed; current contents are committed application assets and libraries.
- Committed: Yes.

**`EntryLog.Data/MongoDB/Serializers/`:**
- Purpose: BSON class-map registration for Mongo persistence schema.
- Generated: No.
- Committed: Yes.

**`EntryLog.Data/SqlLegacy/Configs/`:**
- Purpose: EF Core entity-to-table mapping for the legacy SQL database.
- Generated: No.
- Committed: Yes.

**`tests/EntryLog.Tests/Helpers/`:**
- Purpose: Builder objects for concise test setup.
- Generated: No.
- Committed: Yes.

**`tests/EntryLog.Tests/TestResults/`:**
- Purpose: Local test execution artifacts such as coverage output.
- Generated: Yes.
- Committed: Current directory exists in the worktree; treat as generated output rather than a place for source files.

**`.planning/codebase/`:**
- Purpose: Generated architecture/stack/convention/concern reference docs for planning agents.
- Generated: Yes.
- Committed: Intended to be committed once generated.

**`.github/workflows/`:**
- Purpose: CI/CD workflow location.
- Generated: No.
- Committed: Yes, but currently empty.

## Placement Guidance by Dependency Direction

- Keep HTTP concerns in `EntryLog.Api/` or `EntryLog.Web/`; do not move controller-only types into `EntryLog.Business/` unless they become shared DTO contracts.
- Keep orchestration in `EntryLog.Business/`; presentation projects should call interfaces from `EntryLog.Business/Interfaces/` rather than repositories directly.
- Keep database queries and schema mapping in `EntryLog.Data/`; `EntryLog.Business/Services/*.cs` should depend on `EntryLog.Data/Interfaces/*.cs`, not on Mongo or EF types.
- Keep pure entity shape in `EntryLog.Entities/`; avoid adding service, controller, or DI code there.
- Mirror production namespace/role structure in `tests/EntryLog.Tests/` when adding tests.

---

*Structure analysis: 2026-03-30*
