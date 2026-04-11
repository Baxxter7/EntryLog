using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Services;
using EntryLog.Data.Interfaces;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace EntryLog.Tests.Business.Services;

public class FaceIdServiceTests
{
    private readonly IAppUserRepository _userRepo = Substitute.For<IAppUserRepository>();
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly ILoadImagesService _imageService = Substitute.For<ILoadImagesService>();
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly IJwtService _jwtService = Substitute.For<IJwtService>();
    private readonly FaceIdService _sut;

    public FaceIdServiceTests()
    {
        _sut = new FaceIdService(_userRepo, _employeeRepo, _imageService, _httpClientFactory, _jwtService);
    }

    private static string ValidDescriptorJson() =>
        JsonSerializer.Serialize(Enumerable.Range(0, 128).Select(i => i * 0.01f).ToList());

    private static IFormFile CreateMockFormFile()
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("face.png");
        file.ContentType.Returns("image/png");
        file.Length.Returns(1024);
        file.OpenReadStream().Returns(new MemoryStream(new byte[1024]));
        return file;
    }

    private void SetupImageDownload(byte[] bytes, string mediaType = "image/png")
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
        };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
        var client = new HttpClient(new StubHttpMessageHandler(response));
        _httpClientFactory.CreateClient().Returns(client);
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_InvalidCode_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(0, CreateMockFormFile(), ValidDescriptorJson());

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("EmployeeCode is required");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_NullImage_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, null!, ValidDescriptorJson());

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Image is required");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_EmptyDescriptor_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), string.Empty);

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Descriptor is required");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_InvalidDescriptorJson_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), "not-json");

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid descriptor format");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_WrongDescriptorLength_ReturnsFalse()
    {
        var shortDescriptor = JsonSerializer.Serialize(new List<float> { 1.0f, 2.0f });
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), shortDescriptor);

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Descriptor length no valid");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_EmployeeNotFound_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _employeeRepo.GetByCodeAsync(1001).ReturnsNull();

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("An error while fetching the employee");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_UserNotFound_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).ReturnsNull();
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("An error while fetching the user");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_FaceIdAlreadyActive_ReturnsFalse()
    {
        var existingFaceId = new FaceID { ImageUrl = "url", RegisterDate = DateTime.UtcNow, Active = true };
        var user = new AppUserBuilder().WithFaceId(existingFaceId).Build();
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).Returns(user);
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("FaceID is already set up");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_ValidData_UploadsImageAndPersistsFaceId()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        var user = new AppUserBuilder().Build();
        _userRepo.GetByCodeAsync(1001).Returns(user);
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("https://img.test/face.png");
        SetupImageDownload([1, 2, 3, 4]);

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeTrue();
        message.Should().Be("Face ID created successfully");
        data.Should().NotBeNull();
        data!.Base64Image.Should().StartWith("data:image/png;base64,");
        await _userRepo.Received(1).UpdateAsync(Arg.Is<AppUser>(u =>
            u.FaceID != null &&
            u.FaceID.Active &&
            u.FaceID.ImageUrl == "https://img.test/face.png" &&
            u.FaceID.Descriptor.Count == 128));
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_UploadThrows_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(_ => Task.FromException<string?>(new InvalidOperationException("upload failed")));

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Unable to upload image");
        data.Should().BeNull();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_UploadReturnsEmptyUrl_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(string.Empty);

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Unable to upload image");
        data.Should().BeNull();
    }

    [Fact]
    public async Task GenerateReferenceImageTokenAsync_DelegatesToJwtService()
    {
        _jwtService.GenerateTokenAsync("123", "faceid_reference", TimeSpan.FromSeconds(30))
            .Returns("generated_token");

        var result = await _sut.GenerateReferenceImageTokenAsync("123");

        result.Should().Be("generated_token");
    }

    [Fact]
    public async Task GetReferenceImageAsync_EmptyHeader_ReturnsEmpty()
    {
        var result = await _sut.GetReferenceImageAsync(string.Empty);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReferenceImageAsync_InvalidBearerFormat_ReturnsEmpty()
    {
        var result = await _sut.GetReferenceImageAsync("Basic abc123");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReferenceImageAsync_InvalidToken_ReturnsEmpty()
    {
        _jwtService.ValidateToken("bad_token").ReturnsNull();

        var result = await _sut.GetReferenceImageAsync("Bearer bad_token");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReferenceImageAsync_WrongPurpose_ReturnsEmpty()
    {
        var claims = new Dictionary<string, string>
        {
            ["purpose"] = "wrong_purpose",
            ["sub"] = "1001"
        };
        _jwtService.ValidateToken("token").Returns(claims);

        var result = await _sut.GetReferenceImageAsync("Bearer token");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetReferenceImageAsync_ValidToken_ReturnsStoredBase64Image()
    {
        var claims = new Dictionary<string, string>
        {
            ["purpose"] = "faceid_reference",
            ["sub"] = "1001"
        };
        var user = new AppUserBuilder().WithFaceId(new FaceID
        {
            ImageUrl = "https://img.test/face.png",
            RegisterDate = DateTime.UtcNow,
            Descriptor = Enumerable.Range(0, 128).Select(i => i * 0.01f).ToList(),
            Active = true
        }).Build();

        _jwtService.ValidateToken("token").Returns(claims);
        _userRepo.GetByCodeAsync(1001).Returns(user);
        SetupImageDownload([1, 2, 3, 4]);

        var result = await _sut.GetReferenceImageAsync("Bearer token");

        result.Should().StartWith("data:image/png;base64,");
    }

    [Fact]
    public async Task GetReferenceImageAsync_InvalidSubClaim_ReturnsEmpty()
    {
        var claims = new Dictionary<string, string>
        {
            ["purpose"] = "faceid_reference",
            ["sub"] = "not-a-number"
        };
        _jwtService.ValidateToken("token").Returns(claims);

        var result = await _sut.GetReferenceImageAsync("Bearer token");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateEmployeeFaceIdAsync_ImageDownloadFails_ReturnsFalse()
    {
        var dto = new AddEmployeeFaceIdDto(1001, CreateMockFormFile(), ValidDescriptorJson());
        _userRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns("https://img.test/face.png");

        var response = new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new ByteArrayContent([])
        };
        var client = new HttpClient(new StubHttpMessageHandler(response));
        _httpClientFactory.CreateClient().Returns(client);

        var (success, message, data) = await _sut.CreateEmployeeFaceIdAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Unable to process image");
        data.Should().BeNull();
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public StubHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }
}
