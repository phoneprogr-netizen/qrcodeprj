namespace QrPortal.Application.DTOs;

public record QrCodeCreateRequest(
    int ClientId,
    int QrTypeId,
    int? CategoryId,
    string Title,
    string? Description,
    bool IsDynamic,
    string? DestinationUrl,
    string? PayloadJson,
    DateTime? ExpirationDate,
    string? Notes,
    int UserId);

public record PlanEligibilityResult(bool Allowed, string Message);
