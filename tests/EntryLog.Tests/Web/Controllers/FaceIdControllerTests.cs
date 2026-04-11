using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Security.Claims;
using System.Text.Json;

namespace EntryLog.Tests.Web.Controllers;

public class FaceIdControllerTests
{
    private readonly IFaceIdService _faceIdService = Substitute.For<IFaceIdService>();
    private readonly FaceIdController _sut;

    public FaceIdControllerTests()
    {
        _sut = new FaceIdController(_faceIdService);
        _sut.TempData = Substitute.For<ITempDataDictionary>();
        SetupAuthenticatedUser(1001, "john@test.com", "Employee", "John Doe");
    }

    [Fact]
    public async Task Index_ReturnsViewWithFaceIdDto()
    {
        var faceIdDto = new EmployeeFaceIdDto("base64", "01/01/2026", true);
        _faceIdService.GetFaceIdAsync(1001).Returns(faceIdDto);

        var result = await _sut.Index();

        result.Should().BeOfType<ViewResult>();
        var viewResult = (ViewResult)result;
        viewResult.Model.Should().Be(faceIdDto);
    }

    [Fact]
    public async Task GenerateSecurityTokenAsync_AuthenticatedUser_ReturnsOkWithToken()
    {
        _faceIdService.GenerateReferenceImageTokenAsync("1001").Returns("test_token");

        var result = await _sut.GenerateSecurityTokenAsync();

        result.Should().BeOfType<OkObjectResult>();
        var payload = GetPayload((OkObjectResult)result);
        payload.GetProperty("token").GetString().Should().Be("test_token");
    }

    [Fact]
    public async Task GenerateSecurityTokenAsync_UnauthenticatedUser_ReturnsUnauthorized()
    {
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await _sut.GenerateSecurityTokenAsync();

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CreateAsync_UsesAuthenticatedUserCodeAndReturnsJson()
    {
        var file = Substitute.For<IFormFile>();
        var input = new AddEmployeeFaceIdDto(9999, file, "descriptor-json");
        var response = new EmployeeFaceIdDto("base64", "01/01/2026", true);
        _faceIdService.CreateEmployeeFaceIdAsync(Arg.Any<AddEmployeeFaceIdDto>())
            .Returns((true, "created", response));

        var result = await _sut.CreateAsync(input);
        var payload = GetPayload(result);

        payload.GetProperty("success").GetBoolean().Should().BeTrue();
        payload.GetProperty("message").GetString().Should().Be("created");
        await _faceIdService.Received(1).CreateEmployeeFaceIdAsync(Arg.Is<AddEmployeeFaceIdDto>(dto =>
            dto.EmployeeCode == 1001 &&
            dto.image == file &&
            dto.Descriptor == "descriptor-json"));
    }

    [Fact]
    public async Task GetReferenceImageAsync_AuthenticatedUser_ReturnsOkWithImage()
    {
        _faceIdService.GetReferenceImageAsync("Bearer token").Returns("data:image/png;base64,AAAA");

        var result = await _sut.GetReferenceImageAsync("Bearer token");

        result.Should().BeOfType<OkObjectResult>();
        var payload = GetPayload((OkObjectResult)result);
        payload.GetProperty("imageBase64").GetString().Should().Be("data:image/png;base64,AAAA");
    }

    private void SetupAuthenticatedUser(int code, string email, string role, string name)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, code.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(ClaimTypes.Name, name)
        };
        var identity = new ClaimsIdentity(claims, "test");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    private static JsonElement GetPayload(OkObjectResult result)
    {
        var json = JsonSerializer.Serialize(result.Value);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static JsonElement GetPayload(JsonResult result)
    {
        var json = JsonSerializer.Serialize(result.Value);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
