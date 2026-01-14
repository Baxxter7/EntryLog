using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAppUserServices _appUserServices;

    public AccountController(IAppUserServices appUserServices)
    {
        this._appUserServices = appUserServices;
    }

    [HttpPost("register-employee")]
    public async Task<Object> CreateUserEmployeeAsync([FromBody] CreateEmployeeUserDto employeeUserDto)
    {
        return await _appUserServices.RegisterEmployeeAsync(employeeUserDto);
    }

    [HttpPost("login")]
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

    [HttpPost("recovery-start")]
    public async Task<Object> AccountRecoveryStartAsync([FromBody] string username)
    {
        (bool success, string message) = await _appUserServices.AccountRecoveryStartAsync(username);
        return Ok(new
        {
            success,
            message
        });
    }

    [HttpPost("recovery-complete")]
    public async Task<Object> AccountRecoveryCompleteAsync([FromBody] AccountRecoveryDto recoveryDto)
    {
        (bool success, string message) = await _appUserServices.AccountRecoveryCompleteAsync(recoveryDto);
        return Ok(new
        {
            success,
            message
        });
    }
}
