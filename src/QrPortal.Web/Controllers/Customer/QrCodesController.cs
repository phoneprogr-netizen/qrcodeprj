using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.Application.DTOs;
using QrPortal.Application.Interfaces;
using QrPortal.Domain.Enums;

namespace QrPortal.Web.Controllers.Customer;

[Area("Customer")]
[Authorize(Roles = SystemRoles.Customer)]
public class QrCodesController(IQrCodeService qrCodeService, IQrImageGenerator imageGenerator) : Controller
{
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Create(QrCodeCreateRequest request)
    {
        var clientId = int.Parse(User.FindFirstValue("ClientId")!);
        if (request.ClientId != clientId) return Forbid();
        var id = await qrCodeService.CreateAsync(request);
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var clientId = int.Parse(User.FindFirstValue("ClientId")!);
        var qr = await qrCodeService.GetByIdAsync(id, clientId, false);
        return qr is null ? NotFound() : View(qr);
    }

    public async Task<IActionResult> DownloadPng(int id)
    {
        var clientId = int.Parse(User.FindFirstValue("ClientId")!);
        var qr = await qrCodeService.GetByIdAsync(id, clientId, false);
        if (qr is null) return NotFound();
        return File(imageGenerator.GeneratePng(qr.GeneratedContent), "image/png", $"qr-{id}.png");
    }

    public async Task<IActionResult> DownloadSvg(int id)
    {
        var clientId = int.Parse(User.FindFirstValue("ClientId")!);
        var qr = await qrCodeService.GetByIdAsync(id, clientId, false);
        if (qr is null) return NotFound();
        return File(System.Text.Encoding.UTF8.GetBytes(imageGenerator.GenerateSvg(qr.GeneratedContent)), "image/svg+xml", $"qr-{id}.svg");
    }
}
