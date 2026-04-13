using EntryLog.Business.DTOs;
using EntryLog.Business.Enums;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mappers;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;
using EntryLog.Business.Specs;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using System.Text.Json;

namespace EntryLog.Business.Services;

internal class WorkSessionServices : IWorkSessionServices
{
    private readonly IWorkSessionRepository _workSessionRepository;
    private readonly IAppUserRepository _appUserRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILoadImagesService _loadImageService;
    private readonly IUriService _uriService;

    private int DescriptorLength = 128;

    public WorkSessionServices(
        IWorkSessionRepository workSessionRepository,
        IAppUserRepository appUserRepository,
        IEmployeeRepository employeeRepository,
        ILoadImagesService loadImagesService,
        IUriService uriService)
    {
        _workSessionRepository = workSessionRepository;
        _appUserRepository = appUserRepository;
        _employeeRepository = employeeRepository;
        _loadImageService = loadImagesService;
        _uriService = uriService;
    }

    public async Task<(bool success, string message, GetWorkSessionDto? data)> ClosedSessionAsync(CloseWorkSessionDto sessionDto)
    {
        if (!int.TryParse(sessionDto.UserId, out int code))
            return (false, "Invalid user id", null);

        var (sucess, message) = await ValidateEmployeeUserAsync(code);

        if (!sucess)
            return (sucess, message, null);

        WorkSession? activeSession = await _workSessionRepository.GetActiveSessionByEmployeeIdAsync(code);

        if (activeSession is null)
            return (false, "There is no active session for the user", null);

        List<float>? descriptor;
        try
        {
            descriptor = JsonSerializer.Deserialize<List<float>>(sessionDto.Descriptor);
        }
        catch (Exception)
        {

            throw;
        }

        if (descriptor is null)
            throw new InvalidOperationException("Invalid descriptor format");

        string extension = Path.GetExtension(sessionDto.Image.FileName);
        string filename = $"checkout-{DateTime.UtcNow}{extension}";

        string? imageBBUrl;

        try
        {
            imageBBUrl = await _loadImageService
                .UploadAsync(sessionDto.Image.OpenReadStream(), sessionDto.Image.ContentType, filename);
        }
        catch
        {
            return (false, "Unable to upload image", null);
        }

        if (string.IsNullOrEmpty(imageBBUrl))
            return (false, "Unable to upload image", null);

        activeSession.CheckOut ??= new Check();
        activeSession.CheckOut.Method = _uriService.UserAgent;
        activeSession.CheckOut.DeviceName = _uriService.Platform;
        activeSession.CheckOut.Date = DateTime.UtcNow;
        activeSession.CheckOut.Location.Latitude = sessionDto.Latitude;
        activeSession.CheckOut.Location.Longitude = sessionDto.Longitude;
        activeSession.CheckOut.Location.IpAddress = _uriService.RemoteIpAddress;
        activeSession.CheckOut.PhotoUrl = imageBBUrl;
        activeSession.CheckOut.Notes = sessionDto.Notes;
        activeSession.CheckOut.Descriptor = descriptor!;
        activeSession.Status = SessionStatus.Completed;

        await _workSessionRepository.UpdateAsync(activeSession);
        return (true, "Session closed successfully", WorkSessionMapper.MapToGetWorkSessionDto(activeSession));
    }

    public async Task<PaginatedResult<GetWorkSessionDto>> GetSessionListByFilterAsync(WorkSessionQueryFilter filter)
    {
        var spec = new WorkSessionSpec();

        if (filter.EmployeeId.HasValue)
            spec.AndAlso(x => x.EmployeeId == filter.EmployeeId.Value);

        //Contar los registros que coinciden con ese filtro
        int count = await _workSessionRepository.CountAsync(spec);

        //Aplicar la paginación
        filter.PageIndex ??= 1;
        filter.PageSize = 10;
        filter.PageSize = Math.Min(filter.PageSize.Value, 50);

        spec.ApplyPaging(filter.PageSize.Value, filter.PageSize.Value * (filter.PageIndex.Value - 1));

        //Aplicar ordenamiento
        switch (filter.Sort)
        {
            case SortType.Ascending:
                spec.AndOrderBy(x => x.CheckIn.Date);
                break;
            case SortType.Descending:
                spec.AndOrderByDescending(x => x.CheckIn.Date);
                break;
            default:
                spec.AndOrderByDescending(x => x.CheckIn.Date);
                break;

        }

        //Paginacion en base de datos
        IEnumerable<WorkSession> sessions = await _workSessionRepository.GetAllAsync(spec);

        IEnumerable<GetWorkSessionDto> results = sessions.Select(WorkSessionMapper.MapToGetWorkSessionDto);

        return PaginatedResult<GetWorkSessionDto>.Create(results, filter, count);
    }

    public async Task<(bool success, string message, GetWorkSessionDto? data)> OpenSessionAsync(CreateWorkSessionDto sessionDto)
    {
        if (!int.TryParse(sessionDto.UserId, out int code))
            return (false, "Invalid user id", null);

        var (success, message) = await ValidateEmployeeUserAsync(code);

        if (!success)
            return (success, message, null);

        WorkSession session = await _workSessionRepository.GetActiveSessionByEmployeeIdAsync(code);

        if (session is not null)
        {
            return (false, "The employee has an active session", null);
        }

        List<float>? descriptor;

        try
        {
            descriptor = JsonSerializer.Deserialize<List<float>>(sessionDto.descriptor);
        }
        catch (Exception)
        {
            return (false, "Descriptor is invalid", null);
        }

        if (descriptor is null || descriptor.Count != DescriptorLength)
            return (false, "Descriptor is invalid", null);

        string extension = Path.GetExtension(sessionDto.Image.FileName);
        string filename = $"checkIn-{DateTime.UtcNow}{extension}";

        string? imageBBUrl;

        try
        {
            imageBBUrl = await _loadImageService
                .UploadAsync(sessionDto.Image.OpenReadStream(), sessionDto.Image.ContentType, filename);
        }
        catch
        {
            return (false, "Unable to upload image", null);
        }

        if (string.IsNullOrEmpty(imageBBUrl))
            return (false, "Unable to upload image", null);

        (success, message) = await ValidateEmployeeFaceDescriptorAsync(code, [.. descriptor]);
        if (!success)
            return (success, message, null);

        session = new WorkSession
        {
            EmployeeId = code,
            CheckIn = new Check
            {
                Method = _uriService.UserAgent,
                DeviceName = _uriService.Platform,
                Date = DateTime.UtcNow,
                Location = new Location
                {
                    Latitude = sessionDto.Latitude,
                    Longitude = sessionDto.Longitude,
                    IpAddress = _uriService.RemoteIpAddress,
                },
                Notes = sessionDto.Notes ?? null,
                Descriptor = descriptor,
                PhotoUrl = imageBBUrl
            },
            Status = SessionStatus.InProgress
        };

        await _workSessionRepository.CreateAsync(session);

        return (true, "Session opened successfully", WorkSessionMapper.MapToGetWorkSessionDto(session));
    }

    private async Task<(bool success, string message)> ValidateEmployeeUserAsync(int code)
    {
        Employee? employee = await _employeeRepository.GetByCodeAsync(code);

        if (employee == null)
            return (false, "Employee not found");

        AppUser? user = await _appUserRepository.GetByCodeAsync(code);

        if (user == null)
            return (false, "User not found");

        return (true, "");
    }

    private async Task<(bool success, string message)> ValidateEmployeeFaceDescriptorAsync(int employeeCode, float[] currentDescriptor)
    {
        if (currentDescriptor is null || currentDescriptor.Length != 128)
            return (false, "Descriptor no válido");

        AppUser? user = await _appUserRepository.GetByCodeAsync(employeeCode);
        List<float>? storeDescriptor = user?.FaceID?.Descriptor;

        if (storeDescriptor is null)
            return (false, "FaceId no configurado");

        double distance = EuclideanDistance(currentDescriptor, [.. storeDescriptor]);
        bool match = distance < 0.5;

        return (match, match ? "" : "El rostro no coincide con el FaceId registrado");

    }
    private static double EuclideanDistance(float[] currentDescriptor, float[] storeDescriptor)
    {
        double sum = 0;
        for (int i = 0; i < currentDescriptor.Length; i++)
            sum += Math.Pow(currentDescriptor[i] - storeDescriptor[i], 2);
        return Math.Sqrt(sum);

    }

    public async Task<bool> HasActiveAnySessionAsync(int employeeCode)
    {
        WorkSession session = await _workSessionRepository.GetActiveSessionByEmployeeIdAsync(employeeCode);
        return session is not null;
    }

    public async Task<GetWorkSessionDto?> GetSessionByIdAsync(string id)
    {
        Guid guid = Guid.Parse(id);

        WorkSession? session = await _workSessionRepository.GetByIdAsync(guid);
        return WorkSessionMapper.MapToGetWorkSessionDto(session!);
    }

    public async Task<IEnumerable<GetLocationDto>> GetLastLocationByEmployeeAsync(int employeeCode)
    {
        var filter = new WorkSessionQueryFilter
        {
            EmployeeId = employeeCode,
            Sort = SortType.Descending,
            PageIndex = 1,
            PageSize = 5
        };

        PaginatedResult<GetWorkSessionDto> paginatedResult = await GetSessionListByFilterAsync(filter);

        IEnumerable<GetWorkSessionDto>? lastSessions = paginatedResult.ResultsCount > 0 ? paginatedResult.Results : [];

        IEnumerable<GetLocationDto> lastCheckInLocations = lastSessions?.Select(session => session.CheckIn.Location) ?? [];

        return lastCheckInLocations;
    }
}
