using EntryLog.Business.DTOs;
using EntryLog.Business.Interfaces;
using EntryLog.Web.Extensions;
using EntryLog.Web.Models;
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

        [HttpGet("registro")]
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
            (bool success, string message, LoginResponseDto? data) = await _appUserServices.RegisterEmployeeAsync(model);
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

        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
                return RedirectToAction("index", "main");


            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoginAsync(UserCredentialsDto model)
        {
            (bool success, string message, LoginResponseDto? data) = await _appUserServices.UserLoginAsync(model);
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

        [HttpGet("cuenta/miperfil")]
        [Authorize (Roles = "Employee")]
        public async Task <IActionResult> MyProfileAsync()
        {
            UserViewModel user = User.GetUserData()!;
            return View(await _appUserServices.GetUserInfoAsync(user.NameIdentifier));
        }

        [HttpGet("/cuenta/salir")]
    
        public async Task<IActionResult> LogOutAsync()
        {
            await HttpContext.SignOutCookiesAsync();
            return RedirectToAction("Login");
        }

    }
}
