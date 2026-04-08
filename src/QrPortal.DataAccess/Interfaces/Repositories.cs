using QrPortal.Domain.Entities;

namespace QrPortal.DataAccess.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<UserListItem>> SearchAsync(string? query, int? roleId, bool? isActive);
    Task<int> CreateAsync(User user);
    Task<int> CountByClientIdAsync(int clientId);
}
public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllAsync();
    Task<Role?> GetByNameAsync(string name);
}
public interface IClientRepository
{
    Task<Client?> GetByIdAsync(int id);
    Task<Client?> GetByUserIdAsync(int userId);
    Task<IEnumerable<ClientListItem>> SearchAsync(string? query, string? status, bool? isActive);
    Task<int> CreateAsync(Client client);
    Task UpdateAsync(Client client);
}
public interface ISubscriptionPlanRepository
{
    Task<SubscriptionPlan?> GetByIdAsync(int id);
    Task<IEnumerable<SubscriptionPlan>> GetActiveAsync();
    Task<IEnumerable<SubscriptionPlan>> SearchAsync(string? query, bool? isActive);
    Task<int> CreateAsync(SubscriptionPlan plan);
    Task UpdateAsync(SubscriptionPlan plan);
}
public interface IClientSubscriptionRepository
{
    Task<ClientSubscription?> GetActiveByClientIdAsync(int clientId);
    Task<IEnumerable<ClientSubscriptionListItem>> SearchAsync(int? clientId);
    Task SetStatusAsync(int id, string status);
}
public interface IQrTypeRepository { Task<QrType?> GetByIdAsync(int id); Task<QrType?> GetByCodeAsync(string code); Task<IEnumerable<QrType>> GetActiveAsync(); }
public interface IQrCategoryRepository { Task<IEnumerable<QrCategory>> GetActiveAsync(); }
public interface IQrCodeRepository
{
    Task<int> CountActiveByClientAsync(int clientId);
    Task<int> CreateAsync(QrCode qrCode);
    Task<QrCode?> GetByIdAsync(int id);
    Task<IEnumerable<QrCodeListItem>> SearchAsync(int? clientId);
    Task<QrCode?> GetByShortCodeAsync(string shortCode);
    Task UpdateScanAsync(int id, DateTime when);
    Task UpdateAsync(QrCode qrCode);
    Task SoftDeleteAsync(int id, int clientId, int userId);
}
public interface IQrScanRepository
{
    Task CreateAsync(QrScan scan);
    Task<IEnumerable<QrScanListItem>> SearchAsync(int? clientId, int take = 200);
}
public interface IAuditLogRepository { Task CreateAsync(AuditLog log); }

public class ClientListItem
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? VatNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? CurrentPlanName { get; set; }
    public int ActiveQrCount { get; set; }
}

public class UserListItem
{
    public int Id { get; set; }
    public int? ClientId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class ClientSubscriptionListItem
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int SubscriptionPlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class QrCodeListItem
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int QrTypeId { get; set; }
    public string QrTypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool IsDynamic { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ScanCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastScanAt { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class QrScanListItem
{
    public long Id { get; set; }
    public int QrCodeId { get; set; }
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string QrTitle { get; set; } = string.Empty;
    public DateTime ScanDate { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? City { get; set; }
}
