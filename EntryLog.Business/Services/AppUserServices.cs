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

    public AppUserServices(IEmployeeRepository employeeRepository, IAppUserRepository appUserRepository, IPasswordHasherService hasherService)
    {
        _employeeRepository = employeeRepository;
        _appUserRepository = appUserRepository;
        _hasherService = hasherService;
    }
    public Task<(bool success, string message)> AccountRecoveryCompleteAsync(AccountRecoveryDto recoveryDto)
    {
        throw new NotImplementedException();
    }

    public Task<(bool success, string message)> AccountRecoveryStartAsync(string username)
    {
        throw new NotImplementedException();
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
            //Password = employeeDto.Password,
            Password = _hasherService.Hash(employeeDto.Password),
            Attempts = 0,
            RecoveryTokenActive = false,
            Active = true
        };

        await _appUserRepository.CreateAsync(user);

        return (true, "Login Successfull", new LoginResponseDto(user.Code, user.Role.ToString(), user.Email));
    }

    public Task<(bool success, string message, LoginResponseDto? data)> UserLoginAsync(UserCredentialsDto credentialsDto)
    {
        throw new NotImplementedException();
    }
}
