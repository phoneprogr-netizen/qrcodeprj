using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using QrPortal.Application.DTOs;
using QrPortal.Application.Interfaces;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Entities;
using QRCoder;

namespace QrPortal.Application.Services;

public class AuthService(IUserRepository users) : IAuthService
{
    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        var user = await users.GetByUsernameAsync(username);
        if (user is null || !user.IsActive) return null;
        return Verify(password, user.PasswordHash) ? user : null;
    }

    private static bool Verify(string password, string hash)
    {
        var computed = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(password)));
        return computed == hash;
    }
}

public class QrCodeContentGenerator : IQrCodeContentGenerator
{
    public string GenerateContent(string qrTypeCode, string payloadJson, bool isDynamic, string? shortCode, string? destinationUrl, string baseRedirectUrl)
    {
        if (isDynamic) return $"{baseRedirectUrl.TrimEnd('/')}/{shortCode}";
        var payload = JsonDocument.Parse(payloadJson).RootElement;
        return qrTypeCode switch
        {
            "URL" or "WEB_PAGE" or "PAYMENT_LINK" => payload.GetProperty("url").GetString() ?? string.Empty,
            "TEXT" => payload.GetProperty("text").GetString() ?? string.Empty,
            "EMAIL" => $"mailto:{payload.GetProperty("email").GetString()}?subject={Uri.EscapeDataString(payload.GetProperty("subject").GetString() ?? "")}&body={Uri.EscapeDataString(payload.GetProperty("body").GetString() ?? "")}",
            "PHONE" => $"tel:{payload.GetProperty("phone").GetString()}",
            "WHATSAPP" => $"https://wa.me/{payload.GetProperty("phone").GetString()}?text={Uri.EscapeDataString(payload.GetProperty("message").GetString() ?? "")}",
            "SMS" => $"sms:{payload.GetProperty("phone").GetString()}?body={Uri.EscapeDataString(payload.GetProperty("message").GetString() ?? "")}",
            "GEOLOCATION" => $"geo:{payload.GetProperty("latitude").GetDecimal()},{payload.GetProperty("longitude").GetDecimal()}",
            "WIFI" => $"WIFI:T:{payload.GetProperty("security").GetString()};S:{payload.GetProperty("ssid").GetString()};P:{payload.GetProperty("password").GetString()};;",
            _ => destinationUrl ?? payloadJson
        };
    }
}

public class QrPayloadValidator : IQrPayloadValidator
{
    public (bool IsValid, string Error) Validate(string qrTypeCode, string payloadJson)
    {
        try
        {
            var root = JsonDocument.Parse(payloadJson).RootElement;
            return qrTypeCode switch
            {
                "URL" or "WEB_PAGE" or "PAYMENT_LINK" => ValidateUrl(root, "url"),
                "EMAIL" => root.TryGetProperty("email", out var e) && e.GetString()?.Contains('@') == true ? (true, "") : (false, "Email non valida"),
                "PHONE" or "WHATSAPP" or "SMS" => root.TryGetProperty("phone", out var p) && !string.IsNullOrWhiteSpace(p.GetString()) ? (true, "") : (false, "Telefono richiesto"),
                "GEOLOCATION" => root.TryGetProperty("latitude", out _) && root.TryGetProperty("longitude", out _) ? (true, "") : (false, "Coordinate richieste"),
                _ => (true, "")
            };
        }
        catch { return (false, "Payload JSON non valido"); }
    }

    private static (bool, string) ValidateUrl(JsonElement root, string prop)
    {
        if (!root.TryGetProperty(prop, out var u) || !Uri.TryCreate(u.GetString(), UriKind.Absolute, out _)) return (false, "URL non valida");
        return (true, "");
    }
}

public class RedirectResolver : IRedirectResolver
{
    public string ResolveFinalUrl(QrCode qrCode, string userAgent)
    {
        if (!string.IsNullOrWhiteSpace(qrCode.DestinationUrl)) return qrCode.DestinationUrl;
        return qrCode.GeneratedContent;
    }
}

public class QrImageGenerator : IQrImageGenerator
{
    public byte[] GeneratePng(string content, int pixelsPerModule = 20)
    {
        using var data = new QRCodeGenerator().CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(data).GetGraphic(pixelsPerModule);
    }

    public string GenerateSvg(string content)
    {
        using var data = new QRCodeGenerator().CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        return new SvgQRCode(data).GetGraphic(4);
    }
}

public class QrCodeService(
    IQrCodeRepository qrCodes,
    IQrTypeRepository qrTypes,
    IClientRepository clients,
    IClientSubscriptionRepository subscriptions,
    ISubscriptionPlanRepository plans,
    IQrCodeContentGenerator generator,
    IQrPayloadValidator validator) : IQrCodeService
{
    public Task<QrCode?> GetByShortCodeAsync(string shortCode) => qrCodes.GetByShortCodeAsync(shortCode);

    public async Task<PlanEligibilityResult> CanCreateQrAsync(int clientId)
    {
        var check = await ValidateSubscriptionAsync(clientId);
        if (!check.Allowed) return check;
        var sub = await subscriptions.GetActiveByClientIdAsync(clientId);
        var plan = await plans.GetByIdAsync(sub!.SubscriptionPlanId);
        var used = await qrCodes.CountActiveByClientAsync(clientId);
        return used < plan!.MaxQrCodes ? new(true, "OK") : new(false, "Limite piano raggiunto");
    }
    public async Task<PlanEligibilityResult> CanUseDynamicQrAsync(int clientId) => await FeatureCheck(clientId, p => p.AllowDynamicQr, "Piano non consente QR dinamici");
    public async Task<PlanEligibilityResult> CanUseTrackingAsync(int clientId) => await FeatureCheck(clientId, p => p.AllowTracking, "Piano non consente tracking");
    public async Task<PlanEligibilityResult> CanUseMultiLinkAsync(int clientId) => await FeatureCheck(clientId, p => p.AllowMultiLink, "Piano non consente multi link");

    public async Task<int> CreateAsync(QrCodeCreateRequest request)
    {
        var can = await CanCreateQrAsync(request.ClientId);
        if (!can.Allowed) throw new InvalidOperationException(can.Message);

        var type = await qrTypes.GetByIdAsync(request.QrTypeId) ?? throw new InvalidOperationException("Tipo QR non trovato");
        if (request.IsDynamic)
        {
            var d = await CanUseDynamicQrAsync(request.ClientId);
            if (!d.Allowed) throw new InvalidOperationException(d.Message);
        }

        var payload = request.PayloadJson ?? "{}";
        var valid = validator.Validate(type.Code, payload);
        if (!valid.IsValid) throw new InvalidOperationException(valid.Error);

        var shortCode = request.IsDynamic ? Guid.NewGuid().ToString("N")[..8] : null;
        var generated = generator.GenerateContent(type.Code, payload, request.IsDynamic, shortCode, request.DestinationUrl, "https://tuodominio.it/r");
        var qr = new QrCode
        {
            ClientId = request.ClientId,
            QrTypeId = request.QrTypeId,
            CategoryId = request.CategoryId,
            Title = request.Title,
            Description = request.Description,
            IsDynamic = request.IsDynamic,
            ShortCode = shortCode,
            DestinationUrl = request.DestinationUrl,
            PayloadJson = payload,
            GeneratedContent = generated,
            Status = "Active",
            ExpirationDate = request.ExpirationDate,
            Notes = request.Notes,
            CreatedBy = request.UserId,
            UpdatedBy = request.UserId
        };
        return await qrCodes.CreateAsync(qr);
    }

    public async Task UpdateAsync(QrCodeUpdateRequest request)
    {
        var current = await qrCodes.GetByIdAsync(request.Id) ?? throw new InvalidOperationException("QR non trovato");
        if (current.ClientId != request.ClientId) throw new InvalidOperationException("QR non appartenente al cliente");

        var type = await qrTypes.GetByIdAsync(request.QrTypeId) ?? throw new InvalidOperationException("Tipo QR non trovato");
        var payload = request.PayloadJson ?? "{}";
        var valid = validator.Validate(type.Code, payload);
        if (!valid.IsValid) throw new InvalidOperationException(valid.Error);

        current.QrTypeId = request.QrTypeId;
        current.CategoryId = request.CategoryId;
        current.Title = request.Title;
        current.Description = request.Description;
        current.DestinationUrl = request.DestinationUrl;
        current.PayloadJson = payload;
        current.GeneratedContent = generator.GenerateContent(type.Code, payload, current.IsDynamic, current.ShortCode, request.DestinationUrl, "https://tuodominio.it/r");
        current.Status = request.Status;
        current.ExpirationDate = request.ExpirationDate;
        current.Notes = request.Notes;
        current.UpdatedBy = request.UserId;

        await qrCodes.UpdateAsync(current);
    }

    public async Task<QrCode?> GetByIdAsync(int id, int? clientId, bool isAdmin)
    {
        var qr = await qrCodes.GetByIdAsync(id);
        if (qr is null || (!isAdmin && qr.ClientId != clientId)) return null;
        return qr;
    }

    public Task MarkDeletedAsync(int id, int clientId, int userId) => qrCodes.SoftDeleteAsync(id, clientId, userId);

    public Task<IEnumerable<QrCodeListItem>> SearchAsync(int? clientId, bool isAdmin)
    {
        if (!isAdmin && !clientId.HasValue) throw new InvalidOperationException("ClientId richiesto");
        return qrCodes.SearchAsync(clientId);
    }

    private async Task<PlanEligibilityResult> FeatureCheck(int clientId, Func<SubscriptionPlan, bool> predicate, string message)
    {
        var check = await ValidateSubscriptionAsync(clientId);
        if (!check.Allowed) return check;
        var sub = await subscriptions.GetActiveByClientIdAsync(clientId);
        var plan = await plans.GetByIdAsync(sub!.SubscriptionPlanId);
        return predicate(plan!) ? new(true, "OK") : new(false, message);
    }

    private async Task<PlanEligibilityResult> ValidateSubscriptionAsync(int clientId)
    {
        var client = await clients.GetByIdAsync(clientId);
        if (client is null || !client.IsActive) return new(false, "Cliente non attivo");
        var sub = await subscriptions.GetActiveByClientIdAsync(clientId);
        if (sub is null || sub.Status != "Active") return new(false, "Abbonamento non attivo");
        if (sub.EndDate.HasValue && sub.EndDate.Value.Date < DateTime.UtcNow.Date) return new(false, "Abbonamento scaduto");
        return new(true, "OK");
    }
}

public class DashboardService;
public class UserService;
public class ClientService;
public class SubscriptionPlanService;
public class ClientSubscriptionService;
public class QrTypeService;
public class QrCategoryService;
public class QrScanService;
public class AuditLogService;
