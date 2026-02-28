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
}
