using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QrPortal.Application.Interfaces;
using QrPortal.Domain.Enums;
using QrPortal.Web.ViewModels;

namespace QrPortal.Web.Controllers;

public class AuthController(IAuthService authService) : Controller
{
    [HttpGet("auth/login")]
    public IActionResult Login() => View(new LoginVm());

    [HttpPost("auth/login")]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        var user = await authService.ValidateCredentialsAsync(vm.Username, vm.Password);
        if (user is null)
        {
            vm.Error = "Credenziali non valide";
            return View(vm);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, user.RoleId switch
            {
                1 => SystemRoles.SuperAdmin,
                3 => SystemRoles.Customer,
                _ => SystemRoles.Admin
            }),
            new("ClientId", user.ClientId?.ToString() ?? string.Empty)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return user.RoleId == 3 ? RedirectToAction("Index", "Dashboard", new { area = "Customer" }) : RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }

    [HttpPost("auth/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("auth/denied")]
    public IActionResult Denied() => Content("Accesso negato");
}
