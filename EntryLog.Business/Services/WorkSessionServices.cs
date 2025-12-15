using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Specs;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Services;

public class WorkSessionServices : IWorkSessionServices
{
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly IAppUserRepository _appUserRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public WorkSessionServices(
        IWorkSessionRepository workSessionRepository,
        IAppUserRepository appUserRepository,
        IEmployeeRepository employeeRepository)
    {
        _workSessionRepository = workSessionRepository;
        _appUserRepository = appUserRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<(bool success, string message)> ClosedJobSession(CloseJobSessionDTO sessionDTO)
    {
        int code = int.Parse(sessionDTO.UserId);
        Employee? employee = await _employeeRepository.GetByCodeAsync(code);

        if (employee == null)
            return (false, "Empleado no encontrado");

        AppUser? user = await _appUserRepository.GetByCodeAsync(code);

        if (user == null)
            return (false, "Usuario no encontrado");

        WorkSessionSpec spec = new WorkSessionSpec();
        spec.AndAlso(x => x.Status == SessionStatus.InProgress);

        IEnumerable<WorkSession> sessions = await _workSessionRepository.GetAllAsync(spec);

        if (!sessions.Any()) {
            return (false, "Ha ocurrido un error al cerrar la session");
        }

        Guid id = Guid.Parse(sessionDTO.SessionId);
        
        WorkSession? session = await _workSessionRepository.GetByIdAsync(id);
        session.CheckOut ??= new Check();
        session.CheckOut.Method = sessionDTO.Method;
        session.CheckOut.DeviceName = sessionDTO.DeviceName;
        session.CheckOut.Date = DateTime.UtcNow;
        session.CheckOut.Location.Latitude = sessionDTO.Latitude;
        session.CheckOut.Location.Longitude = sessionDTO.Longitude;
        session.CheckOut.Location.IpAddress = sessionDTO.IpAddress;
        session.Status = SessionStatus.Completed;

        await _workSessionRepository.UpdateAsync(session);
        return (true, "Se registro exitosamente la sesión");
    }

    public Task<(bool success, string message)> OpenJobSession(CreateJobSessionDTO sessionDTO)
    {
        throw new NotImplementedException();
    }
}
