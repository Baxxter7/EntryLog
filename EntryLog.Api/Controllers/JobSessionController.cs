using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.QueryFilters;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class JobSessionController : ControllerBase
{
    private readonly IWorkSessionServices _workSessionServices;

    public JobSessionController(IWorkSessionServices workSessionServices)
    {
        _workSessionServices = workSessionServices;
    }

    [HttpPost("open")]
    public async Task<object> OpenJobSessionAsync([FromForm] CreateJobSessionDto jobSessionDto)
    {
        (bool success, string message) = await _workSessionServices.OpenJobSessionAsync(jobSessionDto);
        return Ok(new
        {
            success,
            message
        });
    }

    [HttpPost("close")]
    public async Task<object> CloseJobSessionAsync([FromForm] CloseJobSessionDto jobSessionDto)
    {
        (bool success, string message) = await _workSessionServices.ClosedJobSessionAsync(jobSessionDto);
        return Ok(new
        {
            success,
            message
        });
    }

    [HttpPost("filter")]
    public async Task<IEnumerable<GetWorkSessionDto>> FilterAsync([FromQuery] WorkSessionQueryFilter filter)
        => await _workSessionServices.GetSessionListByFilterAsync(filter);
}
