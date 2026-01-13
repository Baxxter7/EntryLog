using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces;

public interface IAppUserServices
{
    Task<(bool success, string message, LoginResponseDto? data)> RegisterEmployeeAsync(CreateEmployeeUserDto employeeDto);
    Task<(bool success, string message, LoginResponseDto? data)> UserLoginAsync(UserCredentialsDto credentialsDto);
    Task<(bool success, string message)> AccountRecoveryStartAsync(string username);
    Task<(bool success, string message)> AccountRecoveryCompleteAsync(AccountRecoveryDto recoveryDto);
}
