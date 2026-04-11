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
            _employeeRepo,
            _appUserRepo,
            _hasher,
            _encryption,
            _emailSender,
            _uriService);
    }

    private static bool IsValidRecoveryTokenPlain(string token)
    {
        var parts = token.Split(':');
        if (parts.Length != 2 || parts[1] != "user@test.com" || !long.TryParse(parts[0], out var value))
        {
            return false;
        }

        var timestamp = DateTime.FromBinary(value);
        return timestamp > DateTime.UtcNow.AddMinutes(-1) && timestamp <= DateTime.UtcNow.AddMinutes(1);
    }

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
    public async Task RegisterEmployeeAsync_InvalidEmployeeCode_ReturnsFalse()
    {
        var dto = new CreateEmployeeUserDto("abc", "user@test.com", "555", "Pass1", "Pass1");

        var (success, message, data) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid employee code");
        data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterEmployeeAsync_UserAlreadyExists_ReturnsFalse()
    {
        var dto = new CreateEmployeeUserDto("1001", "user@test.com", "555", "Pass1", "Pass1");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().WithCode(1001).Build());

        var (success, message, data) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("The employee already has a registered user account");
        data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterEmployeeAsync_UsernameAlreadyTaken_ReturnsFalse()
    {
        var dto = new CreateEmployeeUserDto("1001", "taken@test.com", "555", "Pass1", "Pass1");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).ReturnsNull();
        _appUserRepo.GetByUserNameAsync("taken@test.com").Returns(new AppUserBuilder().Build());

        var (success, message, data) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("The user already exists");
        data.Should().BeNull();
    }

    [Fact]
    public async Task RegisterEmployeeAsync_PasswordMismatch_ReturnsFalse()
    {
        var dto = new CreateEmployeeUserDto("1001", "user@test.com", "555", "Pass1", "Pass2");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).ReturnsNull();
        _appUserRepo.GetByUserNameAsync("user@test.com").ReturnsNull();

        var (success, message, data) = await _sut.RegisterEmployeeAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Passwords do not match");
        data.Should().BeNull();
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
        await _appUserRepo.Received(1).CreateAsync(Arg.Is<AppUser>(u =>
            u.Code == 1001 &&
            u.Email == "user@test.com" &&
            u.Password == "hashed_pass"));
    }

    [Fact]
    public async Task UserLoginAsync_UserNotFound_ReturnsFalse()
    {
        var dto = new UserCredentialsDto("unknown@test.com", "pass");
        _appUserRepo.GetByUserNameAsync("unknown@test.com").ReturnsNull();

        var (success, message, data) = await _sut.UserLoginAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Incorrect username or password");
        data.Should().BeNull();
    }

    [Fact]
    public async Task UserLoginAsync_UserInactive_ReturnsFalse()
    {
        var dto = new UserCredentialsDto("user@test.com", "pass");
        _appUserRepo.GetByUserNameAsync("user@test.com")
            .Returns(new AppUserBuilder().WithActive(false).Build());

        var (success, message, data) = await _sut.UserLoginAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("An error has occurred. Please contact the administrator");
        data.Should().BeNull();
    }

    [Fact]
    public async Task UserLoginAsync_WrongPassword_ReturnsFalse()
    {
        var dto = new UserCredentialsDto("user@test.com", "wrong");
        var user = new AppUserBuilder().WithPassword("hashed").Build();
        _appUserRepo.GetByUserNameAsync("user@test.com").Returns(user);
        _hasher.Verify("wrong", "hashed").Returns(false);

        var (success, message, data) = await _sut.UserLoginAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Incorrect username or password");
        data.Should().BeNull();
    }

    [Fact]
    public async Task UserLoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var dto = new UserCredentialsDto("user@test.com", "pass");
        var user = new AppUserBuilder()
            .WithCode(1001)
            .WithEmail("user@test.com")
            .WithPassword("hashed")
            .Build();
        _appUserRepo.GetByUserNameAsync("user@test.com").Returns(user);
        _hasher.Verify("pass", "hashed").Returns(true);

        var (success, message, data) = await _sut.UserLoginAsync(dto);

        success.Should().BeTrue();
        message.Should().Be("Login successful");
        data.Should().NotBeNull();
        data!.DocumentNumber.Should().Be(1001);
    }

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
        _emailSender.SendEmailWithTemplateAsync("RecoveryToken", "user@test.com", Arg.Any<object>()).Returns(true);
        _uriService.ApplicationURL.Returns("https://app.test");

        var (success, message) = await _sut.AccountRecoveryStartAsync("user@test.com");

        success.Should().BeTrue();
        message.Should().Be("email sent to account user@test.com");
        _encryption.Received(1).Encrypt(Arg.Is<string>(token => IsValidRecoveryTokenPlain(token)));
        await _appUserRepo.Received(1).UpdateAsync(Arg.Is<AppUser>(u =>
            u.RecoveryToken == "encrypted_token" && u.RecoveryTokenActive));
    }

    [Fact]
    public async Task AccountRecoveryStartAsync_EmailSendFails_ReturnsFalse()
    {
        var user = new AppUserBuilder().WithEmail("user@test.com").Build();
        _appUserRepo.GetByUserNameAsync("user@test.com").Returns(user);
        _encryption.Encrypt(Arg.Any<string>()).Returns("encrypted_token");
        _emailSender.SendEmailWithTemplateAsync("RecoveryToken", "user@test.com", Arg.Any<object>()).Returns(false);
        _uriService.ApplicationURL.Returns("https://app.test");

        var (success, message) = await _sut.AccountRecoveryStartAsync("user@test.com");

        success.Should().BeFalse();
        message.Should().Be("An error occurred while sending the email");
        await _appUserRepo.DidNotReceive().UpdateAsync(Arg.Any<AppUser>());
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_EmptyToken_ReturnsFalse()
    {
        var dto = new AccountRecoveryDto("", "newPass", "newPass");

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid token");
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_PasswordMismatch_ReturnsFalse()
    {
        var dto = new AccountRecoveryDto("encrypted", "newPass", "differentPass");

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Passwords do not match");
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_DecryptFails_ReturnsFalse()
    {
        var dto = new AccountRecoveryDto("encrypted", "newPass", "newPass");
        _encryption.Decrypt("encrypted").Returns(_ => throw new InvalidOperationException("bad token"));

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid token");
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_MalformedToken_ReturnsFalse()
    {
        var dto = new AccountRecoveryDto("encrypted", "newPass", "newPass");
        _encryption.Decrypt("encrypted").Returns("malformed-token");

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
            u.Password == "new_hashed" &&
            u.RecoveryToken == null &&
            !u.RecoveryTokenActive));
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
        await _appUserRepo.Received(1).UpdateAsync(Arg.Is<AppUser>(u =>
            u.RecoveryToken == null &&
            !u.RecoveryTokenActive));
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_FutureToken_ReturnsFalse()
    {
        var ticks = DateTime.UtcNow.AddMinutes(5).ToBinary();
        var plainToken = $"{ticks}:user@test.com";
        var dto = new AccountRecoveryDto("encrypted", "newPass", "newPass");
        var user = new AppUserBuilder().WithEmail("user@test.com").WithRecoveryToken("encrypted").Build();

        _encryption.Decrypt("encrypted").Returns(plainToken);
        _appUserRepo.GetByRecoveryTokenAsync("encrypted").Returns(user);

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid or expired token");
        await _appUserRepo.Received(1).UpdateAsync(Arg.Is<AppUser>(u =>
            u.RecoveryToken == null &&
            !u.RecoveryTokenActive));
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_TokenEmailMismatch_ReturnsFalse()
    {
        var ticks = DateTime.UtcNow.AddMinutes(-5).ToBinary();
        var plainToken = $"{ticks}:user@test.com";
        var dto = new AccountRecoveryDto("encrypted", "newPass", "newPass");
        var user = new AppUserBuilder().WithEmail("other@test.com").WithRecoveryToken("encrypted").Build();

        _encryption.Decrypt("encrypted").Returns(plainToken);
        _appUserRepo.GetByRecoveryTokenAsync("encrypted").Returns(user);

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid token");
    }

    [Fact]
    public async Task AccountRecoveryCompleteAsync_InvalidBinaryTimestamp_ReturnsFalse()
    {
        var dto = new AccountRecoveryDto("encrypted", "newPass", "newPass");
        _encryption.Decrypt("encrypted").Returns($"{long.MaxValue}:user@test.com");

        var (success, message) = await _sut.AccountRecoveryCompleteAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid token");
    }
}
