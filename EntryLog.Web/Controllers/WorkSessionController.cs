using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers;

public class WorkSessionController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
