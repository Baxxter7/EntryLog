using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers;

[Authorize(Roles = "Employee")]
public class WorkSessionController : Controller
{
    private readonly IWorkSessionServices _workSessionService;
    private readonly IFaceIdService _faceIdService;

    public WorkSessionController(IWorkSessionServices workSessionServices, IFaceIdService faceId)
    {
        _workSessionService = workSessionServices;
        _faceIdService = faceId;
    }

    public async Task<IActionResult> Index()
    {
        UserViewModel userData = User.GetUserData()!;
        EmployeeFaceIdDto faceId = await _faceIdService.GetFaceIdAsync(userData.NameIdentifier);
        ViewBag.IsFaceIdActive = faceId.Active;

        bool hasActiveSession = await _workSessionService.HasActiveAnySessionAsync(userData.NameIdentifier);
        ViewBag.HasActiveSession = hasActiveSession;

        PaginatedResult<GetWorkSessionDto> model = await _workSessionService.GetSessionListByFilterAsync(new WorkSessionQueryFilter
        {
            EmployeeId = userData.NameIdentifier,
            Sort = Business.Enums.SortType.Descending
        });

        return View(model);
    }

    [HttpPost("/emleado/sessiones/abrir")]
    public async Task<JsonResult> OpenWorkSessionAsync(OpenWorkSessionViewModel model)
    {
        UserViewModel userData = User.GetUserData()!;
        (bool success, string message, GetWorkSessionDto data) = await _workSessionService.OpenJobSessionAsync(new CreateWorkSessionDto(
            userData.NameIdentifier.ToString(),
            model.Latitude,
            model.Longitude,
            model.Image,
            model.Notes,
            model.Descriptor
            )
        );

        return Json(new
        {
            success, 
            message, 
            data 
        });
    }

    [HttpPost("empleado/sessiones/cerrar")]
    public async Task<JsonResult> CloseWorkSessionAsync(CloseWorkSessionViewModel model)
    {
        UserViewModel userData = User.GetUserData()!;
        (bool success, string message, GetWorkSessionDto? data) = await _workSessionService.ClosedJobSessionAsync(
             new CloseWorkSessionDto(
                 model.sessionId,
                 userData.NameIdentifier.ToString(),
                 model.Latitude,
                 model.Longitude,
                 model.Image,
                 model.Notes,
                 model.Descriptor
             )
        );

        return Json(new
        {
            success,
            message,
            data
        });
    }
}
