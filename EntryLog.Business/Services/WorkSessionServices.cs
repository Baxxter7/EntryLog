using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mappers;
using EntryLog.Business.QueryFilters;
using EntryLog.Business.Specs;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;

namespace EntryLog.Business.Services;

internal class WorkSessionServices : IWorkSessionServices
{
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly IAppUserRepository _appUserRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILoadImagesService _loadImageService;

    public WorkSessionServices(
        IWorkSessionRepository workSessionRepository,
        IAppUserRepository appUserRepository,
        IEmployeeRepository employeeRepository,
        ILoadImagesService loadImagesService)
    {
        _workSessionRepository = workSessionRepository;
        _appUserRepository = appUserRepository;
        _employeeRepository = employeeRepository;
        _loadImageService = loadImagesService;
    }

    public async Task<(bool success, string message)> ClosedJobSessionAsync(CloseJobSessionDto sessionDto)
    {
        int code = int.Parse(sessionDto.UserId);
        var (sucess, message) = await ValidateEmployeeUserAsync(code);

        if (!sucess)
            return (sucess, message);

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

    public async Task<IEnumerable<GetWorkSessionDto>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter)
    {
        var spec = new WorkSessionSpec();

        if (filter.EmployeeId.HasValue)
            spec.AndAlso(x => x.EmployeeId == filter.EmployeeId.Value);

        IEnumerable<WorkSession> sessions = await _workSessionRepository.GetAllAsync(spec);

        return sessions.Select(WorkSessionMapper.MapToGetWorkSessionDto);
    }

    public async Task<(bool success, string message)> OpenJobSessionAsync(CreateJobSessionDto sessionDto)
    {
        int code = int.Parse(sessionDto.UserId);
        var (sucess, message) = await ValidateEmployeeUserAsync(code);

        if (!sucess)
            return (sucess, message);

        WorkSession session = await _workSessionRepository.GetActiveSessionByEmployeeIdAsync(code);

        if (session is not null)
        {
            return (false, "El empleado tiene una sesion activa");
        }

        string filename = sessionDto.Image.FileName;
        string extension = Path.GetExtension(sessionDto.Image.FileName);

        ImageBBResponseDto? imageBB = await _loadImageService
            .UploadAsync(sessionDto.Image.OpenReadStream(), sessionDto.Image.ContentType, filename, extension);

        session = new WorkSession
        {
            EmployeeId = code,
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
                PhotoUrl = imageBB.Data.Url
            },
            Status = SessionStatus.InProgress
        };

        await _workSessionRepository.CreateAsync(session);

        return (true, "Session abierta exitosamente");
    }

    private async Task<(bool success, string message)> ValidateEmployeeUserAsync(int code)
    {
        Employee? employee = await _employeeRepository.GetByCodeAsync(code);

        if (employee == null)
            return (false, "Empleado no encontrado");

        AppUser? user = await _appUserRepository.GetByCodeAsync(code);

        if (user == null)
            return (false, "Usuario no encontrado");

        return (true, "");
    }

}
