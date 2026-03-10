using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Business.Pagination;
using EntryLog.Business.QueryFilters;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers;

[Authorize(Roles ="Employee")]
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

        PaginatedResult<GetWorkSessionDto> model = await _workSessionService.GetSessionListByFilterAsync(new WorkSessionQueryFilter
        {
            EmployeeId = userData.NameIdentifier,
            Sort = Business.Enums.SortType.Descending
        });

        return View(model);
    }
}
