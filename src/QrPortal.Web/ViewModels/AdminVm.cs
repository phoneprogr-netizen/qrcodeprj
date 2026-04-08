using System.ComponentModel.DataAnnotations;
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
