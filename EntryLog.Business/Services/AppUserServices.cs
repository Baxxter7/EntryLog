using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
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

    public AppUserServices(
        IEmployeeRepository employeeRepository,
        IAppUserRepository appUserRepository,
        IPasswordHasherService hasherService,
        IEncryptionService encryptionService,
        IEmailSenderService emailSenderService)
    {
        _employeeRepository = employeeRepository;
        _appUserRepository = appUserRepository;
        _hasherService = hasherService;
        _encryptionService = encryptionService;
        _emailSenderService = emailSenderService;
    }
    public Task<(bool success, string message)> AccountRecoveryCompleteAsync(AccountRecoveryDto recoveryDto)
    {
        throw new NotImplementedException();
    }

    public async Task<(bool success, string message)> AccountRecoveryStartAsync(string username)
    {
        AppUser user = await _appUserRepository.GetByUserNameAsync(username);

        if (user is null)
            return (false, "Account recovery was not successful.");

        if(!user.Active)
            return (false, "Account recovery was not successful.");

        string recoveryTokenPlain = $"{DateTime.UtcNow.Ticks}:{user.Email}";

        string recoveryToken = _encryptionService.Encrypt(recoveryTokenPlain);

        user.RecoveryToken = recoveryToken;
        user.RecoveryTokenActive = true;

        await _appUserRepository.UpdateAsync(user);

        bool isSend = await _emailSenderService.SendEmailWithTemplateAsync("RecoveryToken", user.Email);

        return (isSend, isSend ? $"email sent to account {user.Email}" : "An error occurred while sending the email");
    }

    public async Task<(bool success, string message, LoginResponseDto? data)> RegisterEmployeeAsync(CreateEmployeeUserDto employeeDto)
    {
        if(!int.TryParse(employeeDto.DocumentNumber, out int code))
            return (false, "Número de documento inválido", null);

        Employee? employee = await _employeeRepository.GetByCodeAsync(code);

        if (employee == null)
            return (false, "Empleado no encontrado", null);

        AppUser? user = await _appUserRepository.GetByCodeAsync(code);

        if (user != null)
            return (false, "El empleado ya tiene un usuario registrado", null);

        user = await _appUserRepository.GetByUserNameAsync(employeeDto.UserName);

        if (user != null)
            return (false, "El usuario ya existe", null);

       if(string.IsNullOrEmpty(employeeDto.Password) || employeeDto.Password != employeeDto.PasswordConf) 
            return (false, "El usuario ya existe", null);

        user = new AppUser
        {
            Code = code,
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

        if(!user.Active)
            return (false, "An error has occurred. Please contact the administrator", null);

        bool accessGranted = _hasherService.Verify(credentialsDto.Password, user.Password);

        if (!accessGranted)
            return (false, "Incorrect username or password", null);

        return (true, "Login successful", new LoginResponseDto(user.Code, user.Role.ToString(), user.Email));
    }
}
