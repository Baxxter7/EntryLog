using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers
{
    [Authorize(Roles = "Employee")]
    public class MainController(IWorkSessionServices workSessionServices) : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("menu/ultimas_locaciones")]
        public async Task<JsonResult> LastEmployeeLocationAsync()
        {
            UserViewModel? user = User.GetUserData()!;

            IEnumerable<GetLocationDto> locations = await workSessionServices.GetLastLocationByEmployeeAsync(user.NameIdentifier);

            return Json(new
            {
                locations = locations.ToArray()
            });
        }
    }
}
