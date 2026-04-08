using System.ComponentModel.DataAnnotations;
using QrPortal.Application.DTOs;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Entities;

namespace QrPortal.Web.ViewModels;

public class ClientIndexVm
{
    public string? Query { get; set; }
    public string? Status { get; set; }
    public bool? IsActive { get; set; }
    public IEnumerable<ClientListItem> Items { get; set; } = [];
}

public class ClientFormVm
{
    public int? Id { get; set; }

    [Required, StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? VatNumber { get; set; }

    [StringLength(50)]
    public string? TaxCode { get; set; }

    [Required, StringLength(150)]
    public string ContactName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Phone { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Active";

    public bool IsActive { get; set; } = true;
    public bool CreateDefaultCustomerUser { get; set; } = true;

    public Client ToEntity(Client? current = null)
    {
        var entity = current ?? new Client();
        entity.CompanyName = CompanyName.Trim();
        entity.VatNumber = VatNumber?.Trim();
        entity.TaxCode = TaxCode?.Trim();
        entity.ContactName = ContactName.Trim();
        entity.Email = Email.Trim();
        entity.Phone = Phone?.Trim();
        entity.Status = Status.Trim();
        entity.IsActive = IsActive;
        return entity;
    }

    public static ClientFormVm FromEntity(Client client) => new()
    {
        Id = client.Id,
        CompanyName = client.CompanyName,
        VatNumber = client.VatNumber,
        TaxCode = client.TaxCode,
        ContactName = client.ContactName,
        Email = client.Email,
        Phone = client.Phone,
        Status = client.Status,
        IsActive = client.IsActive
    };
}

public class UserIndexVm
{
    public string? Query { get; set; }
    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
    public IEnumerable<Role> Roles { get; set; } = [];
    public IEnumerable<UserListItem> Items { get; set; } = [];
}

public class UserFormVm
{
    [Required, StringLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Phone { get; set; }

    [Required]
    public int RoleId { get; set; } = 3;

    public int? ClientId { get; set; }
    public bool IsActive { get; set; } = true;

    public bool RequiresClientAssociation => RoleId == 3;

    public User ToEntity(string passwordHash) => new()
    {
        Username = Username.Trim(),
        FullName = FullName.Trim(),
        Email = Email.Trim(),
        Phone = Phone?.Trim(),
        RoleId = RoleId,
        ClientId = RoleId == 3 ? ClientId : null,
        IsActive = IsActive,
        PasswordHash = passwordHash
    };
}

public class PlanIndexVm
{
    public string? Query { get; set; }
    public bool? IsActive { get; set; }
    public IEnumerable<SubscriptionPlan> Items { get; set; } = [];
}

public class PlanFormVm
{
    public int? Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(1, int.MaxValue)]
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

    public SubscriptionPlan ToEntity(SubscriptionPlan? current = null)
    {
        var entity = current ?? new SubscriptionPlan();
        entity.Name = Name.Trim();
        entity.Description = Description?.Trim();
        entity.MaxQrCodes = MaxQrCodes;
        entity.AllowDynamicQr = AllowDynamicQr;
        entity.AllowTracking = AllowTracking;
        entity.MaxMonthlyScans = MaxMonthlyScans;
        entity.MaxUsers = MaxUsers;
        entity.AllowExport = AllowExport;
        entity.AllowMultiLink = AllowMultiLink;
        entity.AllowCustomBranding = AllowCustomBranding;
        entity.Price = Price;
        entity.DurationMonths = DurationMonths;
        entity.IsActive = IsActive;
        return entity;
    }

    public static PlanFormVm FromEntity(SubscriptionPlan plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        Description = plan.Description,
        MaxQrCodes = plan.MaxQrCodes,
        AllowDynamicQr = plan.AllowDynamicQr,
        AllowTracking = plan.AllowTracking,
        MaxMonthlyScans = plan.MaxMonthlyScans,
        MaxUsers = plan.MaxUsers,
        AllowExport = plan.AllowExport,
        AllowMultiLink = plan.AllowMultiLink,
        AllowCustomBranding = plan.AllowCustomBranding,
        Price = plan.Price,
        DurationMonths = plan.DurationMonths,
        IsActive = plan.IsActive
    };
}

public class CustomerQrCodeIndexVm
{
    public CustomerQrCodeFormVm Create { get; set; } = new();
    public IEnumerable<QrCodeListItem> Items { get; set; } = [];
    public IEnumerable<QrType> Types { get; set; } = [];
    public IEnumerable<QrCategory> Categories { get; set; } = [];
}

public class CustomerQrCodeFormVm
{
    public int? Id { get; set; }
    public int ClientId { get; set; }
    [Required] public int QrTypeId { get; set; }
    public int? CategoryId { get; set; }
    [Required, StringLength(200)] public string Title { get; set; } = string.Empty;
    [StringLength(500)] public string? Description { get; set; }
    public bool IsDynamic { get; set; }
    [StringLength(1000)] public string? DestinationUrl { get; set; }
    public string? PayloadJson { get; set; } = "{}";
    [StringLength(20)] public string Status { get; set; } = "Active";
    public DateTime? ExpirationDate { get; set; }
    public string? Notes { get; set; }

    public QrCodeCreateRequest ToCreateRequest(int userId) => new(
        ClientId, QrTypeId, CategoryId, Title, Description, IsDynamic, DestinationUrl, PayloadJson, ExpirationDate, Notes, userId);

    public QrCodeUpdateRequest ToUpdateRequest(int userId) => new(
        Id ?? 0, ClientId, QrTypeId, CategoryId, Title, Description, DestinationUrl, PayloadJson, Status, ExpirationDate, Notes, userId);

    public static CustomerQrCodeFormVm FromEntity(QrCode qr) => new()
    {
        Id = qr.Id,
        ClientId = qr.ClientId,
        QrTypeId = qr.QrTypeId,
        CategoryId = qr.CategoryId,
        Title = qr.Title,
        Description = qr.Description,
        IsDynamic = qr.IsDynamic,
        DestinationUrl = qr.DestinationUrl,
        PayloadJson = qr.PayloadJson,
        Status = qr.Status,
        ExpirationDate = qr.ExpirationDate,
        Notes = qr.Notes
    };
}

public class ClientSubscriptionIndexVm
{
    public int? ClientId { get; set; }
    public ClientSubscriptionCreateVm Create { get; set; } = new();
    public IEnumerable<ClientListItem> Clients { get; set; } = [];
    public IEnumerable<SubscriptionPlan> Plans { get; set; } = [];
    public IEnumerable<ClientSubscriptionListItem> Items { get; set; } = [];
}

public class ClientSubscriptionCreateVm
{
    [Required]
    public int ClientId { get; set; }

    [Required]
    public int SubscriptionPlanId { get; set; }

    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    [Required, StringLength(20)]
    public string Status { get; set; } = "Active";

    [StringLength(1000)]
    public string? Notes { get; set; }

    public ClientSubscription ToEntity() => new()
    {
        ClientId = ClientId,
        SubscriptionPlanId = SubscriptionPlanId,
        StartDate = StartDate,
        EndDate = EndDate,
        Status = Status.Trim(),
        Notes = Notes?.Trim()
    };
}
