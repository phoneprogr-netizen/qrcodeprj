using Dapper;
using QrPortal.DataAccess.Infrastructure;
using QrPortal.DataAccess.Interfaces;
using QrPortal.Domain.Entities;

namespace QrPortal.DataAccess.Repositories;

public class UserRepository(ISqlConnectionFactory factory) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username)
    {
        using var cn = factory.Create();
        return await cn.QueryFirstOrDefaultAsync<User>("SELECT TOP 1 * FROM Users WHERE Username=@username AND IsDeleted=0", new { username });
    }
}

public class RoleRepository(ISqlConnectionFactory factory) : IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync() { using var cn = factory.Create(); return await cn.QueryAsync<Role>("SELECT * FROM Roles WHERE IsActive=1"); }
}

public class ClientRepository(ISqlConnectionFactory factory) : IClientRepository
{
    public async Task<Client?> GetByIdAsync(int id) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<Client>("SELECT * FROM Clients WHERE Id=@id AND IsDeleted=0", new { id }); }
}

public class SubscriptionPlanRepository(ISqlConnectionFactory factory) : ISubscriptionPlanRepository
{
    public async Task<SubscriptionPlan?> GetByIdAsync(int id) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<SubscriptionPlan>("SELECT * FROM SubscriptionPlans WHERE Id=@id", new { id }); }
    public async Task<IEnumerable<SubscriptionPlan>> GetActiveAsync() { using var cn = factory.Create(); return await cn.QueryAsync<SubscriptionPlan>("SELECT * FROM SubscriptionPlans WHERE IsActive=1"); }
}

public class ClientSubscriptionRepository(ISqlConnectionFactory factory) : IClientSubscriptionRepository
{
    public async Task<ClientSubscription?> GetActiveByClientIdAsync(int clientId)
    {
        using var cn = factory.Create();
        return await cn.QueryFirstOrDefaultAsync<ClientSubscription>(@"SELECT TOP 1 * FROM ClientSubscriptions
WHERE ClientId=@clientId AND Status='Active' AND (EndDate IS NULL OR EndDate >= CAST(GETUTCDATE() AS date))
ORDER BY StartDate DESC", new { clientId });
    }
}

public class QrTypeRepository(ISqlConnectionFactory factory) : IQrTypeRepository
{
    public async Task<QrType?> GetByIdAsync(int id) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<QrType>("SELECT * FROM QrTypes WHERE Id=@id", new { id }); }
    public async Task<QrType?> GetByCodeAsync(string code) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<QrType>("SELECT * FROM QrTypes WHERE Code=@code", new { code }); }
    public async Task<IEnumerable<QrType>> GetActiveAsync() { using var cn = factory.Create(); return await cn.QueryAsync<QrType>("SELECT * FROM QrTypes WHERE IsActive=1 ORDER BY SortOrder"); }
}

public class QrCategoryRepository(ISqlConnectionFactory factory) : IQrCategoryRepository
{
    public async Task<IEnumerable<QrCategory>> GetActiveAsync() { using var cn = factory.Create(); return await cn.QueryAsync<QrCategory>("SELECT * FROM QrCategories WHERE IsActive=1 ORDER BY Name"); }
}

public class QrCodeRepository(ISqlConnectionFactory factory) : IQrCodeRepository
{
    public async Task<int> CountActiveByClientAsync(int clientId) { using var cn = factory.Create(); return await cn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM QrCodes WHERE ClientId=@clientId AND IsDeleted=0", new { clientId }); }
    public async Task<int> CreateAsync(QrCode q)
    {
        using var cn = factory.Create();
        const string sql = @"INSERT INTO QrCodes (ClientId,QrTypeId,CategoryId,Title,Description,IsDynamic,ShortCode,DestinationUrl,PayloadJson,GeneratedContent,Status,ExpirationDate,Notes,ScanCount,CreatedAt,UpdatedAt,CreatedBy,UpdatedBy,IsDeleted)
VALUES (@ClientId,@QrTypeId,@CategoryId,@Title,@Description,@IsDynamic,@ShortCode,@DestinationUrl,@PayloadJson,@GeneratedContent,@Status,@ExpirationDate,@Notes,0,GETUTCDATE(),GETUTCDATE(),@CreatedBy,@UpdatedBy,0);
SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await cn.ExecuteScalarAsync<int>(sql, q);
    }
    public async Task<QrCode?> GetByIdAsync(int id) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<QrCode>("SELECT * FROM QrCodes WHERE Id=@id AND IsDeleted=0", new { id }); }
    public async Task<QrCode?> GetByShortCodeAsync(string shortCode) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<QrCode>("SELECT * FROM QrCodes WHERE ShortCode=@shortCode AND IsDeleted=0", new { shortCode }); }
    public async Task UpdateScanAsync(int id, DateTime when) { using var cn = factory.Create(); await cn.ExecuteAsync("UPDATE QrCodes SET ScanCount=ScanCount+1,LastScanAt=@when,UpdatedAt=@when WHERE Id=@id", new { id, when }); }
    public async Task SoftDeleteAsync(int id, int clientId, int userId) { using var cn = factory.Create(); await cn.ExecuteAsync("UPDATE QrCodes SET IsDeleted=1,UpdatedAt=GETUTCDATE(),UpdatedBy=@userId WHERE Id=@id AND ClientId=@clientId", new { id, clientId, userId }); }
}

public class QrScanRepository(ISqlConnectionFactory factory) : IQrScanRepository
{
    public async Task CreateAsync(QrScan scan)
    {
        using var cn = factory.Create();
        await cn.ExecuteAsync(@"INSERT INTO QrScans (QrCodeId,ClientId,ScanDate,IpAddress,UserAgent,Referrer,DeviceInfo,Country,City,CreatedAt)
VALUES (@QrCodeId,@ClientId,@ScanDate,@IpAddress,@UserAgent,@Referrer,@DeviceInfo,@Country,@City,GETUTCDATE())", scan);
    }
}

public class AuditLogRepository(ISqlConnectionFactory factory) : IAuditLogRepository
{
    public async Task CreateAsync(AuditLog log)
    {
        using var cn = factory.Create();
        await cn.ExecuteAsync(@"INSERT INTO AuditLogs (UserId,ClientId,ActionType,EntityName,EntityId,Details,CreatedAt)
VALUES (@UserId,@ClientId,@ActionType,@EntityName,@EntityId,@Details,GETUTCDATE())", log);
    }
}
