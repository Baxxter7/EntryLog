using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
