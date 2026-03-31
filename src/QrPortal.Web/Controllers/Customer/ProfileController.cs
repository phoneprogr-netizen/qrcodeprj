using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.Domain.Enums;

namespace QrPortal.Web.Controllers.Customer;

[Area("Customer")]
[Authorize(Roles = SystemRoles.Customer)]
public class ProfileController : Controller
{
    public IActionResult Index() => View();
}
