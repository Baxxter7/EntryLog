# Codebase Concerns

**Analysis Date:** 2026-03-30

## Tech Debt

**Hybrid persistence split across legacy SQL and MongoDB:**
- Issue: Core user flows depend on both the SQL-backed employee source and MongoDB-backed application data with no transaction boundary or reconciliation layer.
- Files: `EntryLog.Data/DataDependencies.cs`, `EntryLog.Data/SqlLegacy/Repositories/EmployeeRepository.cs`, `EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs`, `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs`, `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`
- Impact: Registration, login validation, and work-session creation can succeed against one store while the other remains stale or unavailable.
- Fix approach: Introduce a clear source-of-truth strategy, add synchronization/retry rules around cross-store flows, and document failure handling for partial success.

**Business logic concentrated in large service classes:**
- Issue: `AppUserServices`, `WorkSessionServices`, and `FaceIdService` mix validation, orchestration, persistence, external API calls, and formatting.
- Files: `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`, `EntryLog.Business/Services/FaceIdService.cs`
- Impact: Changes to auth, recovery, Face ID, or session rules require editing large files with many branches, increasing regression risk.
- Fix approach: Extract focused collaborators for validation, token/recovery flow, image handling, and session state transitions before adding more features.

**Dead and misleading API surface in close-session flow:**
- Issue: `CloseJobSessionDto` exposes `SessionId`, but the service never reads it and always closes the active session by employee code.
- Files: `EntryLog.Business/DTOs/CloseJobSessionDto.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`
- Impact: API consumers can assume session-specific closing is supported when it is not; future multi-session logic will be harder to add safely.
- Fix approach: Remove `SessionId` from the contract or implement session-targeted close behavior with explicit validation.

**Pagination contract does not match implementation:**
- Issue: `PaginationQuery.PageSize` is accepted from callers, then overwritten to `10` before applying the page limit.
- Files: `EntryLog.Business/QueryFilters/PaginationQuery.cs`, `EntryLog.Business/QueryFilters/WorkSessionQueryFilter.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`
- Impact: Clients cannot request variable page sizes, and the public filter shape is misleading.
- Fix approach: Honor caller-provided values with bounds enforcement, or remove the unused input from the public contract.

## Known Bugs

**API project has authorization middleware but no authentication setup:**
- Symptoms: `EntryLog.Api` cannot enforce bearer or cookie authentication because `Program.cs` calls `UseAuthorization()` without `AddAuthentication()` or `UseAuthentication()`.
- Files: `EntryLog.Api/Program.cs`, `EntryLog.Api/Controllers/AccountController.cs`, `EntryLog.Api/Controllers/JobSessionController.cs`
- Trigger: Any attempt to protect API endpoints with `[Authorize]` will fail or behave unexpectedly until authentication is actually registered.
- Workaround: None in the API host; token validation is currently done ad hoc inside business services instead of the ASP.NET Core pipeline.

**Authenticated MVC POST endpoints skip anti-forgery validation:**
- Symptoms: Logged-in browser flows for Face ID registration and work-session opening accept POST requests without `[ValidateAntiForgeryToken]`.
- Files: `EntryLog.Web/Controllers/FaceIdController.cs`, `EntryLog.Web/Controllers/WorkSessionController.cs`, `EntryLog.Web/Controllers/AccountController.cs`
- Trigger: A user with an active auth cookie visits a malicious page that submits a forged form to `/empleado/faceid` or `/emleado/sessiones/abrir`.
- Workaround: None in server code; only the account endpoints enforce anti-forgery today.

**Windows-only timezone identifier in user-facing mapping:**
- Symptoms: Face ID date formatting depends on `TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time")`, which is a Windows timezone ID.
- Files: `EntryLog.Business/Utils/TimeFunctions.cs`, `EntryLog.Business/Mappers/FaceIdMapper.cs`
- Trigger: Running the app on Linux containers or non-Windows hosts.
- Workaround: None in code; deployment must stay on a host that understands the Windows timezone identifier.

**Work-session and Face ID routes contain typos/inconsistent naming:**
- Symptoms: Route strings use mixed English/Spanish names and include typos such as `/emleado/sessiones/abrir`.
- Files: `EntryLog.Web/Controllers/WorkSessionController.cs`, `EntryLog.Api/Controllers/JobSessionController.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`
- Trigger: Frontend or integration code relies on route naming conventions or attempts to discover endpoints predictably.
- Workaround: Consumers must hardcode the current route strings exactly as implemented.

## Security Considerations

**Sensitive configuration is bound from appsettings-shaped files:**
- Risk: The application expects database credentials, API tokens, RSA keys, and JWT secret material under configuration paths such as `ConnectionStrings:EmployeeDB`, `EntryLogDbOptions:ConnectionUri`, `ImageBBOptions:ApiToken`, `MailtrapApiOptions:ApiToken`, `EncryptionKeyValues:PublicKey`, `EncryptionKeyValues:PrivateKey`, and `JwtConfiguration.secret`.
- Files: `EntryLog.Business/BusinessDependencies.cs`, `EntryLog.Data/DataDependencies.cs`, `EntryLog.Api/appsettings.json`, `EntryLog.Web/appsettings.json`, `EntryLog.Api/appsettings.Development.json`, `EntryLog.Web/appsettings.Development.json`, `.gitignore`
- Current mitigation: `.gitignore` excludes `appsettings.*.json` while keeping base `appsettings.json` tracked.
- Recommendations: Move production secrets to a secret manager/environment variables, standardize key casing for JWT config, and document the approved secret source explicitly.

**Public API endpoints perform privileged operations without host-level auth:**
- Risk: Registration, login, password recovery, and session open/close endpoints are anonymous HTTP endpoints in `EntryLog.Api`.
- Files: `EntryLog.Api/Controllers/AccountController.cs`, `EntryLog.Api/Controllers/JobSessionController.cs`, `EntryLog.Api/Program.cs`
- Current mitigation: Service-layer validation checks business rules, but there is no API authentication/authorization boundary.
- Recommendations: Add real authentication schemes, protect endpoints appropriately, and move token validation out of controller/service-specific code paths.

**Password recovery has no rate limiting or login lockout enforcement:**
- Risk: `AppUser.Attempts` exists but is never updated, and the recovery workflow can be requested repeatedly for active users.
- Files: `EntryLog.Entities/POCOEntities/AppUser.cs`, `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs`
- Current mitigation: Recovery tokens expire after 30 minutes and are deactivated after completion/expiry handling.
- Recommendations: Enforce lockout/throttling, track failed logins, and add abuse protection to recovery start endpoints.

**Static file serving is overly permissive:**
- Risk: `ServeUnknownFileTypes = true` serves any unknown file from `wwwroot` as `application/octet-stream`.
- Files: `EntryLog.Web/Program.cs`
- Current mitigation: None beyond the `wwwroot` boundary.
- Recommendations: Disable unknown-file serving unless a specific business case requires it, and allow only explicit static asset types.

## Performance Bottlenecks

**Missing MongoDB index setup for hot query paths:**
- Problem: Repositories query by user code, email, recovery token, employee id, and session status, but no index creation code exists.
- Files: `EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs`, `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs`, `EntryLog.Data/MongoDB/Serializers/AppUserSerializer.cs`, `EntryLog.Data/MongoDB/Serializers/WorkSessionSerializer.cs`, `EntryLog.Data/DataDependencies.cs`
- Cause: Collection mapping exists, but repository startup never creates supporting indexes.
- Improvement path: Add startup index creation for `Code`, `Email`, `RecoveryToken`, and the active-session query path on `EmployeeId` + `Status`.

**Reference image responses re-download and re-encode full images on demand:**
- Problem: Every Face ID read loads the remote image over HTTP, buffers it fully, and base64-encodes it before returning it.
- Files: `EntryLog.Business/Services/FaceIdService.cs`, `EntryLog.Web/Controllers/FaceIdController.cs`
- Cause: `GenerateBase64PngImageAsync` always fetches remote content and materializes a full byte array in memory.
- Improvement path: Cache derived payloads, serve URLs where possible, or stream content instead of duplicating it into base64 strings.

**Failed session flows can create orphaned external uploads:**
- Problem: `OpenJobSessionAsync` uploads an image before validating the stored Face ID match; a later validation failure leaves the uploaded image unused.
- Files: `EntryLog.Business/Services/WorkSessionServices.cs`, `EntryLog.Business/Services/FaceIdService.cs`, `EntryLog.Business/ImageBB/ImageBBService.cs`
- Cause: External side effects happen before all business validation completes, and there is no compensating delete step.
- Improvement path: Validate as much as possible before upload and add best-effort cleanup for failed post-upload flows.

## Fragile Areas

**Password recovery token format is custom and tightly coupled to stored ciphertext:**
- Files: `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Cryptography/RsaAsymmetricEncryptionService.cs`
- Why fragile: The token format embeds `DateTime.ToBinary()` and the user email inside encrypted text, then compares the stored encrypted token directly. Any format or crypto change can break recovery links and stored tokens.
- Safe modification: Introduce a versioned token format or dedicated signed token generator before changing encryption or payload structure.
- Test coverage: Unit tests exist for current behavior in `tests/EntryLog.Tests/Business/Services/AppUserServicesTests.cs`, but there is no end-to-end coverage through HTTP endpoints.

**Session-opening rules depend on magic numbers and mixed-language messages:**
- Files: `EntryLog.Business/Services/WorkSessionServices.cs`, `EntryLog.Business/Services/FaceIdService.cs`
- Why fragile: The descriptor length (`128`) and match threshold (`0.5`) are hardcoded, and failure messages mix English and Spanish, making behavior hard to tune consistently.
- Safe modification: Centralize descriptor constants and thresholds in options/config, then update tests before changing matching behavior.
- Test coverage: Service-level tests cover many branches in `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs` and `tests/EntryLog.Tests/Business/Services/FaceIdServiceTests.cs`, but there is no integration coverage of real image upload and persistence.

**Operational diagnostics are nearly absent:**
- Files: `EntryLog.Business/Services/AppUserServices.cs`, `EntryLog.Business/Services/WorkSessionServices.cs`, `EntryLog.Business/Services/FaceIdService.cs`, `EntryLog.Business/Cryptography/RsaAsymmetricEncryptionService.cs`, `EntryLog.Web/Controllers/HomeController.cs`
- Why fragile: Several exceptions are caught and suppressed into generic messages, while only `HomeController` injects `ILogger` and does not use it.
- Safe modification: Add structured logging at the service and HTTP boundaries before changing high-risk flows.
- Test coverage: No tests assert logging/telemetry behavior.

## Scaling Limits

**Current session listing is fixed to small pages and lacks richer filtering:**
- Current capacity: `GetSessionListByFilterAsync` always limits results to 10 records per page and only filters by `EmployeeId`.
- Limit: Admin/reporting scenarios will need broader filtering, larger exports, and indexed queries once data volume grows.
- Scaling path: Expand `WorkSessionQueryFilter`, add matching repository indexes, and separate end-user pagination from reporting/export use cases.

**Authentication/session lifetime policy is hardcoded:**
- Current capacity: Auth cookies expire after 30 minutes and Face ID reference tokens expire after 30 seconds.
- Limit: Policy changes require code edits instead of configuration updates, making environment-specific tuning slow and error-prone.
- Scaling path: Move expirations into options bound from configuration and document environment defaults.

## Dependencies at Risk

**Legacy EF Core SQL dependency for employees:**
- Risk: The `SqlLegacy` namespace and `EmployeeDB` connection make employee lookup dependent on an older SQL source that is separate from the Mongo-backed app domain.
- Impact: Any schema drift or outage in the legacy store blocks registration and session validation.
- Migration plan: Define whether employee master data stays in SQL or is replicated into Mongo; then isolate the chosen source behind a stable adapter.

**External image and email vendors are hard requirements for key flows:**
- Risk: Face ID/session image upload depends on ImageBB, and account recovery depends on Mailtrap.
- Impact: Outages or API changes in those services break user-facing operations immediately.
- Migration plan: Add retries/timeouts/circuit breaking, vendor abstraction, and a fallback storage/email provider.

## Missing Critical Features

**No CI/CD or automated quality gate detected:**
- Problem: No `.github/workflows/`, Azure Pipeline files, or other repo-local pipeline definitions are present.
- Blocks: There is no enforced build/test check before merges or deployments.

**No health checks or readiness endpoints:**
- Problem: Neither `EntryLog.Api/Program.cs` nor `EntryLog.Web/Program.cs` registers ASP.NET Core health checks.
- Blocks: Operators cannot probe SQL, MongoDB, or external dependency readiness in a standard way.

**No centralized exception-to-HTTP mapping in the API:**
- Problem: `EntryLog.Api` does not configure `UseExceptionHandler`, `ProblemDetails`, or a consistent error contract.
- Blocks: API consumers receive ad hoc success/message payloads instead of standard status codes and machine-readable failures.

## Test Coverage Gaps

**API host and API controllers are untested:**
- What's not tested: `EntryLog.Api/Program.cs`, `EntryLog.Api/Controllers/AccountController.cs`, and `EntryLog.Api/Controllers/JobSessionController.cs` do not have matching tests under `tests/EntryLog.Tests`.
- Files: `EntryLog.Api/Program.cs`, `EntryLog.Api/Controllers/AccountController.cs`, `EntryLog.Api/Controllers/JobSessionController.cs`
- Risk: Anonymous endpoint exposure, response-shape regressions, and middleware/configuration mistakes can ship unnoticed.
- Priority: High

**Mongo repository behavior is untested against real query semantics:**
- What's not tested: Repository classes under `EntryLog.Data/MongoDB/Repositories` have no direct tests; current data-layer tests only cover specification composition.
- Files: `EntryLog.Data/MongoDB/Repositories/AppUserRepository.cs`, `EntryLog.Data/MongoDB/Repositories/WorkSessionRepository.cs`, `tests/EntryLog.Tests/Data/Specifications/SpecificationTests.cs`
- Risk: Query/index assumptions can fail only in production-like MongoDB environments.
- Priority: High

**Authenticated MVC session flow is only partially covered:**
- What's not tested: `EntryLog.Web/Controllers/WorkSessionController.cs` and `EntryLog.Web/Extensions/HttpContextExtensions.cs` have no direct test files, while current web coverage is limited to `AccountController` and `FaceIdController`.
- Files: `EntryLog.Web/Controllers/WorkSessionController.cs`, `EntryLog.Web/Extensions/HttpContextExtensions.cs`, `tests/EntryLog.Tests/Web/AccountControllerTests.cs`, `tests/EntryLog.Tests/Web/FaceIdControllerTests.cs`
- Risk: Cookie-claim assumptions, route typos, and anti-forgery gaps can regress without detection.
- Priority: Medium

**Repository docs are stale relative to the actual test footprint:**
- What's not tested/documented accurately: Both `AGENTS.md` and `Reame.md` still state that no test project exists even though `tests/EntryLog.Tests/EntryLog.Tests.csproj` is in the solution.
- Files: `AGENTS.md`, `Reame.md`, `EntryLog.slnx`, `tests/EntryLog.Tests/EntryLog.Tests.csproj`
- Risk: Future planning can underestimate current coverage and duplicate setup work.
- Priority: Medium

---

*Concerns audit: 2026-03-30*
