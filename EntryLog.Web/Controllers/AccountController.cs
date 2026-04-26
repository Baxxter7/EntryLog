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

        [HttpGet("registro", Name = "Register")]
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
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> MyProfileAsync()
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

        [HttpGet("cuenta/recuperar", Name = "GetRecover")]
        public IActionResult Recover()
        {
            return View();
        }

        [HttpGet("cuenta/recuperar", Name = "PostRecover")]
        public async Task<JsonResult> RecoverAsync([FromQuery] string email)
        {
            (bool success, string message) = await _appUserServices.AccountRecoveryStartAsync(email);

            return Json(new
            {
                success,
                message
            });
        }

        [HttpGet("completar/recuperar", Name = "GetCompleteRecover")]
        public async Task<IActionResult> CompleteRecover([FromQuery] string token)
        {
            (bool success, string message) = await _appUserServices.ValidateRecoveryTokenAsync(token);

            ViewBag.Success = success;
            ViewBag.Message = message;

            return View();
        }

        [HttpPost("completar/recuperar", Name = "PostCompleteRecover")]
        public async Task<JsonResult> CompleteRecoverAsync([FromQuery] AccountRecoveryDto model)
        {
            (bool success, string message) = await _appUserServices.AccountRecoveryCompleteAsync(model);

            return Json(new
            {
                success,
                message
            });
        }
    }
}
