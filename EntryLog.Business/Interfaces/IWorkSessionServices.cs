using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces;

public interface IWorkSessionServices
{
    Task<(bool success, string message)> OpenJobSession(CreateJobSessionDTO sessionDTO);
    Task<(bool success, string message)> ClosedJobSession(CloseJobSessionDTO sessionDTO);
}
