using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QrPortal.Application.DTOs;
using QrPortal.Application.Interfaces;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Enums;
using QrPortal.Web.ViewModels;

namespace QrPortal.Web.Controllers.Customer;

[Area("Customer")]
[Authorize(Roles = SystemRoles.Customer)]
public class QrCodesController(IQrCodeService qrCodeService, IQrImageGenerator imageGenerator, IQrTypeRepository qrTypes, IQrCategoryRepository qrCategories) : Controller
{
    public async Task<IActionResult> Index()
    {
        var clientId = GetClientId();
        var vm = new CustomerQrCodeIndexVm
        {
            Items = await qrCodeService.SearchAsync(clientId, false),
            Types = await qrTypes.GetActiveAsync(),
            Categories = await qrCategories.GetActiveAsync(),
            Create = new CustomerQrCodeFormVm { ClientId = clientId }
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerQrCodeFormVm vm)
    {
        var clientId = GetClientId();
        if (vm.ClientId != clientId) return Forbid();
        var userId = GetUserId();
        var id = await qrCodeService.CreateAsync(vm.ToCreateRequest(userId));
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(CustomerQrCodeFormVm vm)
    {
        var clientId = GetClientId();
        if (vm.ClientId != clientId || !vm.Id.HasValue) return Forbid();
        var userId = GetUserId();
        await qrCodeService.UpdateAsync(vm.ToUpdateRequest(userId));
        return RedirectToAction(nameof(Details), new { id = vm.Id.Value });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await qrCodeService.MarkDeletedAsync(id, GetClientId(), GetUserId());
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var clientId = GetClientId();
        var qr = await qrCodeService.GetByIdAsync(id, clientId, false);
        return qr is null ? NotFound() : View(qr);
    }

    public async Task<IActionResult> DownloadPng(int id)
    {
        var clientId = GetClientId();
        var qr = await qrCodeService.GetByIdAsync(id, clientId, false);
        if (qr is null) return NotFound();
        return File(imageGenerator.GeneratePng(qr.GeneratedContent), "image/png", $"qr-{id}.png");
    }

    public async Task<IActionResult> DownloadSvg(int id)
    {
        var clientId = GetClientId();
        var qr = await qrCodeService.GetByIdAsync(id, clientId, false);
        if (qr is null) return NotFound();
        return File(System.Text.Encoding.UTF8.GetBytes(imageGenerator.GenerateSvg(qr.GeneratedContent)), "image/svg+xml", $"qr-{id}.svg");
    }

    private int GetClientId() => int.Parse(User.FindFirstValue("ClientId")!);
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
