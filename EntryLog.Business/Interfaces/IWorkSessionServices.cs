using EntryLog.Business.DTOs;
using EntryLog.Business.QueryFilters;
using Microsoft.AspNetCore.Http;

namespace EntryLog.Business.Interfaces;

public interface IWorkSessionServices
{
    Task<(bool success, string message)> OpenJobSession(CreateJobSessionDto sessionDto);
    Task<(bool success, string message)> ClosedJobSession(CloseJobSessionDto sessionDto);
    Task<IEnumerable<GetWorkSessionDto>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter);

    //TODO: ELIMINAR DESPUES
    Task<(bool success, string message)> ImageTestAsync(IFormFile image);
}
