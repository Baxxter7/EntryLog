using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mailtrap.Models;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Services;

internal class AppUserServices : IAppUserServices
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IAppUserRepository _appUserRepository;
    private readonly IPasswordHasherService _hasherService;
    private readonly IEncryptionService _encryptionService;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IUriService _uriService;

    public AppUserServices(
        IEmployeeRepository employeeRepository,
        IAppUserRepository appUserRepository,
        IPasswordHasherService hasherService,
        IEncryptionService encryptionService,
        IEmailSenderService emailSenderService,
        IUriService uriService)
    {
        _employeeRepository = employeeRepository;
        _appUserRepository = appUserRepository;
        _hasherService = hasherService;
        _encryptionService = encryptionService;
        _emailSenderService = emailSenderService;
        _uriService = uriService;
    }
    public async Task<(bool success, string message)> AccountRecoveryCompleteAsync(AccountRecoveryDto recoveryDto)
    {
        if (string.IsNullOrEmpty(recoveryDto.Token))
            return (false, "Invalid token");

        string recoveryTokenPlain;

        try
        {
            recoveryTokenPlain = _encryptionService.Decrypt(recoveryDto.Token);
        }
        catch (Exception ex)
        {

            return (false, "Invalid token");
        }

        if (string.IsNullOrEmpty(recoveryTokenPlain) || !recoveryTokenPlain.Contains(':'))
            return (false, "Invalid token");

        string[] parts = recoveryTokenPlain.Split(':');

        if (parts.Length != 2 || !long.TryParse(parts[0], out long ticks))
            return (false, "Invalid token");

        string username = parts[1];

        AppUser user = await _appUserRepository.GetByRecoveryTokenAsync(recoveryDto.Token);

        if (user is null || !string.Equals(username, user.Email, StringComparison.OrdinalIgnoreCase))
            return (false, "Invalid token");

        var tokenDate = DateTime.FromBinary(ticks);
        var now = DateTime.UtcNow;

        const int expirationMinutes = 30;

        if ((now - tokenDate).TotalMinutes <= expirationMinutes)
        {
            user.Password = _hasherService.Hash(recoveryDto.Password);
            await FinalizeRecovery(user);
            return (true, "Password updated successfully");
        }
        else
        {
            await FinalizeRecovery(user);
            return (false, "Invalid or expired token");
        }
    }

    private async Task FinalizeRecovery(AppUser user)
    {
        user.RecoveryToken = null;
        user.RecoveryTokenActive = false;
        await _appUserRepository.UpdateAsync(user);
    }

    public async Task<(bool success, string message)> AccountRecoveryStartAsync(string username)
    {
        AppUser user = await _appUserRepository.GetByUserNameAsync(username);

        if (user is null)
            return (false, "Account recovery was not successful.");

        if (!user.Active)
            return (false, "Account recovery was not successful.");

        string recoveryTokenPlain = $"{DateTime.UtcNow.Ticks}:{user.Email}";

        string recoveryToken = _encryptionService.Encrypt(recoveryTokenPlain);

        user.RecoveryToken = recoveryToken;
        user.RecoveryTokenActive = true;

        await _appUserRepository.UpdateAsync(user);

        var vars = new RecoveryAccountVariables
        {
            Name = user.Name,
            Url = $"{_uriService.ApplicationURL}/account/recovery?token={recoveryToken}"
        };

        // bool isSend = await _emailSenderService.SendEmailWithTemplateAsync("RecoveryToken", user.Email, vars);
        bool isSend = true;

        return (isSend, isSend ? $"email sent to account {user.Email}" : "An error occurred while sending the email");
    }

    public async Task<(bool success, string message, LoginResponseDto? data)> RegisterEmployeeAsync(CreateEmployeeUserDto employeeDto)
    {
        int code = int.Parse(employeeDto.DocumentNumber);

        Employee? employee = await _employeeRepository.GetByCodeAsync(code);

        if (employee == null)
            return (false, "Employee not found", null);

        AppUser? user = await _appUserRepository.GetByCodeAsync(code);

        if (user != null)
            return (false, "The employee already has a registered user account", null);

        user = await _appUserRepository.GetByUserNameAsync(employeeDto.UserName);

        if (user != null)
            return (false, "El usuario ya existe", null);

        if (string.IsNullOrEmpty(employeeDto.Password) || employeeDto.Password != employeeDto.PasswordConf)
            return (false, "El usuario ya existe", null);

        user = new AppUser
        {
            Code = code,
            Name = employee.FullName,
            Role = RoleType.Employee,
            Email = employeeDto.UserName,
            CellPhone = employeeDto.CellPhone,
            Password = _hasherService.Hash(employeeDto.Password),
            Attempts = 0,
            RecoveryTokenActive = false,
            Active = true
        };

        await _appUserRepository.CreateAsync(user);

        return (true, "Employee created successfully", new LoginResponseDto(user.Code, user.Role.ToString(), user.Email));
    }

    public async Task<(bool success, string message, LoginResponseDto? data)> UserLoginAsync(UserCredentialsDto credentialsDto)
    {
        AppUser user = await _appUserRepository.GetByUserNameAsync(credentialsDto.Username);
        if (user is null)
            return (false, "Incorrect username or password", null);

        if (!user.Active)
            return (false, "An error has occurred. Please contact the administrator", null);

        bool accessGranted = _hasherService.Verify(credentialsDto.Password, user.Password);

        if (!accessGranted)
            return (false, "Incorrect username or password", null);

        return (true, "Login successful", new LoginResponseDto(user.Code, user.Role.ToString(), user.Email));
    }
}
