using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;

namespace QrPortal.Web.Controllers.Admin;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin + "," + SystemRoles.SuperAdmin)]
public class QrCategoriesController(IQrCategoryRepository qrCategories) : Controller
{
    public async Task<IActionResult> Index() => View(await qrCategories.GetActiveAsync());
}
