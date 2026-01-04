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

    [HttpPost("register-employee")]
    public async Task<Object> CreateUserEmployeeAsync([FromBody] CreateEmployeeUserDto employeeUserDto)
    {
        return await _appUserServices.RegisterEmployeeAsync(employeeUserDto);
    }
}
