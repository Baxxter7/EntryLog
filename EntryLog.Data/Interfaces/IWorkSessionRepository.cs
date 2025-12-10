using EntryLog.Entities.POCOEntities;

namespace EntryLog.Data.Interfaces;

public interface IWorkSessionRepository
{
    Task CreateAsync(WorkSession workSession);
    Task UpdateAsync(WorkSession workSession);
    Task<WorkSession?> GetByIdAsync(Guid id);
    Task<WorkSession?> GetByEmpleadoAsync(int id);
    Task<IEnumerable<WorkSession>> GetAllAsync();
}
