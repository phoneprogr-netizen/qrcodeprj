using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;
using QrPortal.Web.ViewModels;
using System.Security.Cryptography;
using System.Text;

namespace QrPortal.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.SuperAdmin)]
public class UsersController(IUserRepository users, IRoleRepository roles, IClientRepository clients) : Controller
{
    public async Task<IActionResult> Index(string? query, int? roleId, bool? isActive)
    {
        ViewData["Clients"] = await clients.SearchAsync(null, null, true);
        var vm = new UserIndexVm
        {
            Query = query,
            RoleId = roleId,
            IsActive = isActive,
            Roles = await roles.GetAllAsync(),
            Items = await users.SearchAsync(query, roleId, isActive)
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormVm vm)
    {
        var allRoles = (await roles.GetAllAsync()).ToList();
        var allClients = (await clients.SearchAsync(null, null, true)).ToList();

        if (allRoles.All(r => r.Id != vm.RoleId))
        {
            ModelState.AddModelError(nameof(vm.RoleId), "Ruolo non valido.");
        }

        if (vm.RequiresClientAssociation && (!vm.ClientId.HasValue || allClients.All(c => c.Id != vm.ClientId.Value)))
        {
            ModelState.AddModelError(nameof(vm.ClientId), "Per il ruolo Customer è obbligatorio selezionare un cliente.");
        }

        if (!ModelState.IsValid)
        {
            var fallbackVm = new UserIndexVm
            {
                Roles = allRoles,
                Items = await users.SearchAsync(null, null, null)
            };
            ViewData["CreateVm"] = vm;
            ViewData["Clients"] = allClients;
            return View("Index", fallbackVm);
        }

        var password = GeneratePassword();
        var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        await users.CreateAsync(vm.ToEntity(hash));

        TempData["SuccessMessage"] = "Utente creato con successo.";
        TempData["GeneratedUsername"] = vm.Username.Trim();
        TempData["GeneratedPassword"] = password;
        return RedirectToAction(nameof(Index));
    }

    private static string GeneratePassword() => $"{Guid.NewGuid():N}"[..12];
}
