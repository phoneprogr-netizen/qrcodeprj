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
public class ClientsController(IClientRepository clients, IUserRepository users, IRoleRepository roles) : Controller
{
    public async Task<IActionResult> Index(string? query, string? status, bool? isActive)
    {
        var vm = new ClientIndexVm
        {
            Query = query,
            Status = status,
            IsActive = isActive,
            Items = await clients.SearchAsync(query, status, isActive)
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create() => View(new ClientFormVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientFormVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var clientId = await clients.CreateAsync(vm.ToEntity());

        if (vm.CreateDefaultCustomerUser && await users.CountByClientIdAsync(clientId) == 0)
        {
            var customerRole = await roles.GetByNameAsync(SystemRoles.Customer);
            if (customerRole is not null)
            {
                var username = vm.Email.Trim().ToLowerInvariant();
                var password = $"{Guid.NewGuid():N}"[..12];
                var hash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));

                await users.CreateAsync(new()
                {
                    ClientId = clientId,
                    Username = username,
                    FullName = vm.ContactName.Trim(),
                    Email = vm.Email.Trim(),
                    Phone = vm.Phone?.Trim(),
                    RoleId = customerRole.Id,
                    IsActive = true,
                    PasswordHash = hash
                });

                TempData["GeneratedUsername"] = username;
                TempData["GeneratedPassword"] = password;
            }
        }

        TempData["SuccessMessage"] = "Cliente creato con successo.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await clients.GetByIdAsync(id);
        if (client is null) return NotFound();
        return View(ClientFormVm.FromEntity(client));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientFormVm vm)
    {
        if (id != vm.Id) return BadRequest();
        if (!ModelState.IsValid) return View(vm);

        var current = await clients.GetByIdAsync(id);
        if (current is null) return NotFound();

        var updated = vm.ToEntity(current);
        updated.Id = id;
        await clients.UpdateAsync(updated);
        TempData["SuccessMessage"] = "Cliente aggiornato.";
        return RedirectToAction(nameof(Index));
    }
}
