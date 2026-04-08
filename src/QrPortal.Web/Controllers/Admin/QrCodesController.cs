using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.Application.Interfaces;
using QrPortal.Domain.Enums;

namespace QrPortal.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.SuperAdmin)]
public class QrCodesController(IQrCodeService qrCodeService) : Controller
{
    public async Task<IActionResult> Index() => View(await qrCodeService.SearchAsync(null, true));
}
