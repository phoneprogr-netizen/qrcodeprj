using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;

namespace QrPortal.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.SuperAdmin)]
public class ClientSubscriptionsController(IClientSubscriptionRepository subscriptions) : Controller
{
    public async Task<IActionResult> Index(int? clientId) => View(await subscriptions.SearchAsync(clientId));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkExpired(int id)
    {
        await subscriptions.SetStatusAsync(id, "Expired");
        return RedirectToAction(nameof(Index));
    }
}
