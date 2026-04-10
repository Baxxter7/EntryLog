using EntryLog.Business.DTOs;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;

namespace EntryLog.Business.Interfaces;

public interface IWorkSessionServices
{
    Task<(bool success, string message, GetWorkSessionDto? data)> OpenSessionAsync(CreateWorkSessionDto sessionDto);
    Task<(bool success, string message, GetWorkSessionDto? data)> ClosedSessionAsync(CloseWorkSessionDto sessionDto);
    Task<GetWorkSessionDto?> GetSessionByIdAsync(string id);
    Task<PaginatedResult<GetWorkSessionDto>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter);
    Task<bool> HasActiveAnySessionAsync(int employeeCode);
    Task<IEnumerable<GetLocationDto>> GetLastLocationByEmployeeAsync(int employeeCode);
}
