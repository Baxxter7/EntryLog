using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EntryLog.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAppUserServices _appUserServices;

        public AccountController(IAppUserServices appUserServices)
        {
            _appUserServices = appUserServices;
        }
         
        [HttpGet]
        [AllowAnonymous]
        public IActionResult RegisterEmployeeUser()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> RegisterEmployeeUserAsync(CreateEmployeeUserDto model)
        {
            (bool success, string message, LoginResponseDto data) = await _appUserServices.RegisterEmployeeAsync(model)  ;
            //Loguear al empleado

            if (!success)
            {
                return Json(new
                {
                    success,
                    message
                });
            }

            await HttpContext.SignInCookiesAsync(data!);

            return Json(new
            {
                success,
                path = "/main/index"
            });
        }
    }
}
