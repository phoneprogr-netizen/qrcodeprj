using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;
using QrPortal.Web.ViewModels;

namespace QrPortal.Web.Controllers.Customer;

[Area("Customer")]
[Authorize(Roles = SystemRoles.Customer)]
public class ProfileController(IClientRepository clients) : Controller
{
    public async Task<IActionResult> Index()
    {
        var client = await clients.GetByIdAsync(int.Parse(User.FindFirst("ClientId")!.Value));
        if (client is null) return NotFound();
        return View(ClientFormVm.FromEntity(client));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ClientFormVm vm)
    {
        var clientId = int.Parse(User.FindFirst("ClientId")!.Value);
        vm.Id = clientId;
        if (!ModelState.IsValid) return View(vm);

        var current = await clients.GetByIdAsync(clientId);
        if (current is null) return NotFound();

        await clients.UpdateAsync(vm.ToEntity(current));
        TempData["SuccessMessage"] = "Profilo aggiornato.";
        return RedirectToAction(nameof(Index));
    }
}
