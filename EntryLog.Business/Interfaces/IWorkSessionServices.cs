using EntryLog.Business.DTOs;
using EntryLog.Business.QueryFilters;

namespace EntryLog.Business.Interfaces;

public interface IWorkSessionServices
{
    Task<(bool success, string message)> OpenJobSessionAsync(CreateJobSessionDto sessionDto);
    Task<(bool success, string message)> ClosedJobSessionAsync(CloseJobSessionDto sessionDto);
    Task<IEnumerable<GetWorkSessionDto>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter);
}
