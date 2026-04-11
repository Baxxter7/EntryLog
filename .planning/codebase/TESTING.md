# Testing Patterns

**Analysis Date:** 2026-03-30

## Test Framework

**Runner:**
- xUnit 2.9.3 via `tests/EntryLog.Tests/EntryLog.Tests.csproj`
- Config: no separate `xunit.runner.json`, `runsettings`, `jest.config.*`, or `vitest.config.*` file detected; test setup relies on SDK defaults from `tests/EntryLog.Tests/EntryLog.Tests.csproj`

**Assertion Library:**
- FluentAssertions 8.3.0 via `tests/EntryLog.Tests/EntryLog.Tests.csproj`

**Run Commands:**
```bash
dotnet test EntryLog.slnx                                           # Run all tests
dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj              # Run the test project directly
dotnet test EntryLog.slnx --filter FullyQualifiedName~FaceIdServiceTests   # Run a focused subset
dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --collect:"XPlat Code Coverage"  # Generate coverage
```

## Test File Organization

**Location:**
- Tests live in a dedicated project under `tests/EntryLog.Tests`.
- Folders mirror the production layer being exercised:
  - `tests/EntryLog.Tests/Business/Services`
  - `tests/EntryLog.Tests/Business/Cryptography`
  - `tests/EntryLog.Tests/Business/JWT`
  - `tests/EntryLog.Tests/Business/Mappers`
  - `tests/EntryLog.Tests/Web/Controllers`
  - `tests/EntryLog.Tests/Data/Specifications`
  - `tests/EntryLog.Tests/Helpers`

**Naming:**
- Test classes and files use `{Subject}Tests`, e.g. `AppUserServicesTests`, `SpecificationTests`, `AccountControllerTests`.
- Individual test methods use `Method_Scenario_ExpectedResult`, e.g. `AccountRecoveryCompleteAsync_ExpiredToken_ReturnsFalse` in `tests/EntryLog.Tests/Business/Services/AppUserServicesTests.cs`.

**Structure:**
```
tests/EntryLog.Tests/
├── Business/
│   ├── Cryptography/
│   ├── JWT/
│   ├── Mappers/
│   └── Services/
├── Data/
│   └── Specifications/
├── Helpers/
├── Web/
│   └── Controllers/
├── EntryLog.Tests.csproj
└── GlobalUsings.cs
```

## Test Structure

**Suite Organization:**
```csharp
public class FaceIdServiceTests
{
    private readonly IAppUserRepository _userRepo = Substitute.For<IAppUserRepository>();
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly ILoadImagesService _imageService = Substitute.For<ILoadImagesService>();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly FaceIdService _sut;

    public FaceIdServiceTests()
    {
        _sut = new FaceIdService(_userRepo, _employeeRepo, _imageService, _httpClientFactory, _jwtService);
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_InvalidCode_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(0, CreateMockFormFile(), ValidDescriptorJson());

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("EmployeeCode is required");
        data.Should().BeNull();
    }
}
```

**Patterns:**
- Each suite creates substitutes as readonly fields and instantiates `_sut` in the constructor, e.g. `tests/EntryLog.Tests/Business/Services/AppUserServicesTests.cs`, `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs`, and `tests/EntryLog.Tests/Web/Controllers/AccountControllerTests.cs`.
- Shared test helpers are private methods inside the suite when only one suite needs them, e.g. `ValidDescriptorJson()` and `CreateMockFormFile()` in `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs`.
- Reusable domain builders live in `tests/EntryLog.Tests/Helpers/AppUserBuilder.cs`, `tests/EntryLog.Tests/Helpers/EmployeeBuilder.cs`, and `tests/EntryLog.Tests/Helpers/WorkSessionBuilder.cs`.
- Assertions usually validate both return values and interaction side effects, e.g. `Received(1).CreateAsync(...)` and `DidNotReceive().UpdateAsync(...)` in `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs`.

## Mocking

**Framework:**
- NSubstitute 5.3.0 via `tests/EntryLog.Tests/EntryLog.Tests.csproj`

**Patterns:**
```csharp
_employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
_appUserRepo.GetByCodeAsync(1001).ReturnsNull();
_imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
    .Returns(_ => Task.FromException<string?>(new InvalidOperationException("upload failed")));

await _sessionRepo.DidNotReceive().CreateAsync(Arg.Any<WorkSession>());
await _userRepo.Received(1).UpdateAsync(Arg.Is<AppUser>(u =>
    u.FaceID != null &&
    u.FaceID.Active));
```

**What to Mock:**
- Repository interfaces from `EntryLog.Data/Interfaces`, e.g. `IAppUserRepository`, `IEmployeeRepository`, and `IWorkSessionRepository`.
- Infrastructure/service boundaries such as `IHttpClientFactory`, `ILoadImagesService`, `IJwtService`, `IAuthenticationService`, and `IUriService`.
- ASP.NET Core controller context services when testing MVC behavior, e.g. `IAuthenticationService`, `ITempDataDictionary`, `IUrlHelper`, and `HttpContext` setup in `tests/EntryLog.Tests/Web/Controllers/AccountControllerTests.cs`.

**What NOT to Mock:**
- Pure mapping/specification code is tested concretely instead of mocking it, e.g. `tests/EntryLog.Tests/Business/Mappers/WorkSessionMapperTests.cs` and `tests/EntryLog.Tests/Data/Specifications/SpecificationTests.cs`.
- DTOs and entity objects are instantiated directly or via builders rather than substituted.

## Fixtures and Factories

**Test Data:**
```csharp
internal sealed class WorkSessionBuilder
{
    private int _employeeId = 1001;
    private Check _checkIn = new()
    {
        Method = "Mozilla/5.0",
        DeviceName = "Windows",
        Date = DateTime.UtcNow.AddHours(-8),
        Location = new Location
        {
            Latitude = "14.6349",
            Longitude = "-90.5069",
            IpAddress = "127.0.0.1"
        }
    };

    public WorkSessionBuilder WithEmployeeId(int employeeId) { _employeeId = employeeId; return this; }
    public WorkSession Build() => new() { EmployeeId = _employeeId, CheckIn = CloneCheck(_checkIn) };
}
```

**Location:**
- Shared builders are in `tests/EntryLog.Tests/Helpers`.
- Builders clone nested objects to avoid accidental mutation between assertions, e.g. `CloneCheck` in `tests/EntryLog.Tests/Helpers/WorkSessionBuilder.cs`.

## Coverage

**Requirements:**
- No enforced minimum threshold file or CI gate was detected.
- A committed coverage artifact exists at `tests/EntryLog.Tests/TestResults/c381fe70-8c18-4024-8421-4068f3f8ce02/coverage.cobertura.xml`, which signals manual or local coverage collection is already used.
- Current recorded snapshot in that file shows:
  - overall line-rate `0.588`
  - `EntryLog.Business` line-rate `0.7627`
  - `EntryLog.Data` line-rate `0.2013`
  - `EntryLog.Entities` line-rate `1.0`
  - `EntryLog.Web` classes are not represented in the visible package section, even though controller tests exist

**View Coverage:**
```bash
dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --collect:"XPlat Code Coverage"
```

## Test Types

**Unit Tests:**
- Dominant test type.
- Business service tests isolate repositories and infrastructure with NSubstitute and assert tuple results plus persistence side effects:
  - `tests/EntryLog.Tests/Business/Services/AppUserServicesTests.cs`
  - `tests/EntryLog.Tests/Business/Services/FaceIdServiceTests.cs`
  - `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs`
- Utility-level units cover cryptography and JWT helpers directly:
  - `tests/EntryLog.Tests/Business/Cryptography/Argon2PasswordHasherServiceTests.cs`
  - `tests/EntryLog.Tests/Business/Cryptography/RsaEncryptionServiceTests.cs`
  - `tests/EntryLog.Tests/Business/JWT/CustomBearerAuthenticationTests.cs`

**Integration Tests:**
- Not currently implemented as full database or HTTP-host integration tests.
- Closest coverage is controller-level action testing with manually assembled ASP.NET Core context objects in:
  - `tests/EntryLog.Tests/Web/Controllers/AccountControllerTests.cs`
  - `tests/EntryLog.Tests/Web/Controllers/FaceIdControllerTests.cs`

**E2E Tests:**
- Not used. No Playwright, Selenium, Cypress, or browser-host test project detected.

## Common Patterns

**Async Testing:**
```csharp
[Fact]
public async Task LoginAsync_Success_SignsInAndReturnsPath()
{
    var model = new UserCredentialsDto("user", "pass");
    var loginData = new LoginResponseDto(1001, "Employee", "user@test.com", "John");
    _appUserServices.UserLoginAsync(model).Returns((true, "Login successful", loginData));

    var result = await _sut.LoginAsync(model);
    var payload = GetPayload(result);

    payload.GetProperty("success").GetBoolean().Should().BeTrue();
    await _authService.Received(1).SignInAsync(
        Arg.Any<HttpContext>(),
        Arg.Any<string>(),
        Arg.Any<ClaimsPrincipal>(),
        Arg.Any<AuthenticationProperties>());
}
```

**Error Testing:**
```csharp
[Fact]
public void ApplyPaging_NegativeTake_Throws()
{
    var spec = new TestSpec();

    var act = () => spec.ApplyPaging(-1, 0);

    act.Should().Throw<ArgumentOutOfRangeException>();
}
```

## Validation Commands

**Fastest practical checks:**
```bash
dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter FullyQualifiedName~AppUserServicesTests
dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter FullyQualifiedName~WorkSessionServicesTests
dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter FullyQualifiedName~AccountControllerTests
```

**Full regression:**
```bash
dotnet test EntryLog.slnx
```

**Areas currently covered best:**
- Business services in `tests/EntryLog.Tests/Business/Services`
- Cryptography/JWT helpers in `tests/EntryLog.Tests/Business/Cryptography` and `tests/EntryLog.Tests/Business/JWT`
- Specification logic in `tests/EntryLog.Tests/Data/Specifications`

**Areas to treat carefully because tests are thin or absent:**
- DI registration roots `EntryLog.Business/BusinessDependencies.cs` and `EntryLog.Data/DataDependencies.cs`
- External HTTP adapter implementations `EntryLog.Business/ImageBB/ImageBBService.cs` and `EntryLog.Business/Mailtrap/MailtrapApiService.cs`
- Concrete MongoDB and EF repositories under `EntryLog.Data/MongoDB/Repositories` and `EntryLog.Data/SqlLegacy/Repositories`

---

*Testing analysis: 2026-03-30*
