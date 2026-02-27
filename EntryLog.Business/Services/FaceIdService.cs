using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Mappers;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;
using System.IdentityModel.Tokens.Jwt;
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
        if (faceIdDto.EmployeeCode <= 0)
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
            return (false, $"Invalid descriptor format", null);
        }

        if (descriptor is null || descriptor.Count != DescriptorLength)
            return (false, "Descriptor length no valid", null);

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
            return (false, "Unable to upload image", null);
        }

        if (string.IsNullOrEmpty(imageUrl))
            return (false, "Unable to upload image", null);

        string base64Image;
        try
        {
            base64Image = await GenerateBase64PngImageAsync(imageUrl);
        }
        catch (Exception ex)
        {
            return (false, $"Unable to process image", null);
        }

        user.FaceID = new FaceID
        {
            ImageUrl = imageUrl,
            RegisterDate = DateTime.UtcNow,
            Descriptor = descriptor,
            Active = true
        };

        await _userRepository.UpdateAsync(user);

        return (true, "Face ID created successfully", FaceIdMapper.MapToEmployeeFaceIdDto(user.FaceID, base64Image));
    }

    private async Task<string> GenerateBase64PngImageAsync(string imageUrl)
    {
        using var client = _httpClientFactory.CreateClient();
        using var response = await client.GetAsync(imageUrl, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
            return string.Empty;

        //"application/octet-stream"
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet/stream";

        byte[] imageBytes = await response.Content.ReadAsByteArrayAsync();
        var prefix = $"data:{contentType};base64,";
        return prefix + Convert.ToBase64String(imageBytes);
    }

    public async Task<string> GenerateReferenceImageTokenAsync(string userId)
    => await _jwtService.GenerateTokenAsync(userId, "faceid_reference", TimeSpan.FromSeconds(30));

    public async Task<EmployeeFaceIdDto> GetFaceIdAsync(int employeeCode)
    {
        AppUser user = await _userRepository.GetByCodeAsync(employeeCode);
        if (user is null)
            return FaceIdMapper.Empty();

        FaceID? faceId = user.FaceID;
        string base64Image = faceId != null ? await GenerateBase64PngImageAsync(faceId!.ImageUrl) : string.Empty;
        EmployeeFaceIdDto faceIdDto = faceId is not null
            ? FaceIdMapper.MapToEmployeeFaceIdDto(faceId, base64Image) : FaceIdMapper.Empty();

        return faceIdDto;
    }

    public async Task<string> GetReferenceImageAsync(string authHeader)
    {
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return string.Empty;

        var token = authHeader.Substring("Bearer ".Length).Trim();
        var claims = _jwtService.ValidateToken(token);

        if (claims is null || !claims.TryGetValue("purpose", out var purpose) || purpose?.ToString() != "faceid_reference")
            return string.Empty;

        if (!claims.TryGetValue(JwtRegisteredClaimNames.Sub, out var nameId) || !int.TryParse(nameId?.ToString(), out var code))
            return string.Empty;

        var faceIdDto = await GetFaceIdAsync(code);
        return faceIdDto?.Base64Image ?? string.Empty;
    }
}
