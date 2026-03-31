using QrPortal.Domain.Entities;

namespace QrPortal.DataAccess.Interfaces;

public interface IUserRepository { Task<User?> GetByUsernameAsync(string username); }
public interface IRoleRepository { Task<IEnumerable<Role>> GetAllAsync(); }
public interface IClientRepository { Task<Client?> GetByIdAsync(int id); }
public interface ISubscriptionPlanRepository { Task<SubscriptionPlan?> GetByIdAsync(int id); Task<IEnumerable<SubscriptionPlan>> GetActiveAsync(); }
public interface IClientSubscriptionRepository { Task<ClientSubscription?> GetActiveByClientIdAsync(int clientId); }
public interface IQrTypeRepository { Task<QrType?> GetByIdAsync(int id); Task<QrType?> GetByCodeAsync(string code); Task<IEnumerable<QrType>> GetActiveAsync(); }
public interface IQrCategoryRepository { Task<IEnumerable<QrCategory>> GetActiveAsync(); }
public interface IQrCodeRepository
{
    Task<int> CountActiveByClientAsync(int clientId);
    Task<int> CreateAsync(QrCode qrCode);
    Task<QrCode?> GetByIdAsync(int id);
    Task<QrCode?> GetByShortCodeAsync(string shortCode);
    Task UpdateScanAsync(int id, DateTime when);
    Task SoftDeleteAsync(int id, int clientId, int userId);
}
public interface IQrScanRepository { Task CreateAsync(QrScan scan); }
public interface IAuditLogRepository { Task CreateAsync(AuditLog log); }
