using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly IAppUserServices _appUserServices;

    public TestController(IAppUserServices appUserServices)
    {
        _appUserServices = appUserServices;
    }

    [HttpPost("user/register-employee")]
    public async Task<Object> CreateUserEmployeeAsync([FromBody] CreateEmployeeUserDto employeeUserDto)
    {
        return await _appUserServices.RegisterEmployeeAsync(employeeUserDto);
    }

    [HttpPost("user/login")]
    public async Task<Object> UserLogin([FromBody] UserCredentialsDto userCredentialsDto)
    {
        (bool success, string message, LoginResponseDto? loginDto) = await _appUserServices.UserLoginAsync(userCredentialsDto);
        return Ok(new
        {
            success,
            message,
            loginDto
        });
    }


    [HttpPost("user/recovery-start")]
    public async Task<Object> AccountRecoveryStartAsync([FromBody] string username)
    {
        (bool success, string message) = await _appUserServices.AccountRecoveryStartAsync(username);
        return Ok(new
        {
            success,
            message
        });
    }
}
