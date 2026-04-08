using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;

namespace QrPortal.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.SuperAdmin)]
public class QrScansController(IQrScanRepository scans) : Controller
{
    public async Task<IActionResult> Index(int? clientId) => View(await scans.SearchAsync(clientId));
}
