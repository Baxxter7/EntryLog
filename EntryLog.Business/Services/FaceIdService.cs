using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mappers;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;
using System.Text.Json;

namespace EntryLog.Business.Services;

internal class FaceIdService : IFaceIdService
{
    private readonly IAppUserRepository _userRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILoadImagesService _imageService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IJwtService _jwtService;

    private const int DescriptorLength = 128;

    public FaceIdService(IAppUserRepository userRepository,
        IEmployeeRepository employeeRepository,
        ILoadImagesService imageService,
        IHttpClientFactory httpClientFactory,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _imageService = imageService;
        _httpClientFactory = httpClientFactory;
        _jwtService = jwtService;
    }

    public async Task<(bool success, string message, EmployeeFaceIdDto? data)> CreateEmployeeFaceIdAsync(AddEmployeeFaceIdDto faceIdDto)
    {
        if(faceIdDto.EmployeeCode <= 0)
            return (false, "EmployeeCode is required", null);

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

        await Task.WhenAll(userTask, employeeTask);
        AppUser user = await userTask;
        Employee employee = await employeeTask;

        if (employee is null)
            return (false, "An error while fetching the employee", null);

        if (user is null)
            return (false, "An error while fetching the user", null);

        if (user.FaceID is not null && user.FaceID.Active)
            return (false, "FaceID is already set up", null);

        var ext = Path.GetExtension(faceIdDto.image.FileName);
        if (string.IsNullOrEmpty(ext))
            ext = ".png";

        string fileName = $"faceId-{user.Id}{ext}";
        string imageUrl;

        try
        {
            imageUrl = await _imageService.UploadAsync(faceIdDto.image.OpenReadStream(), faceIdDto.image.ContentType, fileName);
        }
        catch
        {
            return (false, "No se ha podido cargar la imagen", null);
        }

        if (string.IsNullOrEmpty(imageUrl))
            return (false, "No se ha podido cargar la imagen", null);

        string base64Image;
        try
        {
            base64Image = await GenerateBase64PngImageAsync(imageUrl);
        }
        catch (Exception ex)
        {
            return (false, $"No se ha podido generar el base 64, {ex.Message}", null);
        }

        user.FaceID = new FaceID
        {
            ImageUrl = imageUrl,
            RegisterDate = DateTime.UtcNow,
            Descriptor = descriptor,
            Active = true
        };

        await _userRepository.UpdateAsync(user);

        return (true, "Se ha creado correctamente el Face ID", FaceIdMapper.MapToEmployeeFaceIdDto(user.FaceID, base64Image));
    }

    private async Task<string> GenerateBase64PngImageAsync(string imageUrl)
    {
        using var client = _httpClientFactory.CreateClient();
        using var response = await client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

        if(!response.IsSuccessStatusCode)
            return string.Empty;

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet/stream";

        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
        var prefix = $"data:{contentType};base64,";
        return prefix + Convert.ToBase64String(imageBytes);
    }

    public async Task<string> GenerateReferenceImageTokenAsync(string userId)
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
