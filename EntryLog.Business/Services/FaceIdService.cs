using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;
using System.Text.Json;

namespace EntryLog.Business.Services;

internal class FaceIdService : IFaceIdService
{
    private readonly IAppUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILoadImagesService _loadImagesService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJwtService _jwtService;

    private const int DescriptorLength = 128;

    public FaceIdService(IAppUserRepository userRepository, 
        IEmployeeRepository employeeRepository, 
        ILoadImagesService loadImagesService,
        IHttpClientFactory httpClientFactory, 
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _loadImagesService = loadImagesService;
        _httpClientFactory = httpClientFactory;
        _jwtService = jwtService;
    }

    public async Task<(bool success, string message, EmployeeFaceIdDto? data)> CreateEmployeeFaceIdAsync(AddEmployeeFaceIdDto faceIdDto)
    {
        if (faceIdDto.image is null || faceIdDto.image.Length == 0)
            return (false, "Image is required", null);

        if (string.IsNullOrWhiteSpace(faceIdDto.Descriptor))
            return (false, "Descriptor is required", null);

        List<float> descriptor = new List<float>();
        try
        {
            descriptor = JsonSerializer.Deserialize<List<float>>(faceIdDto.Descriptor);
        }
        catch (JsonException ex) 
        {
            return (false, $"Descriptor format error: {ex.Message}", null);
        }

        if (descriptor is null || descriptor.Count != DescriptorLength)
            return (false, "Descriptor lenght non validad", null);

        var userTask = _userRepository.GetByCodeAsync(faceIdDto.EmployeeCode);
        var employeeTask = _employeeRepository.GetByCodeAsync(faceIdDto.EmployeeCode);

        await Task.WhenAll(userTask,  employeeTask);
        AppUser user = await userTask;
        Employee employee = await employeeTask;

        if (employee is null)
            return (false,"ha ocurrido un problema al obtener el employee", null);

        if(user is null)
            return (false,"ha ocurrido un problema al obtener el user", null);

        if (user.FaceID is not null && user.FaceID.Active)
            return (false, "FaceID ya configurado", null);

    }

    public Task<string> GenerateReferenceImageTokenAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<EmployeeFaceIdDto> GetFaceIdAsync(int employeeCode)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetReferenceImageAsync(string authHeader)
    {
        throw new NotImplementedException();
    }
}
