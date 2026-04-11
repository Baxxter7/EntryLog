using EntryLog.Business.DTOs;
using EntryLog.Business.Enums;
using EntryLog.Business.Interfaces;
using EntryLog.Business.QueryFilters;
using EntryLog.Business.Services;
using EntryLog.Data.Interfaces;
using EntryLog.Data.Specifications;
using EntryLog.Entities.Enums;
using EntryLog.Entities.POCOEntities;
using EntryLog.Tests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Text.Json;

namespace EntryLog.Tests.Business.Services;

public class WorkSessionServicesTests
{
    private readonly IWorkSessionRepository _sessionRepo = Substitute.For<IWorkSessionRepository>();
    private readonly IAppUserRepository _appUserRepo = Substitute.For<IAppUserRepository>();
    private readonly IEmployeeRepository _employeeRepo = Substitute.For<IEmployeeRepository>();
    private readonly ILoadImagesService _imageService = Substitute.For<ILoadImagesService>();
    private readonly IUriService _uriService = Substitute.For<IUriService>();
    private readonly WorkSessionServices _sut;

    public WorkSessionServicesTests()
    {
        _uriService.UserAgent.Returns("TestAgent");
        _uriService.Platform.Returns("TestPlatform");
        _uriService.RemoteIpAddress.Returns("127.0.0.1");

        _sut = new WorkSessionServices(
            _sessionRepo,
            _appUserRepo,
            _employeeRepo,
            _imageService,
            _uriService);
    }

    private static IFormFile CreateMockFormFile()
    {
        var file = Substitute.For<IFormFile>();
        file.FileName.Returns("test.jpg");
        file.ContentType.Returns("image/jpeg");
        file.OpenReadStream().Returns(new MemoryStream());
        return file;
    }

    private static string ValidDescriptorJson() =>
        JsonSerializer.Serialize(Enumerable.Range(0, 128).Select(i => i * 0.01f).ToList());

    [Fact]
    public async Task OpenJobSessionAsync_EmployeeNotFound_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).ReturnsNull();

        var (success, message, data) = await _sut.OpenSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Employee not found");
        data.Should().BeNull();
    }

    [Fact]
    public async Task OpenJobSessionAsync_InvalidUserId_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("abc", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());

        var (success, message, data) = await _sut.OpenSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid user id");
        data.Should().BeNull();
    }

    [Fact]
    public async Task OpenJobSessionAsync_UserNotFound_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).ReturnsNull();

        var (success, message, data) = await _sut.OpenSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("User not found");
        data.Should().BeNull();
    }

    [Fact]
    public async Task OpenJobSessionAsync_ActiveSessionExists_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).Returns(new WorkSessionBuilder().Build());

        var (success, message, data) = await _sut.OpenSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("The employee has an active session");
        data.Should().BeNull();
    }

    [Fact]
    public async Task OpenJobSessionAsync_InvalidDescriptor_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), null, "not-json");
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).ReturnsNull();

        var (success, message, data) = await _sut.OpenSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Descriptor is invalid");
        data.Should().BeNull();
        await _imageService.DidNotReceive().UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>());
    }

[Fact]
public async Task OpenJobSessionAsync_ValidData_CreatesSessionAndReturnsDto()
{
    var descriptor = Enumerable.Range(0, 128).Select(i => (float)(i * 0.01f)).ToList();
    var faceId = new FaceID
    {
        ImageUrl = "https://img.test/face.jpg",
        RegisterDate = DateTime.UtcNow,
        Descriptor = descriptor,
        Active = true
    };
    
    var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), "test notes", ValidDescriptorJson());
    _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
    _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().WithFaceId(faceId).Build());
    _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).ReturnsNull();
    _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
    .Returns("https://img.test/photo.jpg");

    var (success, message, data) = await _sut.OpenSessionAsync(dto);

    success.Should().BeTrue();
    message.Should().Be("Session opened successfully");
    data.Should().NotBeNull();
    data!.EmployeeId.Should().Be(1001);
    data.Status.Should().Be("InProgress");
    data.CheckIn.Method.Should().Be("TestAgent");
    data.CheckIn.DeviceName.Should().Be("TestPlatform");
    data.CheckIn.Location.IpAddress.Should().Be("127.0.0.1");
    await _sessionRepo.Received(1).CreateAsync(Arg.Is<WorkSession>(session =>
    session.EmployeeId == 1001 &&
    session.Status == SessionStatus.InProgress &&
    session.CheckIn.Method == "TestAgent" &&
    session.CheckIn.DeviceName == "TestPlatform" &&
    session.CheckIn.Location.IpAddress == "127.0.0.1" &&
    session.CheckIn.Descriptor.Count == 128 &&
    session.CheckIn.PhotoUrl == "https://img.test/photo.jpg"));
}

    [Fact]
    public async Task OpenJobSessionAsync_ImageUploadFails_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), "test notes", ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).ReturnsNull();
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((string?)null);

        var (success, message, data) = await _sut.OpenSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Unable to upload image");
        data.Should().BeNull();
        await _sessionRepo.DidNotReceive().CreateAsync(Arg.Any<WorkSession>());
    }

    [Fact]
    public async Task OpenJobSessionAsync_ImageUploadThrows_ReturnsFalse()
    {
        var dto = new CreateWorkSessionDto("1001", "14.0", "-90.0", CreateMockFormFile(), "test notes", ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).ReturnsNull();
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(_ => Task.FromException<string?>(new InvalidOperationException("upload failed")));

        var (success, message, data) = await _sut.OpenSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Unable to upload image");
        data.Should().BeNull();
    }

    [Fact]
    public async Task ClosedJobSessionAsync_NoActiveSession_ReturnsFalse()
    {
        var dto = new CloseWorkSessionDto(null, "1001", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).ReturnsNull();

        var (success, message, data) = await _sut.ClosedSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("There is no active session for the user");
        data.Should().BeNull();
    }

    [Fact]
    public async Task ClosedJobSessionAsync_InvalidUserId_ReturnsFalse()
    {
        var dto = new CloseWorkSessionDto(null, "abc", "14.0", "-90.0", CreateMockFormFile(), null, ValidDescriptorJson());

        var (success, message, data) = await _sut.ClosedSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Invalid user id");
        data.Should().BeNull();
    }

[Fact]
public async Task ClosedJobSessionAsync_ValidData_ClosesSessionSuccessfully()
{
    var descriptor = Enumerable.Range(0, 128).Select(i => (float)(i * 0.01f)).ToList();
    var faceId = new FaceID
    {
        ImageUrl = "https://img.test/face.jpg",
        RegisterDate = DateTime.UtcNow,
        Descriptor = descriptor,
        Active = true
    };
    
    var activeSession = new WorkSessionBuilder().WithEmployeeId(1001).Build();
    var dto = new CloseWorkSessionDto(null, "1001", "14.0", "-90.0", CreateMockFormFile(), "closing notes", ValidDescriptorJson());
    _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
    _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().WithFaceId(faceId).Build());
    _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).Returns(activeSession);
    _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
    .Returns("https://img.test/checkout.jpg");

    var (success, message, data) = await _sut.ClosedSessionAsync(dto);

    success.Should().BeTrue();
    message.Should().Be("Session closed successfully");
    data.Should().NotBeNull();
    activeSession.Status.Should().Be(SessionStatus.Completed);
    activeSession.CheckOut.Should().NotBeNull();
    activeSession.CheckOut!.Method.Should().Be("TestAgent");
    activeSession.CheckOut.DeviceName.Should().Be("TestPlatform");
    activeSession.CheckOut.Location.IpAddress.Should().Be("127.0.0.1");
    await _sessionRepo.Received(1).UpdateAsync(activeSession);
}

    [Fact]
    public async Task ClosedJobSessionAsync_ImageUploadFails_ReturnsFalse()
    {
        var activeSession = new WorkSessionBuilder().WithEmployeeId(1001).Build();
        var dto = new CloseWorkSessionDto(null, "1001", "14.0", "-90.0", CreateMockFormFile(), "closing notes", ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).Returns(activeSession);
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns((string?)null);

        var (success, message, data) = await _sut.ClosedSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Unable to upload image");
        data.Should().BeNull();
        await _sessionRepo.DidNotReceive().UpdateAsync(Arg.Any<WorkSession>());
    }

    [Fact]
    public async Task ClosedJobSessionAsync_ImageUploadThrows_ReturnsFalse()
    {
        var activeSession = new WorkSessionBuilder().WithEmployeeId(1001).Build();
        var dto = new CloseWorkSessionDto(null, "1001", "14.0", "-90.0", CreateMockFormFile(), "closing notes", ValidDescriptorJson());
        _employeeRepo.GetByCodeAsync(1001).Returns(new EmployeeBuilder().Build());
        _appUserRepo.GetByCodeAsync(1001).Returns(new AppUserBuilder().Build());
        _sessionRepo.GetActiveSessionByEmployeeIdAsync(1001).Returns(activeSession);
        _imageService.UploadAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(_ => Task.FromException<string?>(new InvalidOperationException("upload failed")));

        var (success, message, data) = await _sut.ClosedSessionAsync(dto);

        success.Should().BeFalse();
        message.Should().Be("Unable to upload image");
        data.Should().BeNull();
    }

    [Fact]
    public async Task GetSessionListByFilterAsync_ReturnsPagedResults()
    {
        var sessions = new List<WorkSession> { new WorkSessionBuilder().Build() };
        Specification<WorkSession>? countSpec = null;
        ISpecification<WorkSession>? listSpec = null;

        _sessionRepo.CountAsync(Arg.Any<Specification<WorkSession>>())
            .Returns(callInfo =>
            {
                countSpec = callInfo.Arg<Specification<WorkSession>>();
                return 1;
            });
        _sessionRepo.GetAllAsync(Arg.Any<ISpecification<WorkSession>>())
            .Returns(callInfo =>
            {
                listSpec = callInfo.Arg<ISpecification<WorkSession>>();
                return sessions;
            });

        var filter = new WorkSessionQueryFilter { EmployeeId = 1001, Sort = SortType.Descending };

        var result = await _sut.GetSessionListByFilterAsync(filter);

        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result.ResultsCount.Should().Be(1);
        result.Results.Should().HaveCount(1);
        countSpec.Should().NotBeNull();
        countSpec!.Expression.Compile()(new WorkSessionBuilder().WithEmployeeId(1001).Build()).Should().BeTrue();
        countSpec.Expression.Compile()(new WorkSessionBuilder().WithEmployeeId(2002).Build()).Should().BeFalse();
        listSpec.Should().NotBeNull();
        listSpec!.IsPagingEnabled.Should().BeTrue();
        listSpec.Take.Should().Be(10);
        listSpec.Skip.Should().Be(0);
        listSpec.OrderByDescending.Should().NotBeNull();
    }
}
