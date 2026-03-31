using QrPortal.Application.DTOs;
using QrPortal.Domain.Entities;

namespace QrPortal.Application.Interfaces;

public interface IAuthService
{
    Task<User?> ValidateCredentialsAsync(string username, string password);
}

public interface IQrCodeService
{
    Task<PlanEligibilityResult> CanCreateQrAsync(int clientId);
    Task<PlanEligibilityResult> CanUseDynamicQrAsync(int clientId);
    Task<PlanEligibilityResult> CanUseTrackingAsync(int clientId);
    Task<PlanEligibilityResult> CanUseMultiLinkAsync(int clientId);
    Task<int> CreateAsync(QrCodeCreateRequest request);
    Task<QrCode?> GetByIdAsync(int id, int? clientId, bool isAdmin);
    Task<QrCode?> GetByShortCodeAsync(string shortCode);
    Task MarkDeletedAsync(int id, int clientId, int userId);
}

public interface IQrCodeContentGenerator
{
    string GenerateContent(string qrTypeCode, string payloadJson, bool isDynamic, string? shortCode, string? destinationUrl, string baseRedirectUrl);
}

public interface IQrPayloadValidator
{
    (bool IsValid, string Error) Validate(string qrTypeCode, string payloadJson);
}

public interface IRedirectResolver
{
    string ResolveFinalUrl(QrCode qrCode, string userAgent);
}

public interface IQrImageGenerator
{
    byte[] GeneratePng(string content, int pixelsPerModule = 20);
    string GenerateSvg(string content);
}
