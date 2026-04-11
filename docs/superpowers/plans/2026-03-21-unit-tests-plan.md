# Unit Tests Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add comprehensive unit test coverage to EntryLog using xUnit, NSubstitute, and FluentAssertions with London School (mock-first) approach.

**Architecture:** Single test project `EntryLog.Tests` targeting `net8.0`, organized by layer (Business/Data/Web). Before adding tests, fix the current production-contract mismatches that would make the suite assert broken behavior (password mismatch message, nullable DTO contract, FaceId controller reference endpoint, and JWT nullability). Services and controllers are tested with mocks; crypto/JWT components are tested with concrete implementations using test configuration values.

**Tech Stack:** xUnit, NSubstitute, FluentAssertions, coverlet.collector, Microsoft.NET.Test.Sdk

**Spec:** `docs/superpowers/specs/2026-03-21-unit-tests-design.md`

---

### Task 0: Pre-Test Contract Fixes

**Files:**
- Modify: `EntryLog.Business/Services/AppUserServices.cs`
- Modify: `EntryLog.Business/DTOs/GetWorkSessionDto.cs`
- Modify: `EntryLog.Business/Interfaces/IJwtService.cs`
- Modify: `EntryLog.Web/Controllers/FaceIdController.cs`

- [ ] **Step 1: Fix password mismatch handling in AppUserServices**

Update `EntryLog.Business/Services/AppUserServices.cs` so `RegisterEmployeeAsync` returns a dedicated password mismatch message instead of reusing the username conflict message:

```csharp
if (string.IsNullOrEmpty(employeeDto.Password) || employeeDto.Password != employeeDto.PasswordConf)
    return (false, "Passwords do not match", null);
```

- [ ] **Step 2: Align GetWorkSessionDto with the mapper**

Update `EntryLog.Business/DTOs/GetWorkSessionDto.cs` so `CheckOut` is nullable, matching `WorkSessionMapper`:

```csharp
public record GetWorkSessionDto
(
    string Id,
    int EmployeeId,
    GetCheckDto CheckIn,
    GetCheckDto? CheckOut,
    TimeSpan? TotalWorked,
    string Status
);
```

- [ ] **Step 3: Align JWT interface nullability with implementation**

Update `EntryLog.Business/Interfaces/IJwtService.cs` so `ValidateToken` reflects the current implementation contract:

```csharp
IDictionary<string, string>? ValidateToken(string token);
```

- [ ] **Step 4: Fix FaceIdController reference-image endpoint**

Update `EntryLog.Web/Controllers/FaceIdController.cs` so `GetReferenceImageAsync` delegates to the correct service method:

```csharp
string imageBase64 = await _faceIdService.GetReferenceImageAsync(authHeader);
```

- [ ] **Step 5: Build the affected projects**

Run: `dotnet build EntryLog.Business/EntryLog.Business.csproj && dotnet build EntryLog.Web/EntryLog.Web.csproj`
Expected: Build succeeded with 0 errors.

- [ ] **Step 6: Commit**

```bash
git add EntryLog.Business/Services/AppUserServices.cs EntryLog.Business/DTOs/GetWorkSessionDto.cs EntryLog.Business/Interfaces/IJwtService.cs EntryLog.Web/Controllers/FaceIdController.cs
git commit -m "fix: align auth and work-session contracts before unit tests"
```

---

### Task 1: Project Scaffolding

**Files:**
- Create: `tests/EntryLog.Tests/EntryLog.Tests.csproj`
- Modify: `EntryLog.slnx`
- Modify: `EntryLog.Business/EntryLog.Business.csproj`
- Modify: `EntryLog.Data/EntryLog.Data.csproj` (optional; only if internal Data types will be tested directly)

- [ ] **Step 1: Create the test project file**

Create `tests/EntryLog.Tests/EntryLog.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="FluentAssertions" Version="8.3.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="NSubstitute" Version="5.3.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.0.2" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\EntryLog.Business\EntryLog.Business.csproj" />
    <ProjectReference Include="..\..\EntryLog.Data\EntryLog.Data.csproj" />
    <ProjectReference Include="..\..\EntryLog.Entities\EntryLog.Entities.csproj" />
    <ProjectReference Include="..\..\EntryLog.Web\EntryLog.Web.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Add InternalsVisibleTo to EntryLog.Business.csproj**

Add before `</Project>`:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="EntryLog.Tests" />
</ItemGroup>
```

- [ ] **Step 3: Add InternalsVisibleTo to EntryLog.Data.csproj if Data internals will be tested**

Add before `</Project>` only if this plan later includes direct tests of `internal` Data types such as evaluators:

```xml
<ItemGroup>
  <InternalsVisibleTo Include="EntryLog.Tests" />
</ItemGroup>
```

- [ ] **Step 4: Add test project to EntryLog.slnx**

Add inside `<Solution>`:

```xml
<Project Path="tests/EntryLog.Tests/EntryLog.Tests.csproj" />
```

- [ ] **Step 5: Verify the project builds**

Run: `dotnet build EntryLog.slnx`
Expected: Build succeeded with 0 errors.

- [ ] **Step 6: Commit**

```bash
git add tests/EntryLog.Tests/EntryLog.Tests.csproj EntryLog.slnx EntryLog.Business/EntryLog.Business.csproj
git add EntryLog.Data/EntryLog.Data.csproj # include only if modified in Step 3
git commit -m "chore: add EntryLog.Tests project with xUnit, NSubstitute, FluentAssertions"
```

---

### Task 2: Test Data Builders

**Files:**
- Create: `tests/EntryLog.Tests/Helpers/AppUserBuilder.cs`
- Create: `tests/EntryLog.Tests/Helpers/EmployeeBuilder.cs`
- Create: `tests/EntryLog.Tests/Helpers/WorkSessionBuilder.cs`

- [ ] **Step 1: Create AppUserBuilder**

Create `tests/EntryLog.Tests/Helpers/AppUserBuilder.cs`:

```csharp
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Tests.Helpers;

internal class AppUserBuilder
{
    private Guid _id = Guid.NewGuid();
    private int _code = 1001;
    private string _name = "John Doe";
    private RoleType _role = RoleType.Employee;
    private string _email = "john@test.com";
    private string _cellPhone = "55551234";
    private string _password = "hashed_password";
    private int _attempts = 0;
    private string? _recoveryToken;
    private bool _recoveryTokenActive;
    private FaceID? _faceId;
    private bool _active = true;

    public AppUserBuilder WithCode(int code) { _code = code; return this; }
    public AppUserBuilder WithName(string name) { _name = name; return this; }
    public AppUserBuilder WithEmail(string email) { _email = email; return this; }
    public AppUserBuilder WithPassword(string password) { _password = password; return this; }
    public AppUserBuilder WithActive(bool active) { _active = active; return this; }
    public AppUserBuilder WithRecoveryToken(string? token) { _recoveryToken = token; return this; }
    public AppUserBuilder WithRecoveryTokenActive(bool active) { _recoveryTokenActive = active; return this; }
    public AppUserBuilder WithFaceId(FaceID? faceId) { _faceId = faceId; return this; }

    public AppUser Build() => new()
    {
        Id = _id,
        Code = _code,
        Name = _name,
        Role = _role,
        Email = _email,
        CellPhone = _cellPhone,
        Password = _password,
        Attempts = _attempts,
        RecoveryToken = _recoveryToken,
        RecoveryTokenActive = _recoveryTokenActive,
        FaceID = _faceId,
        Active = _active
    };
}
```

- [ ] **Step 2: Create EmployeeBuilder**

Create `tests/EntryLog.Tests/Helpers/EmployeeBuilder.cs`:

```csharp
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Tests.Helpers;

internal class EmployeeBuilder
{
    private int _code = 1001;
    private string _fullName = "John Doe";
    private int _positionId = 1;

    public EmployeeBuilder WithCode(int code) { _code = code; return this; }
    public EmployeeBuilder WithFullName(string name) { _fullName = name; return this; }

    public Employee Build() => new()
    {
        Code = _code,
        FullName = _fullName,
        PositionId = _positionId,
        DateofBirthday = new DateTime(1990, 1, 1),
        TownName = "Test Town",
        Position = new Position { Id = _positionId, Name = "Developer", Description = "Dev" }
    };
}
```

- [ ] **Step 3: Create WorkSessionBuilder**

Create `tests/EntryLog.Tests/Helpers/WorkSessionBuilder.cs`:

```csharp
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Tests.Helpers;

internal class WorkSessionBuilder
{
    private Guid _id = Guid.NewGuid();
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
        },
        PhotoUrl = "https://example.com/checkin.jpg",
        Descriptor = Enumerable.Range(0, 128).Select(i => (float)i * 0.01f).ToList()
    };
    private Check? _checkOut;
    private SessionStatus _status = SessionStatus.InProgress;

    public WorkSessionBuilder WithId(Guid id) { _id = id; return this; }
    public WorkSessionBuilder WithEmployeeId(int id) { _employeeId = id; return this; }
    public WorkSessionBuilder WithStatus(SessionStatus status) { _status = status; return this; }
    public WorkSessionBuilder WithCheckOut(Check checkOut) { _checkOut = checkOut; _status = SessionStatus.Completed; return this; }

    public WorkSession Build() => new()
    {
        Id = _id,
        EmployeeId = _employeeId,
        CheckIn = _checkIn,
        CheckOut = _checkOut,
        Status = _status
    };
}
```

- [ ] **Step 4: Verify build**

Run: `dotnet build tests/EntryLog.Tests/EntryLog.Tests.csproj`
Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add tests/EntryLog.Tests/Helpers/
git commit -m "feat(tests): add test data builders for AppUser, Employee, WorkSession"
```

---

### Task 3: AppUserServices Tests

**Files:**
- Create: `tests/EntryLog.Tests/Business/Services/AppUserServicesTests.cs`

This task assumes Task 0 already fixed the password mismatch branch in `AppUserServices`.

- [ ] **Step 1: Write the test class with RegisterEmployee tests**

Create `tests/EntryLog.Tests/Business/Services/AppUserServicesTests.cs`:

```csharp
using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Services;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace EntryLog.Tests.Business.Services;

public class AppUserServicesTests
{
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly IAppUserRepository _appUserRepo = Substitute.For<IAppUserRepository>();
    private readonly IPasswordHasherService _hasher = Substitute.For<IPasswordHasherService>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly IEmailSenderService _emailSender = Substitute.For<IEmailSenderService>();
    private readonly IUriService _uriService = Substitute.For<IUriService>();
    private readonly AppUserServices _sut;

    public AppUserServicesTests()
    {
        _sut = new AppUserServices(
            _employeeRepo, _appUserRepo, _hasher,
            _encryption, _emailSender, _uriService);
    }

    // --- RegisterEmployeeAsync ---

    [Fact]
    public async Task RegisterEmployeeAsync_EmployeeNotFound_ReturnsFalse()
    {
        var dto = new CreateEmployeeUserDto("1001", "user@test.com", "555", "Pass1", "Pass1");
        _employeeRepo.GetByCodeAsync(1001).ReturnsNull();

        var (success, message, data) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Employee not found");
        data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterEmployeeAsync_UserAlreadyExists_ReturnsFalse()
    {
        var dto = new CreateEmployeeUserDto("1001", "user@test.com", "555", "Pass1", "Pass1");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().WithCode(1001).Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().WithCode(1001).Build());

        var (success, message, _) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("The employee already has a registered user account");
    }

    [Fact]
    public async Task RegisterEmployeeAsync_UsernameAlreadyTaken_ReturnsFalse()
    {
        var dto = new CreateEmployeeUserDto("1001", "taken@test.com", "555", "Pass1", "Pass1");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().WithCode(1001).Build());
        _appUserRepo.GetByCodeAsync(1001).ReturnsNull();
        _appUserRepo.GetByUserNameAsync("taken@test.com").Returns(new AppUserBuilder().Build());

        var (success, message, _) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("The user already exists");
    }

    [Fact]
    public async Task RegisterEmployeeAsync_PasswordMismatch_ReturnsFalse()
    {
        var dto = new CreateEmployeeUserDto("1001", "user@test.com", "555", "Pass1", "Pass2");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().WithCode(1001).Build());
        _appUserRepo.GetByCodeAsync(1001).ReturnsNull();
        _appUserRepo.GetByUserNameAsync("user@test.com").ReturnsNull();

        var (success, message, _) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Passwords do not match");
    }

    [Fact]
    public async Task RegisterEmployeeAsync_ValidData_CreatesUserAndReturnsSuccess()
    {
        var dto = new CreateEmployeeUserDto("1001", "user@test.com", "555", "Pass1", "Pass1");
        var employee = new EmployeeBuilder().WithCode(1001).WithFullName("John Doe").Build();
        _employeeRepo.GetByCodeAsync(1001).Returns(employee);
        _appUserRepo.GetByCodeAsync(1001).ReturnsNull();
        _appUserRepo.GetByUserNameAsync("user@test.com").ReturnsNull();
        _hasher.Hash("Pass1").Returns("hashed_pass");

        var (success, message, data) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeTrue();
        message.Should().Be("Employee created successfully");
        data.Should().NotBeNull();
        data!.DocumentNumber.Should().Be(1001);
        data.Name.Should().Be("John Doe");
        await _appUserRepo.Received(1).CreateAsync(Arg.Is<AppUser>(u => u.Code == 1001));
    }

    // --- UserLoginAsync ---

    [Fact]
    public async Task UserLoginAsync_UserNotFound_ReturnsFalse()
    {
        var dto = new UserCredentialsDto("unknown@test.com", "pass");
        _appUserRepo.GetByUserNameAsync("unknown@test.com").ReturnsNull();

        var (success, message, _) = await _sut.UserLoginAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Incorrect username or password");
    }

    [Fact]
    public async Task UserLoginAsync_UserInactive_ReturnsFalse()
    {
        var dto = new UserCredentialsDto("user@test.com", "pass");
        _appUserRepo.GetByUserNameAsync("user@test.com")
            .Returns(new AppUserBuilder().WithActive(false).Build());

        var (success, message, _) = await _sut.UserLoginAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("An error has occurred. Please contact the administrator");
    }

    [Fact]
    public async Task UserLoginAsync_WrongPassword_ReturnsFalse()
    {
        var dto = new UserCredentialsDto("user@test.com", "wrong");
        var user = new AppUserBuilder().WithPassword("hashed").Build();
        _appUserRepo.GetByUserNameAsync("user@test.com").Returns(user);
        _hasher.Verify("wrong", "hashed").Returns(false);

        var (success, _, _) = await _sut.UserLoginAsync(dto);

        success.Should().BeFalse();
    }

    [Fact]
    public async Task UserLoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var dto = new UserCredentialsDto("user@test.com", "pass");
        var user = new AppUserBuilder().WithCode(1001).WithEmail("user@test.com").WithPassword("hashed").Build();
        _appUserRepo.GetByUserNameAsync("user@test.com").Returns(user);
        _hasher.Verify("pass", "hashed").Returns(true);

        var (success, message, data) = await _sut.UserLoginAsync(dto);

        success.Should().BeTrue();
        message.Should().Be("Login successful");
        data.Should().NotBeNull();
    }

    // --- AccountRecoveryStartAsync ---

    [Fact]
    public async Task AccountRecoveryStartAsync_UserNotFound_ReturnsFalse()
    {
        _appUserRepo.GetByUserNameAsync("unknown").ReturnsNull();

        var (success, message) = await _sut.AccountRecoveryStartAsync("unknown");

        success.Should().BeFalse();
        message.Should().Be("Account recovery was not successful.");
    }

    [Fact]
    public async Task AccountRecoveryStartAsync_UserInactive_ReturnsFalse()
    {
        _appUserRepo.GetByUserNameAsync("user@test.com")
            .Returns(new AppUserBuilder().WithActive(false).Build());

        var (success, message) = await _sut.AccountRecoveryStartAsync("user@test.com");

        success.Should().BeFalse();
        message.Should().Be("Account recovery was not successful.");
    }

    [Fact]
    public async Task AccountRecoveryStartAsync_ValidUser_EncryptsTokenAndUpdatesUser()
    {
        var user = new AppUserBuilder().WithEmail("user@test.com").Build();
        _appUserRepo.GetByUserNameAsync("user@test.com").Returns(user);
        _encryption.Encrypt(Arg.Any<string>()).Returns("encrypted_token");
        _uriService.ApplicationURL.Returns("https://app.test");

        var (success, message) = await _sut.AccountRecoveryStartAsync("user@test.com");

        success.Should().BeTrue();
        message.Should().Be("email sent to account user@test.com");
        await _appUserRepo.Received(1).UpdateAsync(Arg.Is<AppUser>(u =>
            u.RecoveryToken == "encrypted_token" && u.RecoveryTokenActive));
    }

    // --- AccountRecoveryCompleteAsync ---

    [Fact]
    public async Task AccountRecoveryCompleteAsync_EmptyToken_ReturnsFalse()
    {
        var dto = new AccountRecoveryDto("", "newPass", "newPass");

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid token");
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_ValidTokenNotExpired_UpdatesPassword()
    {
        var ticks = DateTime.UtcNow.AddMinutes(-5).ToBinary();
        var plainToken = $"{ticks}:user@test.com";
        var dto = new AccountRecoveryDto("encrypted", "newPass", "newPass");
        var user = new AppUserBuilder().WithEmail("user@test.com").WithRecoveryToken("encrypted").Build();

        _encryption.Decrypt("encrypted").Returns(plainToken);
        _appUserRepo.GetByRecoveryTokenAsync("encrypted").Returns(user);
        _hasher.Hash("newPass").Returns("new_hashed");

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeTrue();
        message.Should().Be("Password updated successfully");
        await _appUserRepo.Received(1).UpdateAsync(Arg.Is<AppUser>(u =>
            u.Password == "new_hashed" && u.RecoveryToken == null && !u.RecoveryTokenActive));
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_ExpiredToken_ReturnsFalse()
    {
        var ticks = DateTime.UtcNow.AddMinutes(-60).ToBinary();
        var plainToken = $"{ticks}:user@test.com";
        var dto = new AccountRecoveryDto("encrypted", "newPass", "newPass");
        var user = new AppUserBuilder().WithEmail("user@test.com").WithRecoveryToken("encrypted").Build();

        _encryption.Decrypt("encrypted").Returns(plainToken);
        _appUserRepo.GetByRecoveryTokenAsync("encrypted").Returns(user);

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid or expired token");
    }
}
```

Note: do not assert `_emailSender` interactions in this task. The current implementation returns `isSend = true` without calling the sender yet, so these tests should lock the public result contract, not a future side effect.

- [ ] **Step 2: Run tests to verify they pass**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~AppUserServicesTests" -v normal`
Expected: All tests PASS.

- [ ] **Step 3: Commit**

```bash
git add tests/EntryLog.Tests/Business/Services/AppUserServicesTests.cs
git commit -m "test: add AppUserServices unit tests (register, login, recovery)"
```

---

### Task 4: WorkSessionServices Tests

**Files:**
- Create: `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs`

- [ ] **Step 1: Write the test class**

Create `tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs`:

```csharp
using EntryLog.Business.DTOs;
using EntryLog.Business.Enums;
using EntryLog.Business.Interfaces;
using EntryLog.Business.QueryFilters;
using EntryLog.Business.Services;
using EntryLog.Data.Interfaces;
using EntryLog.Data.Specifications;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace EntryLog.Tests.Business.Services;

public class WorkSessionServicesTests
{
    private readonly IWorkSessionRepository _sessionRepo = Substitute.For<IWorkSessionRepository>();
    private readonly IAppUserRepository _appUserRepo = Substitute.For<IAppUserRepository>();
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly ILoadImagesService _imageService = Substitute.For<ILoadImagesService>();
    private readonly IUriService _uriService = Substitute.For<IUriService>();
    private readonly WorkSessionServices _sut;

    public WorkSessionServicesTests()
    {
        _uriService.UserAgent.Returns("TestAgent");
        _uriService.Platform.Returns("TestPlatform");
        _uriService.RemoteIpAddress.Returns("127.0.0.1");

        _sut = new WorkSessionServices(
            _sessionRepo, _appUserRepo, _employeeRepo, _imageService, _uriService);
    }

    private static IFormFile CreateMockFormFile()
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("test.jpg");
        file.ContentType.Returns("image/jpeg");
        file.OpenReadStream().Returns(new MemoryStream());
        return file;
    }

    private static string ValidDescriptorJson()
        => JsonSerializer.Serialize(Enumerable.Range(0, 128).Select(i => (float)i * 0.01f).ToList());

    // --- OpenJobSessionAsync ---

    [Fact]
    public async Task OpenJobSessionAsync_EmployeeNotFound_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).ReturnsNull();

        var (success, message, _) = await _sut.OpenJobSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Employee not found");
    }

    [Fact]
    public async Task OpenJobSessionAsync_UserNotFound_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).ReturnsNull();

        var (success, message, _) = await _sut.OpenJobSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("User not found");
    }

    [Fact]
    public async Task OpenJobSessionAsync_ActiveSessionExists_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).Returns(new WorkSessionBuilder().Build());

        var (success, message, _) = await _sut.OpenJobSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("The employee has an active session");
    }

    [Fact]
    public async Task OpenJobSessionAsync_InvalidDescriptor_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), null, "not-json");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).ReturnsNull();
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>()).Returns("https://img.test/photo.jpg");

        var (success, message, _) = await _sut.OpenJobSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Descriptor is invalid");
    }

    [Fact]
    public async Task OpenJobSessionAsync_ValidData_CreatesSessionAndReturnsDto()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), "test notes", ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).ReturnsNull();
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>()).Returns("https://img.test/photo.jpg");

        var (success, message, data) = await _sut.OpenJobSessionAsync(dto);

        success.Should().BeTrue();
        message.Should().Be("Session opened successfully");
        data.Should().NotBeNull();
        data!.EmployeeId.Should().Be(1001);
        data.Status.Should().Be("InProgress");
        await _sessionRepo.Received(1).CreateAsync(Arg.Any<WorkSession>());
    }

    // --- ClosedJobSessionAsync ---

    [Fact]
    public async Task ClosedJobSessionAsync_NoActiveSession_ReturnsFalse()
    {
        var dto = new CloseJobSessionDto(null, "1001", "14.0", "-90.0", CreateMockFormFile(), null);
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).ReturnsNull();

        var (success, message) = await _sut.ClosedJobSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("There is no active session for the user");
    }

    [Fact]
    public async Task ClosedJobSessionAsync_ValidData_ClosesSessionSuccessfully()
    {
        var activeSession = new WorkSessionBuilder().WithEmployeeId(1001).Build();
        var dto = new CloseJobSessionDto(null, "1001", "14.0", "-90.0", CreateMockFormFile(), "closing notes");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).Returns(activeSession);
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>()).Returns("https://img.test/checkout.jpg");

        var (success, message) = await _sut.ClosedJobSessionAsync(dto);

        success.Should().BeTrue();
        message.Should().Be("Session closed successfully");
        activeSession.Status.Should().Be(SessionStatus.Completed);
        activeSession.CheckOut.Should().NotBeNull();
        await _sessionRepo.Received(1).UpdateAsync(activeSession);
    }

    // --- GetSessionListByFilterAsync ---

    [Fact]
    public async Task GetSessionListByFilterAsync_ReturnsPagedResults()
    {
        var sessions = new List<WorkSession> { new WorkSessionBuilder().Build() };
        _sessionRepo.CountAsync(Arg.Any<Specification<WorkSession>>()).Returns(1);
        _sessionRepo.GetAllAsync(Arg.Any<ISpecification<WorkSession>>()).Returns(sessions);

        var filter = new WorkSessionQueryFilter { EmployeeId = 1001, Sort = SortType.Descending };

        var result = await _sut.GetSessionListByFilterAsync(filter);

        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result.Results.Should().HaveCount(1);
    }
}
```

Add assertions in the two success-path tests to verify `CheckIn`/`CheckOut` copy `_uriService.UserAgent`, `_uriService.Platform`, and `_uriService.RemoteIpAddress` into the resulting session object.

- [ ] **Step 2: Run tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~WorkSessionServicesTests" -v normal`
Expected: All tests PASS.

- [ ] **Step 3: Commit**

```bash
git add tests/EntryLog.Tests/Business/Services/WorkSessionServicesTests.cs
git commit -m "test: add WorkSessionServices unit tests (open, close, filter)"
```

---

### Task 5: FaceIdService Tests

**Files:**
- Create: `tests/EntryLog.Tests/Business/Services/FaceIdServiceTests.cs`

- [ ] **Step 1: Write the test class**

Create `tests/EntryLog.Tests/Business/Services/FaceIdServiceTests.cs`:

```csharp
using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Services;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace EntryLog.Tests.Business.Services;

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

    private static string ValidDescriptorJson()
        => JsonSerializer.Serialize(Enumerable.Range(0, 128).Select(i => (float)i * 0.01f).ToList());

    private static IFormFile CreateMockFormFile()
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("face.png");
        file.ContentType.Returns("image/png");
        file.Length.Returns(1024);
        file.OpenReadStream().Returns(new MemoryStream(new byte[1024]));
        return file;
    }

    private void SetupImageDownload(byte[] bytes, string mediaType = "image/png")
    {
        var handler = new StubHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        });

        handler.Response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType);
        var client = new HttpClient(handler);
        _httpClientFactory.CreateClient().Returns(client);
    }

    private sealed class StubHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public HttpResponseMessage Response { get; } = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(Response);
    }

    // --- CreateEmployeeFaceIdAsync ---

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_InvalidCode_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(0, CreateMockFormFile(), ValidDescriptorJson());

        var (success, message, _) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("EmployeeCode is required");
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_NullImage_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, null!, ValidDescriptorJson());

        var (success, message, _) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Image is required");
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_EmptyDescriptor_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), "");

        var (success, message, _) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Descriptor is required");
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_InvalidDescriptorJson_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), "not-json");

        var (success, message, _) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid descriptor format");
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_WrongDescriptorLength_ReturnsFalse()
    {
        var shortDescriptor = JsonSerializer.Serialize(new List<float> { 1.0f, 2.0f });
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), shortDescriptor);

        var (success, message, _) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Descriptor length no valid");
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_EmployeeNotFound_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _employeeRepo.GetByCodeAsync(1001).ReturnsNull();

        var (success, message, _) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("An error while fetching the employee");
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_UserNotFound_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).ReturnsNull();
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());

        var (success, message, _) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("An error while fetching the user");
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_FaceIdAlreadyActive_ReturnsFalse()
    {
        var existingFaceId = new FaceID { ImageUrl = "url", RegisterDate = DateTime.UtcNow, Active = true };
        var user = new AppUserBuilder().WithFaceId(existingFaceId).Build();
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).Returns(user);
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());

        var (success, message, _) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("FaceID is already set up");
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_ValidData_UploadsImageAndPersistsFaceId()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        var user = new AppUserBuilder().Build();
        _userRepo.GetByCodeAsync(1001).Returns(user);
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("https://img.test/face.png");
        SetupImageDownload(new byte[] { 1, 2, 3, 4 });

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeTrue();
        message.Should().Be("Face ID created successfully");
        data.Should().NotBeNull();
        data!.Base64Image.Should().StartWith("data:image/png;base64,");
        await _userRepo.Received(1).UpdateAsync(Arg.Is<AppUser>(u => u.FaceID != null && u.FaceID.Active));
    }

    // --- GenerateReferenceImageTokenAsync ---

    [Fact]
    public async Task GenerateReferenceImageTokenAsync_DelegatesToJwtService()
    {
        _jwtService.GenerateTokenAsync("123", "faceid_reference", TimeSpan.FromSeconds(30))
            .Returns("generated_token");

        var result = await _sut.GenerateReferenceImageTokenAsync("123");

        result.Should().Be("generated_token");
    }

    // --- GetReferenceImageAsync ---

    [Fact]
    public async Task GetReferenceImageAsync_EmptyHeader_ReturnsEmpty()
    {
        var result = await _sut.GetReferenceImageAsync("");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReferenceImageAsync_InvalidBearerFormat_ReturnsEmpty()
    {
        var result = await _sut.GetReferenceImageAsync("Basic abc123");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReferenceImageAsync_InvalidToken_ReturnsEmpty()
    {
        _jwtService.ValidateToken("bad_token").ReturnsNull();

        var result = await _sut.GetReferenceImageAsync("Bearer bad_token");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReferenceImageAsync_WrongPurpose_ReturnsEmpty()
    {
        var claims = new Dictionary<string, string>
        {
            { "purpose", "wrong_purpose" },
            { "sub", "1001" }
        };
        _jwtService.ValidateToken("token").Returns(claims);

        var result = await _sut.GetReferenceImageAsync("Bearer token");

        result.Should().BeEmpty();
    }
}
```

- [ ] **Step 2: Run tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~FaceIdServiceTests" -v normal`
Expected: All tests PASS.

- [ ] **Step 3: Commit**

```bash
git add tests/EntryLog.Tests/Business/Services/FaceIdServiceTests.cs
git commit -m "test: add FaceIdService unit tests (create, token, reference image)"
```

---

### Task 6: Argon2 Password Hasher Tests

**Files:**
- Create: `tests/EntryLog.Tests/Business/Cryptography/Argon2PasswordHasherServiceTests.cs`

- [ ] **Step 1: Write the test class**

Create `tests/EntryLog.Tests/Business/Cryptography/Argon2PasswordHasherServiceTests.cs`:

```csharp
using EntryLog.Business.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace EntryLog.Tests.Business.Cryptography;

public class Argon2PasswordHasherServiceTests
{
    private readonly Argon2PasswordHasherService _sut;

    public Argon2PasswordHasherServiceTests()
    {
        var options = Options.Create(new Argon2PasswordHashOptions
        {
            DegreeOfParallelism = 1,
            MemorySize = 1024,
            Iterations = 1,
            SaltSize = 16,
            HashSize = 32
        });
        _sut = new Argon2PasswordHasherService(options);
    }

    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = _sut.Hash("password123");

        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_ContainsSaltAndHashSeparatedByColon()
    {
        var hash = _sut.Hash("password123");

        hash.Should().Contain(":");
        hash.Split(':').Should().HaveCount(2);
    }

    [Fact]
    public void Hash_DifferentPasswordsProduceDifferentHashes()
    {
        var hash1 = _sut.Hash("password1");
        var hash2 = _sut.Hash("password2");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Hash_SamePasswordProducesDifferentHashes_DueToDifferentSalts()
    {
        var hash1 = _sut.Hash("password123");
        var hash2 = _sut.Hash("password123");

        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = _sut.Hash("password123");

        var result = _sut.Verify("password123", hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_IncorrectPassword_ReturnsFalse()
    {
        var hash = _sut.Hash("password123");

        var result = _sut.Verify("wrong_password", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_InvalidHashFormat_ThrowsFormatException()
    {
        var act = () => _sut.Verify("password", "invalid_hash_no_colon");

        act.Should().Throw<FormatException>();
    }
}
```

- [ ] **Step 2: Run tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~Argon2PasswordHasherServiceTests" -v normal`
Expected: All tests PASS.

- [ ] **Step 3: Commit**

```bash
git add tests/EntryLog.Tests/Business/Cryptography/Argon2PasswordHasherServiceTests.cs
git commit -m "test: add Argon2PasswordHasherService tests (hash, verify)"
```

---

### Task 7: RSA Encryption Service Tests

**Files:**
- Create: `tests/EntryLog.Tests/Business/Cryptography/RsaEncryptionServiceTests.cs`

- [ ] **Step 1: Write the test class**

Create `tests/EntryLog.Tests/Business/Cryptography/RsaEncryptionServiceTests.cs`:

```csharp
using EntryLog.Business.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace EntryLog.Tests.Business.Cryptography;

public class RsaEncryptionServiceTests
{
    private readonly RsaAsymmetricEncryptionService _sut;

    public RsaEncryptionServiceTests()
    {
        using var rsa = new RSACryptoServiceProvider(2048);
        var publicKey = rsa.ToXmlString(false);
        var privateKey = rsa.ToXmlString(true);

        var options = Options.Create(new EncryptionKeyValues
        {
            PublicKey = publicKey,
            PrivateKey = privateKey
        });
        _sut = new RsaAsymmetricEncryptionService(options);
    }

    [Fact]
    public void Encrypt_ReturnsNonEmptyBase64String()
    {
        var cipher = _sut.Encrypt("hello world");

        cipher.Should().NotBeNullOrEmpty();
        var act = () => Convert.FromBase64String(cipher);
        act.Should().NotThrow();
    }

    [Fact]
    public void Decrypt_ReturnsOriginalPlainText()
    {
        var plainText = "test secret data";
        var cipher = _sut.Encrypt(plainText);

        var result = _sut.Decrypt(cipher);

        result.Should().Be(plainText);
    }

    [Fact]
    public void Decrypt_InvalidCipherText_ThrowsCryptographicException()
    {
        var act = () => _sut.Decrypt("not-valid-base64-cipher");

        act.Should().Throw<CryptographicException>();
    }

    [Fact]
    public void Encrypt_DifferentInputs_ProduceDifferentOutputs()
    {
        var cipher1 = _sut.Encrypt("text1");
        var cipher2 = _sut.Encrypt("text2");

        cipher1.Should().NotBe(cipher2);
    }
}
```

Note: assert the exception type only for invalid cipher text. The current implementation wraps decrypt failures with the same `"Encryption failed."` message used by encrypt, so the contract worth locking here is the thrown `CryptographicException`.

- [ ] **Step 2: Run tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~RsaEncryptionServiceTests" -v normal`
Expected: All tests PASS.

- [ ] **Step 3: Commit**

```bash
git add tests/EntryLog.Tests/Business/Cryptography/RsaEncryptionServiceTests.cs
git commit -m "test: add RSA encryption service tests (encrypt, decrypt)"
```

---

### Task 8: JWT CustomBearerAuthentication Tests

**Files:**
- Create: `tests/EntryLog.Tests/Business/JWT/CustomBearerAuthenticationTests.cs`

This task assumes Task 0 already updated `IJwtService.ValidateToken` to return `IDictionary<string, string>?`.

- [ ] **Step 1: Write the test class**

Create `tests/EntryLog.Tests/Business/JWT/CustomBearerAuthenticationTests.cs`:

```csharp
using EntryLog.Business.JWT;
using FluentAssertions;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace EntryLog.Tests.Business.JWT;

public class CustomBearerAuthenticationTests
{
    private readonly CustomBearerAuthentication _sut;

    public CustomBearerAuthenticationTests()
    {
        var options = Options.Create(new JwtConfiguration
        {
            Secret = "ThisIsATestSecretKeyThatIsLongEnoughForHmacSha256!!"
        });
        _sut = new CustomBearerAuthentication(options);
    }

    [Fact]
    public async Task GenerateTokenAsync_ReturnsValidJwtString()
    {
        var token = await _sut.GenerateTokenAsync("1001", "login", TimeSpan.FromMinutes(5));

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    [Fact]
    public async Task GenerateTokenAsync_TokenContainsExpectedClaims()
    {
        var token = await _sut.GenerateTokenAsync("1001", "faceid_reference", TimeSpan.FromMinutes(5));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "1001");
        jwt.Claims.Should().Contain(c => c.Type == "purpose" && c.Value == "faceid_reference");
    }

    [Fact]
    public async Task ValidateToken_ValidToken_ReturnsClaimsDictionary()
    {
        var token = await _sut.GenerateTokenAsync("1001", "login", TimeSpan.FromMinutes(5));

        var claims = _sut.ValidateToken(token);

        claims.Should().NotBeNull();
        claims!["sub"].Should().Be("1001");
        claims["purpose"].Should().Be("login");
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        var claims = _sut.ValidateToken("invalid.token.value");

        claims.Should().BeNull();
    }

    [Fact]
    public async Task ValidateToken_ExpiredToken_ReturnsNull()
    {
        var token = await _sut.GenerateTokenAsync("1001", "login", TimeSpan.FromMilliseconds(1));
        await Task.Delay(50);

        var claims = _sut.ValidateToken(token);

        claims.Should().BeNull();
    }
}
```

- [ ] **Step 2: Run tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~CustomBearerAuthenticationTests" -v normal`
Expected: All tests PASS.

- [ ] **Step 3: Commit**

```bash
git add tests/EntryLog.Tests/Business/JWT/CustomBearerAuthenticationTests.cs
git commit -m "test: add JWT CustomBearerAuthentication tests (generate, validate)"
```

---

### Task 9: Mapper Tests

**Files:**
- Create: `tests/EntryLog.Tests/Business/Mappers/WorkSessionMapperTests.cs`
- Create: `tests/EntryLog.Tests/Business/Mappers/FaceIdMapperTests.cs`

This task assumes Task 0 already made `GetWorkSessionDto.CheckOut` nullable.

- [ ] **Step 1: Write WorkSessionMapper tests**

Create `tests/EntryLog.Tests/Business/Mappers/WorkSessionMapperTests.cs`:

```csharp
using EntryLog.Business.Mappers;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;

namespace EntryLog.Tests.Business.Mappers;

public class WorkSessionMapperTests
{
    [Fact]
    public void MapToGetWorkSessionDto_MapsCheckInCorrectly()
    {
        var session = new WorkSessionBuilder().WithEmployeeId(1001).Build();

        var dto = WorkSessionMapper.MapToGetWorkSessionDto(session);

        dto.EmployeeId.Should().Be(1001);
        dto.Status.Should().Be("InProgress");
        dto.CheckIn.Should().NotBeNull();
        dto.CheckIn.Method.Should().Be(session.CheckIn.Method);
        dto.CheckIn.Location.Latitude.Should().Be(session.CheckIn.Location.Latitude);
        dto.CheckOut.Should().BeNull();
        dto.TotalWorked.Should().BeNull();
    }

    [Fact]
    public void MapToGetWorkSessionDto_WithCheckOut_MapsBothChecks()
    {
        var checkOut = new Check
        {
            Method = "Agent",
            DeviceName = "Windows",
            Date = DateTime.UtcNow,
            Location = new Location { Latitude = "14.0", Longitude = "-90.0", IpAddress = "10.0.0.1" },
            PhotoUrl = "https://img.test/out.jpg"
        };
        var session = new WorkSessionBuilder().WithCheckOut(checkOut).Build();

        var dto = WorkSessionMapper.MapToGetWorkSessionDto(session);

        dto.CheckOut.Should().NotBeNull();
        dto.Status.Should().Be("Completed");
        dto.TotalWorked.Should().NotBeNull();
    }
}
```

- [ ] **Step 2: Write FaceIdMapper tests**

Create `tests/EntryLog.Tests/Business/Mappers/FaceIdMapperTests.cs`:

```csharp
using EntryLog.Business.Mappers;
using EntryLog.Entities.POCOEntities;
using FluentAssertions;

namespace EntryLog.Tests.Business.Mappers;

public class FaceIdMapperTests
{
    [Fact]
    public void MapToEmployeeFaceIdDto_MapsFieldsCorrectly()
    {
        var faceId = new FaceID
        {
            ImageUrl = "https://img.test/face.png",
            RegisterDate = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            Active = true
        };

        var dto = FaceIdMapper.MapToEmployeeFaceIdDto(faceId, "base64data");

        dto.Base64Image.Should().Be("base64data");
        dto.Active.Should().BeTrue();
        dto.RegisterDate.Should().Be("15/01/2026 06:00 AM");
    }

    [Fact]
    public void Empty_ReturnsEmptyDto()
    {
        var dto = FaceIdMapper.Empty();

        dto.Base64Image.Should().BeEmpty();
        dto.RegisterDate.Should().BeEmpty();
        dto.Active.Should().BeFalse();
    }
}
```

- [ ] **Step 3: Run tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~Mappers" -v normal`
Expected: All tests PASS.

- [ ] **Step 4: Commit**

```bash
git add tests/EntryLog.Tests/Business/Mappers/
git commit -m "test: add WorkSessionMapper and FaceIdMapper tests"
```

---

### Task 10: Web Controllers Tests

**Files:**
- Create: `tests/EntryLog.Tests/Web/Controllers/AccountControllerTests.cs`
- Create: `tests/EntryLog.Tests/Web/Controllers/FaceIdControllerTests.cs`

- [ ] **Step 1: Write AccountController tests**

Create `tests/EntryLog.Tests/Web/Controllers/AccountControllerTests.cs`:

```csharp
using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace EntryLog.Tests.Web.Controllers;

public class AccountControllerTests
{
    private readonly IAppUserServices _appUserServices = Substitute.For<IAppUserServices>();
    private readonly AccountController _sut;

    public AccountControllerTests()
    {
        _sut = new AccountController(_appUserServices);

        var authService = Substitute.For<IAuthenticationService>();
        authService.SignInAsync(Arg.Any<HttpContext>(), Arg.Any<string>(),
            Arg.Any<ClaimsPrincipal>(), Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(authService);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public void RegisterEmployeeUser_ReturnsView()
    {
        var result = _sut.RegisterEmployeeUser();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Login_NotAuthenticated_ReturnsView()
    {
        var result = _sut.Login();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Login_Authenticated_RedirectsToMain()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "user") }, "test");
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var result = _sut.Login();

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be("index");
        redirect.ControllerName.Should().Be("main");
    }

    [Fact]
    public async Task LoginAsync_Failed_ReturnsJsonWithError()
    {
        var model = new UserCredentialsDto("user", "pass");
        _appUserServices.UserLoginAsync(model).Returns((false, "Incorrect username or password", (LoginResponseDto?)null));

        var result = await _sut.LoginAsync(model);

        result.Should().BeOfType<JsonResult>();
    }

    [Fact]
    public async Task LoginAsync_Success_SignsInAndReturnsPath()
    {
        var model = new UserCredentialsDto("user", "pass");
        var loginData = new LoginResponseDto(1001, "Employee", "user@test.com", "John");
        _appUserServices.UserLoginAsync(model).Returns((true, "Login successful", loginData));

        var result = await _sut.LoginAsync(model);

        result.Should().BeOfType<JsonResult>();
    }

    [Fact]
    public async Task RegisterEmployeeUserAsync_Failed_ReturnsJsonWithMessage()
    {
        var model = new CreateEmployeeUserDto("1001", "user@test.com", "555", "pass", "pass");
        _appUserServices.RegisterEmployeeAsync(model).Returns((false, "Employee not found", (LoginResponseDto?)null));

        var result = await _sut.RegisterEmployeeUserAsync(model);

        result.Should().BeOfType<JsonResult>();
    }

    [Fact]
    public async Task RegisterEmployeeUserAsync_Success_SignsInAndReturnsPath()
    {
        var model = new CreateEmployeeUserDto("1001", "user@test.com", "555", "pass", "pass");
        var loginData = new LoginResponseDto(1001, "Employee", "user@test.com", "John");
        _appUserServices.RegisterEmployeeAsync(model).Returns((true, "Created", loginData));

        var result = await _sut.RegisterEmployeeUserAsync(model);

        result.Should().BeOfType<JsonResult>();
    }
}
```

Add assertions in the two success-path tests to verify the JSON payload contains `success = true` and `path = "/main/index"`, since `AccountController` does not return `message` on success.

- [ ] **Step 2: Write FaceIdController tests**

Create `tests/EntryLog.Tests/Web/Controllers/FaceIdControllerTests.cs`:

```csharp
using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace EntryLog.Tests.Web.Controllers;

public class FaceIdControllerTests
{
    private readonly IFaceIdService _faceIdService = Substitute.For<IFaceIdService>();
    private readonly FaceIdController _sut;

    public FaceIdControllerTests()
    {
        _sut = new FaceIdController(_faceIdService);
        SetupAuthenticatedUser(1001, "john@test.com", "Employee", "John Doe");
    }

    private void SetupAuthenticatedUser(int code, string email, string role, string name)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, code.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, name)
        };
        var identity = new ClaimsIdentity(claims, "test");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact]
    public async Task Index_ReturnsViewWithFaceIdDto()
    {
        var faceIdDto = new EmployeeFaceIdDto("base64", "01/01/2026", true);
        _faceIdService.GetFaceIdAsync(1001).Returns(faceIdDto);

        var result = await _sut.Index();

        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(faceIdDto);
    }

    [Fact]
    public async Task GenerateSecurityTokenAsync_AuthenticatedUser_ReturnsOkWithToken()
    {
        _faceIdService.GenerateReferenceImageTokenAsync("1001").Returns("test_token");

        var result = await _sut.GenerateSecurityTokenAsync();

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GenerateSecurityTokenAsync_UnauthenticatedUser_ReturnsUnauthorized()
    {
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await _sut.GenerateSecurityTokenAsync();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetReferenceImageAsync_AuthenticatedUser_ReturnsOkWithImage()
    {
        _faceIdService.GetReferenceImageAsync("Bearer token").Returns("data:image/png;base64,AAAA");

        var result = await _sut.GetReferenceImageAsync("Bearer token");

        result.Should().BeOfType<OkObjectResult>();
    }
}
```

- [ ] **Step 3: Run tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~Controllers" -v normal`
Expected: All tests PASS.

- [ ] **Step 4: Commit**

```bash
git add tests/EntryLog.Tests/Web/Controllers/
git commit -m "test: add AccountController and FaceIdController tests"
```

---

### Task 11: Specification Pattern Tests

**Files:**
- Create: `tests/EntryLog.Tests/Data/Specifications/SpecificationTests.cs`

Scope only the reusable `Specification<TEntity>` behavior in this task. Do not add separate tests for `WorkSessionSpec`, because the current class is only an empty specialization.

- [ ] **Step 1: Write the test class**

Create `tests/EntryLog.Tests/Data/Specifications/SpecificationTests.cs`:

```csharp
using EntryLog.Data.Specifications;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;

namespace EntryLog.Tests.Data.Specifications;

public class SpecificationTests
{
    private class TestSpec : Specification<WorkSession> { }

    [Fact]
    public void DefaultExpression_MatchesAllItems()
    {
        var spec = new TestSpec();
        var sessions = new List<WorkSession>
        {
            new WorkSessionBuilder().WithEmployeeId(1).Build(),
            new WorkSessionBuilder().WithEmployeeId(2).Build()
        };

        var result = sessions.AsQueryable().Where(spec.Expression).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void AndAlso_FiltersCorrectly()
    {
        var spec = new TestSpec();
        spec.AndAlso(x => x.EmployeeId == 1);

        var sessions = new List<WorkSession>
        {
            new WorkSessionBuilder().WithEmployeeId(1).Build(),
            new WorkSessionBuilder().WithEmployeeId(2).Build()
        };

        var result = sessions.AsQueryable().Where(spec.Expression).ToList();

        result.Should().HaveCount(1);
        result[0].EmployeeId.Should().Be(1);
    }

    [Fact]
    public void AndAlso_MultipleFilters_CombinesWithAnd()
    {
        var spec = new TestSpec();
        spec.AndAlso(x => x.EmployeeId == 1);
        spec.AndAlso(x => x.Status == Entities.Enums.SessionStatus.InProgress);

        var sessions = new List<WorkSession>
        {
            new WorkSessionBuilder().WithEmployeeId(1).WithStatus(Entities.Enums.SessionStatus.InProgress).Build(),
            new WorkSessionBuilder().WithEmployeeId(1).WithStatus(Entities.Enums.SessionStatus.Completed).Build(),
            new WorkSessionBuilder().WithEmployeeId(2).WithStatus(Entities.Enums.SessionStatus.InProgress).Build()
        };

        var result = sessions.AsQueryable().Where(spec.Expression).ToList();

        result.Should().HaveCount(1);
    }

    [Fact]
    public void ApplyPaging_SetsTakeAndSkip()
    {
        var spec = new TestSpec();

        spec.ApplyPaging(10, 20);

        spec.Take.Should().Be(10);
        spec.Skip.Should().Be(20);
        spec.IsPagingEnabled.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --filter "FullyQualifiedName~SpecificationTests" -v normal`
Expected: All tests PASS.

- [ ] **Step 3: Commit**

```bash
git add tests/EntryLog.Tests/Data/Specifications/SpecificationTests.cs
git commit -m "test: add Specification pattern tests (filter, paging)"
```

---

### Task 12: Final Verification

- [ ] **Step 1: Run all tests**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj -v normal`
Expected: All tests PASS, 0 failures.

- [ ] **Step 2: Run with coverage**

Run: `dotnet test tests/EntryLog.Tests/EntryLog.Tests.csproj --collect:"XPlat Code Coverage"`
Expected: Coverage report generated.

- [ ] **Step 3: Verify full solution builds**

Run: `dotnet build EntryLog.slnx`
Expected: Build succeeded with 0 errors.

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "test: complete unit test suite for EntryLog (services, crypto, JWT, mappers, controllers, specs)"
```
