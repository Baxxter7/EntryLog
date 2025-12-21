using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces;

public interface IAppUserServices
{
    Task<(bool sucess, string message)> RegisterEmployeeUserAsync(CreateEmployeeUserDto employeeDto);
}
