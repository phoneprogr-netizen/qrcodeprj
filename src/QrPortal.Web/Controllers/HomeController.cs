using Microsoft.AspNetCore.Mvc;

namespace QrPortal.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    public IActionResult Register() => View();

    public IActionResult Demo() => View();
}
