using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Data.Interfaces;

namespace EntryLog.Business.Services;

internal class FaceIdService : IFaceIdService
{
    private readonly IAppUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILoadImagesService _loadImagesService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJwtService _jwtService;

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

    public Task<(bool success, string message, EmployeeFaceIdDto? data)> CreateEmployeeFaceIdAsync(AddEmployeeFaceIdDto faceIdDto)
    {
        throw new NotImplementedException();
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
