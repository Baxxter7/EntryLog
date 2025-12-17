using EntryLog.Business.DTOs;
using EntryLog.Business.QueryFilters;

namespace EntryLog.Business.Interfaces;

public interface IWorkSessionServices
{
    Task<(bool success, string message)> OpenJobSession(CreateJobSessionDto sessionDto);
    Task<(bool success, string message)> ClosedJobSession(CloseJobSessionDto sessionDto);
    Task<IEnumerable<object>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter);
}
