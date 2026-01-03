using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet]
    public ActionResult Get()
    {
        return Ok("Working");
    }
}
