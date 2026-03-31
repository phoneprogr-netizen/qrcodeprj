using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.Domain.Enums;

namespace QrPortal.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.SuperAdmin)]
public class QrCategoriesController : Controller
{
    public IActionResult Index() => View();
}
