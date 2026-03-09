using EntryLog.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers;

public class WorkSessionController : Controller
{
    private IWorkSessionServices _workSessionServices;
    private IFaceIdService _faceId;

    public WorkSessionController(IWorkSessionServices workSessionServices, IFaceIdService faceId)
    {
        _workSessionServices = workSessionServices;
        _faceId = faceId;
    }

    public async Task <IActionResult> Index()
    {
        return View();
    }
}
