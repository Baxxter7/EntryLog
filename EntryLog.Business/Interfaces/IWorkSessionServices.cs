using EntryLog.Business.DTOs;

namespace EntryLog.Business.Interfaces;

public interface IWorkSessionServices
{
    Task<(bool success, string message)> OpenJobSession(WorkSessionParameters parameters);
    Task<(bool success, string message)> ClosedJobSession(WorkSessionParameters parameters);
}
