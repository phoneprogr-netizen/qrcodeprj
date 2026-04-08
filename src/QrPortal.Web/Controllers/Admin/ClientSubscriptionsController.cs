using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;
using QrPortal.Web.ViewModels;

namespace QrPortal.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.SuperAdmin)]
public class ClientSubscriptionsController(
    IClientSubscriptionRepository subscriptions,
    IClientRepository clients,
    ISubscriptionPlanRepository plans) : Controller
{
    public async Task<IActionResult> Index(int? clientId)
    {
        var vm = new ClientSubscriptionIndexVm
        {
            ClientId = clientId,
            Clients = await clients.SearchAsync(null, "Active", true),
            Plans = await plans.GetActiveAsync(),
            Items = await subscriptions.SearchAsync(clientId)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClientSubscriptionIndexVm vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Clients = await clients.SearchAsync(null, "Active", true);
            vm.Plans = await plans.GetActiveAsync();
            vm.Items = await subscriptions.SearchAsync(vm.ClientId);
            return View(nameof(Index), vm);
        }

        await subscriptions.CreateAsync(vm.Create.ToEntity());
        TempData["SuccessMessage"] = "Abbonamento assegnato con successo.";
        return RedirectToAction(nameof(Index), new { clientId = vm.Create.ClientId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkExpired(int id, int? clientId)
    {
        await subscriptions.SetStatusAsync(id, "Expired");
        return RedirectToAction(nameof(Index), new { clientId });
    }
}
