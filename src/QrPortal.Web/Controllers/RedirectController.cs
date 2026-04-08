using Microsoft.AspNetCore.Mvc;
using QrPortal.Application.Interfaces;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Entities;

namespace QrPortal.Web.Controllers;

public class RedirectController(
    IQrCodeService qrCodeService,
    IQrScanRepository scanRepository,
    IQrCodeRepository qrCodeRepository,
    IRedirectResolver resolver) : Controller
{
    [HttpGet("/r/{shortCode}")]
    public async Task<IActionResult> Go(string shortCode)
    {
        var qr = await qrCodeService.GetByShortCodeAsync(shortCode);
        if (qr is null || qr.Status != "Active" || qr.IsDeleted || (qr.ExpirationDate.HasValue && qr.ExpirationDate.Value < DateTime.UtcNow))
            return View("~/Views/Redirect/NotAvailable.cshtml");

        await scanRepository.CreateAsync(new QrScan
        {
            QrCodeId = qr.Id,
            ClientId = qr.ClientId,
            ScanDate = DateTime.UtcNow,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "n/a",
            UserAgent = Request.Headers.UserAgent.ToString(),
            Referrer = Request.Headers.Referer.ToString()
        });

        await qrCodeRepository.UpdateScanAsync(qr.Id, DateTime.UtcNow);
        var target = resolver.ResolveFinalUrl(qr, Request.Headers.UserAgent.ToString());

        if (await IsMultiLinkAsync(qr)) return View("~/Views/Redirect/MultiLink.cshtml", qr);
        return Redirect(target);
    }

    private async Task<bool> IsMultiLinkAsync(QrCode qr)
    {
        await Task.CompletedTask;
        return qr.PayloadJson?.Contains("\"links\"") == true;
    }
}
