using QrPortal.Domain.Common;

namespace QrPortal.Domain.Entities;

public class Role : BaseEntity { public string Name { get; set; } = string.Empty; public string? Description { get; set; } public bool IsActive { get; set; } = true; }
public class User : AuditedEntity
{
    public int? ClientId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public bool IsDeleted { get; set; }
}
public class Client : AuditedEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public string? VatNumber { get; set; }
    public string? TaxCode { get; set; }
    public string ContactName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Status { get; set; } = "Active";
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
}
public class SubscriptionPlan : AuditedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int MaxQrCodes { get; set; }
    public bool AllowDynamicQr { get; set; }
    public bool AllowTracking { get; set; }
    public int? MaxMonthlyScans { get; set; }
    public int? MaxUsers { get; set; }
    public bool AllowExport { get; set; }
    public bool AllowMultiLink { get; set; }
    public bool AllowCustomBranding { get; set; }
    public decimal? Price { get; set; }
    public int? DurationMonths { get; set; }
    public bool IsActive { get; set; } = true;
}
public class ClientSubscription : AuditedEntity
{
    public int ClientId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active";
    public string? Notes { get; set; }
}
public class QrCategory : BaseEntity { public string Name { get; set; } = string.Empty; public string? Description { get; set; } public bool IsActive { get; set; } = true; }
public class QrType : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = "GENERAL";
    public bool IsDynamicSupported { get; set; }
    public bool RequiresDestinationUrl { get; set; }
    public bool RequiresPayload { get; set; }
    public string? PayloadSchemaJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
public class QrCode : AuditedEntity
{
    public int ClientId { get; set; }
    public int QrTypeId { get; set; }
    public int? CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDynamic { get; set; }
    public string? ShortCode { get; set; }
    public string? DestinationUrl { get; set; }
    public string? PayloadJson { get; set; }
    public string GeneratedContent { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime? ExpirationDate { get; set; }
    public string? Notes { get; set; }
    public int ScanCount { get; set; }
    public DateTime? LastScanAt { get; set; }
    public int? CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
public class QrScan : BaseEntity
{
    public int QrCodeId { get; set; }
    public int ClientId { get; set; }
    public DateTime ScanDate { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string? Referrer { get; set; }
    public string? DeviceInfo { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
}
public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public int? ClientId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}
