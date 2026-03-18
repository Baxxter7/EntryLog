using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace EntryLog.Web.Controllers;

[Authorize(Roles = "Employee")]
public class FaceIdController : Controller
{
    private readonly IFaceIdService _faceIdService;

    public FaceIdController(IFaceIdService faceIdService)
    {
        _faceIdService = faceIdService;
    }

    public async Task<IActionResult> Index()
    {
        UserViewModel userData = User.GetUserData()!;
        return View(await _faceIdService.GetFaceIdAsync(userData.NameIdentifier));
    }

    [HttpPost("empleado/faceid")]
    public async Task<JsonResult> CreateAsync([FromForm] AddEmployeeFaceIdDto faceIdDto)
    {
        UserViewModel user = User.GetUserData()!;
        (bool success, string message, EmployeeFaceIdDto data) = await _faceIdService.CreateEmployeeFaceIdAsync(
            new AddEmployeeFaceIdDto(user.NameIdentifier, faceIdDto.image, faceIdDto.Descriptor));

        return Json(new
        {
            success,
            message,
            data
        });
    }

    [HttpGet("/empleado/faceid/session")]
    public async Task<IActionResult> GenerateSecurityTokenAsync()
    {
        UserViewModel? user = User.GetUserData();
        if (user is null)
            return Unauthorized();

        var token = await _faceIdService
            .GenerateReferenceImageTokenAsync(user.NameIdentifier.ToString());

        return Ok(new { token });
    }

    [HttpGet("/empleado/faceid/reference")]
    public async Task<IActionResult> GetReferenceImageAsync([FromHeader(Name = "Authorization")] string authHeader)
    {
        UserViewModel? user = User.GetUserData();

        if(user is null)
            return Unauthorized();
        string imageBase64 = await _faceIdService.GenerateReferenceImageTokenAsync(authHeader);

        return Ok(new { imageBase64 });
    }
}
