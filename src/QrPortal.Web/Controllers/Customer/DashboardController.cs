using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;

namespace QrPortal.Web.Controllers.Customer;

[Area("Customer")]
[Authorize(Roles = SystemRoles.Customer)]
public class DashboardController(IClientSubscriptionRepository subscriptions, ISubscriptionPlanRepository plans, IQrCodeRepository qrCodes) : Controller
{
    public async Task<IActionResult> Index()
    {
        var clientId = int.Parse(User.FindFirst("ClientId")!.Value);
        ViewData["QrCount"] = await qrCodes.CountActiveByClientAsync(clientId);
        var sub = await subscriptions.GetActiveByClientIdAsync(clientId);
        ViewData["Subscription"] = sub;
        ViewData["Plan"] = sub is null ? null : await plans.GetByIdAsync(sub.SubscriptionPlanId);
        return View();
    }
}
