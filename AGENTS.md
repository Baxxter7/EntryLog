# EntryLog agent notes

This document orients coding agents to this repo. It is a snapshot of observed
conventions; follow existing patterns when editing adjacent code.

## Repo map
- `EntryLog.Api`: ASP.NET Core Web API (controllers, Swagger).
- `EntryLog.Web`: ASP.NET Core MVC site (cookie auth, session, views).
- `EntryLog.Business`: application services, DTOs, mappers, auth, mail.
- `EntryLog.Data`: data access, MongoDB repositories, SQL legacy EF Core.
- `EntryLog.Entities`: POCO entity models and enums.
- `EntryLog.slnx`: solution entry for CLI build/test.

## Tooling and runtime
- Target framework: .NET 8 (`net8.0`).
- Nullable reference types: enabled in all projects.
- Implicit usings: enabled in all projects.

## Build, run, test

### Restore
- `dotnet restore EntryLog.slnx`

### Build
- `dotnet build EntryLog.slnx`
- `dotnet build EntryLog.Api/EntryLog.Api.csproj`
- `dotnet build EntryLog.Web/EntryLog.Web.csproj`

### Run
- API: `dotnet run --project EntryLog.Api/EntryLog.Api.csproj`
- Web MVC: `dotnet run --project EntryLog.Web/EntryLog.Web.csproj`
- Watch: `dotnet watch --project EntryLog.Api/EntryLog.Api.csproj`

### Tests
- No test projects found in the repo at time of writing.
- If tests are added, use the solution:
  `dotnet test EntryLog.slnx`
- Single test (when tests exist):
  `dotnet test EntryLog.slnx --filter FullyQualifiedName~Namespace.Class.Method`
- By trait (xUnit example):
  `dotnet test EntryLog.slnx --filter "Category=Integration"`

### Lint / format
- No repo-specific lint rules or format scripts found.
- Optional formatting if needed (requires tool):
  `dotnet format EntryLog.slnx`

## Cursor / Copilot rules
- No `.cursor/rules/`, `.cursorrules`, or `.github/copilot-instructions.md` found.

## Code style and conventions

### General C# style
- Keep file-scoped namespaces where present (`namespace Foo.Bar;`).
- Some MVC controllers use block namespaces; keep the local file style.
- Indentation is 4 spaces; avoid tabs in new code.
- Prefer explicit access modifiers; match existing visibility choices.
- Keep files named after their primary type.

### Using directives
- `using` statements appear at the top of the file.
- Ordering is not strictly enforced; preserve local ordering when editing.
- Avoid unused `using` entries.

### Naming
- Types, methods, and properties: PascalCase.
- Interfaces: `I` prefix (e.g., `IWorkSessionServices`).
- Private fields: `_camelCase`.
- DTOs: `*Dto` suffix; typically `record` or `record class`.
- Service implementations: `*Service` or `*Services` to match interface names.

### Types and nullability
- Nullable reference types are enabled; use `?` when null is a valid state.
- Use guard clauses for nulls (`if (x is null) return ...`).
- Favor `var` only when the type is clear from the right side; otherwise use explicit types.

### Async and tasks
- Async methods end with `Async` and return `Task` or `Task<T>`.
- Controller actions are async for I/O operations.
- Avoid blocking calls inside async flows.

### Error handling and responses
- Many services return tuples like `(bool success, string message)` or
  `(bool success, string message, T? data)`.
- API controllers wrap responses with `Ok(new { success, message, ... })`.
- MVC controllers commonly return `Json(new { ... })` for XHR flows.
- Prefer early returns for failure branches.

### Records and entities
- DTOs are modeled as `record` types in `EntryLog.Business/DTOs`.
- Entities are POCO `class` types in `EntryLog.Entities/POCOEntities`.
- Entities typically initialize reference properties with defaults.

### Dependency injection
- Service registration uses extension methods:
  - `EntryLog.Business/BusinessDependencies.cs` (`AddBusinessServices`).
  - `EntryLog.Data/DataDependencies.cs` (`AddDataServices`).
- Keep registrations grouped by concern (config, HTTP clients, services, repos).
- Use `IOptions<T>` for configuration sections.

### API layer conventions (`EntryLog.Api`)
- Controllers use `[ApiController]` and `[Route("api/[controller]")]`.
- Use DTOs for input and output instead of entity types.
- For file uploads, DTOs use `IFormFile` and `[FromForm]`.

### Web MVC conventions (`EntryLog.Web`)
- Cookie authentication is configured in `Program.cs`.
- Use `[ValidateAntiForgeryToken]` on POST actions.
- Use `HttpContext` extension methods in `EntryLog.Web/Extensions`.

### Data access conventions (`EntryLog.Data`)
- MongoDB repositories are `internal` with primary constructors.
- `IMongoCollection<T>` stored in a private readonly field.
- MongoDB serializers are initialized in `AddDataServices`.
- SQL legacy uses EF Core `DbContext` in `SqlLegacy/Contexts`.

### Mapping and pagination
- Mappers live in `EntryLog.Business/Mappers` (static mapping helpers).
- Pagination uses `PaginatedResult<T>` and `WorkSessionQueryFilter`.
- Filtering and sorting rely on `Specification` and `Spec` classes.

### Dates and time
- Use `DateTime.UtcNow` for server-side timestamps.
- Avoid mixing local time and UTC in the same flow.

### Security and auth
- JWT configuration and validation live under `EntryLog.Business/JWT`.
- Password hashing uses Argon2 options from config.
- When changing auth flows, keep `LoginResponseDto` and cookie claims in sync.

### Comments and language
- Comments are sparse and sometimes Spanish; only add comments when necessary.
- Keep comment language consistent with nearby code.

## Editing tips for agents
- Keep changes scoped to the relevant project layer (Api/Web/Business/Data/Entities).
- Preserve the existing public API shape and DTO contracts.
- For new services, add interface in `Interfaces`, implementation in `Services`,
  and register in `BusinessDependencies`.
- For new repositories, add interface in `Data/Interfaces`, implementation in
  `Data/MongoDB/Repositories`, and register in `DataDependencies`.
- When touching controllers, ensure routes and action names stay stable.
- When adding configuration, update the corresponding `appsettings*.json`.

## Files to check before large changes
- `EntryLog.Api/Program.cs`
- `EntryLog.Web/Program.cs`
- `EntryLog.Business/BusinessDependencies.cs`
- `EntryLog.Data/DataDependencies.cs`
- `EntryLog.slnx`
