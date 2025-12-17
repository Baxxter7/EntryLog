using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.QueryFilters;
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

    public async Task<(bool success, string message)> ClosedJobSession(CloseJobSessionDto sessionDto)
    {
        int code = int.Parse(sessionDto.UserId);
        Employee? employee = await _employeeRepository.GetByCodeAsync(code);

        if (employee == null)
            return (false, "Empleado no encontrado");

        AppUser? user = await _appUserRepository.GetByCodeAsync(code);

        if (user == null)
            return (false, "Usuario no encontrado");

        WorkSession? activeSession = await _workSessionRepository.GetActiveSessionByEmployeeIdAsync(code);

        if (activeSession is null)
            return (false, "No existe sesion activa para el usuario");

        activeSession.CheckOut ??= new Check();
        activeSession.CheckOut.Method = sessionDto.Method;
        activeSession.CheckOut.DeviceName = sessionDto.DeviceName;
        activeSession.CheckOut.Date = DateTime.UtcNow;
        activeSession.CheckOut.Location.Latitude = sessionDto.Latitude;
        activeSession.CheckOut.Location.Longitude = sessionDto.Longitude;
        activeSession.CheckOut.Location.IpAddress = sessionDto.IpAddress;
        activeSession.CheckOut.PhotoUrl = string.Empty;
        activeSession.CheckOut.Notes = sessionDto.Notes;
        activeSession.Status = SessionStatus.Completed;

        await _workSessionRepository.UpdateAsync(activeSession);
        return (true, "Se registro exitosamente la sesión");
    }

    public async Task<IEnumerable<object>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter)
    {
        var spec = new WorkSessionSpec();

        if (filter.EmployeeId.HasValue)
            spec.AndAlso(x => x.EmployeeId == filter.EmployeeId.Value);

        IEnumerable<WorkSession> sessions = await _workSessionRepository.GetAllAsync(spec);

        //Hola
        throw new NotImplementedException();
    }

    public async Task<(bool success, string message)> OpenJobSession(CreateJobSessionDto sessionDto)
    {

        int code = int.Parse(sessionDto.UserId);
        Employee? employee = await _employeeRepository.GetByCodeAsync(code);

        if (employee == null)
            return (false, "Empleado no encontrado");

        AppUser? user = await _appUserRepository.GetByCodeAsync(code);

        if (user == null)
            return (false, "Usuario no encontrado");

        WorkSession session = await _workSessionRepository.GetActiveSessionByEmployeeIdAsync(code);

        if (session is not null)
        {
            return (false, "El empleado tiene una sesion activa");
        }

        session = new WorkSession
        {
            EmployeeId = user.Code,
            CheckIn = new Check
            {
                Method = sessionDto.Method,
                DeviceName = sessionDto.DeviceName,
                Date = DateTime.UtcNow,
                Location = new Location
                {
                    Latitude = sessionDto.Latitude,
                    Longitude = sessionDto.Longitude,
                    IpAddress = sessionDto.IpAddress,
                },
                Notes = sessionDto.Notes ?? null,
                PhotoUrl = string.Empty
            },
            Status = SessionStatus.InProgress
        };

        await _workSessionRepository.CreateAsync(session);

        return (true, "Session abierta exitosamente");
    }
}
