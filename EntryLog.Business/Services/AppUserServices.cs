using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;

namespace EntryLog.Business.Services;

internal class AppUserServices : IAppUserServices
{
    public Task<(bool success, string message)> AccountRecoveryCompleteAsync(AccountRecoveryDto recoveryDto)
    {
        throw new NotImplementedException();
    }

    public Task<(bool success, string message)> AccountRecoveryStartAsync(string username)
    {
        throw new NotImplementedException();
    }

    public Task<(bool success, string message, LoginResponseDto? data)> RegisterEmployeeAsync(CreateEmployeeUserDto employeeDto)
    {
        throw new NotImplementedException();
    }

    public Task<(bool success, string message, LoginResponseDto? data)> UserLoginAsync(UserCredentialsDto credentialsDto)
    {
        throw new NotImplementedException();
    }
}
