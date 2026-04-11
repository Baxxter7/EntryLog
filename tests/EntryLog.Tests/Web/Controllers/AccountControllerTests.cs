using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using System.Security.Claims;
using System.Text.Json;

namespace EntryLog.Tests.Web.Controllers;

public class AccountControllerTests
{
    private readonly IAppUserServices _appUserServices = Substitute.For<IAppUserServices>();
    private readonly IAuthenticationService _authService = Substitute.For<IAuthenticationService>();
    private readonly AccountController _sut;

    public AccountControllerTests()
    {
        _sut = new AccountController(_appUserServices);

        _authService.SignInAsync(
                Arg.Any<HttpContext>(),
                Arg.Any<string>(),
                Arg.Any<ClaimsPrincipal>(),
                Arg.Any<AuthenticationProperties>())
            .Returns(Task.CompletedTask);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IAuthenticationService)).Returns(_authService);

        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        _sut.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _sut.TempData = Substitute.For<ITempDataDictionary>();
        _sut.Url = Substitute.For<IUrlHelper>();
    }

    [Fact]
    public void RegisterEmployeeUser_ReturnsView()
    {
        var result = _sut.RegisterEmployeeUser();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Login_NotAuthenticated_ReturnsView()
    {
        var result = _sut.Login();

        result.Should().BeOfType<ViewResult>();
    }

    [Fact]
    public void Login_Authenticated_RedirectsToMain()
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "user")], "test");
        _sut.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);

        var result = _sut.Login();

        result.Should().BeOfType<RedirectToActionResult>();
        var redirect = (RedirectToActionResult)result;
        redirect.ActionName.Should().Be("index");
        redirect.ControllerName.Should().Be("main");
    }

    [Fact]
    public async Task LoginAsync_Failed_ReturnsJsonWithError()
    {
        var model = new UserCredentialsDto("user", "pass");
        _appUserServices.UserLoginAsync(model)
            .Returns((false, "Incorrect username or password", (LoginResponseDto?)null));

        var result = await _sut.LoginAsync(model);
        var payload = GetPayload(result);

        payload.GetProperty("success").GetBoolean().Should().BeFalse();
        payload.GetProperty("message").GetString().Should().Be("Incorrect username or password");
    }

    [Fact]
    public async Task LoginAsync_Success_SignsInAndReturnsPath()
    {
        var model = new UserCredentialsDto("user", "pass");
        var loginData = new LoginResponseDto(1001, "Employee", "user@test.com", "John");
        _appUserServices.UserLoginAsync(model).Returns((true, "Login successful", loginData));

        var result = await _sut.LoginAsync(model);
        var payload = GetPayload(result);

        payload.GetProperty("success").GetBoolean().Should().BeTrue();
        payload.GetProperty("path").GetString().Should().Be("/main/index");
        await _authService.Received(1).SignInAsync(
            Arg.Any<HttpContext>(),
            Arg.Any<string>(),
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<AuthenticationProperties>());
    }

    [Fact]
    public async Task RegisterEmployeeUserAsync_Failed_ReturnsJsonWithMessage()
    {
        var model = new CreateEmployeeUserDto("1001", "user@test.com", "555", "pass", "pass");
        _appUserServices.RegisterEmployeeAsync(model)
            .Returns((false, "Employee not found", (LoginResponseDto?)null));

        var result = await _sut.RegisterEmployeeUserAsync(model);
        var payload = GetPayload(result);

        payload.GetProperty("success").GetBoolean().Should().BeFalse();
        payload.GetProperty("message").GetString().Should().Be("Employee not found");
    }

    [Fact]
    public async Task RegisterEmployeeUserAsync_Success_SignsInAndReturnsPath()
    {
        var model = new CreateEmployeeUserDto("1001", "user@test.com", "555", "pass", "pass");
        var loginData = new LoginResponseDto(1001, "Employee", "user@test.com", "John");
        _appUserServices.RegisterEmployeeAsync(model).Returns((true, "Created", loginData));

        var result = await _sut.RegisterEmployeeUserAsync(model);
        var payload = GetPayload(result);

        payload.GetProperty("success").GetBoolean().Should().BeTrue();
        payload.GetProperty("path").GetString().Should().Be("/main/index");
        await _authService.Received(1).SignInAsync(
            Arg.Any<HttpContext>(),
            Arg.Any<string>(),
            Arg.Any<ClaimsPrincipal>(),
            Arg.Any<AuthenticationProperties>());
    }

    private static JsonElement GetPayload(JsonResult result)
    {
        var json = JsonSerializer.Serialize(result.Value);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
