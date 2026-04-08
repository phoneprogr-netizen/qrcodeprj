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

    public async Task<IEnumerable<UserListItem>> SearchAsync(string? query, int? roleId, bool? isActive)
    {
        using var cn = factory.Create();
        const string sql = @"SELECT u.Id,
       u.ClientId,
       u.Username,
       u.FullName,
       u.Email,
       u.RoleId,
       r.Name AS RoleName,
       c.CompanyName AS ClientName,
       u.IsActive,
       u.LastLoginAt
FROM Users u
INNER JOIN Roles r ON r.Id=u.RoleId
LEFT JOIN Clients c ON c.Id=u.ClientId
WHERE u.IsDeleted=0
  AND (@query IS NULL OR u.FullName LIKE '%' + @query + '%' OR u.Email LIKE '%' + @query + '%' OR u.Username LIKE '%' + @query + '%')
  AND (@roleId IS NULL OR u.RoleId=@roleId)
  AND (@isActive IS NULL OR u.IsActive=@isActive)
ORDER BY u.FullName, u.Username";
        return await cn.QueryAsync<UserListItem>(sql, new { query, roleId, isActive });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var cn = factory.Create();
        const string sql = @"INSERT INTO Users
(ClientId,Username,PasswordHash,FullName,Email,Phone,RoleId,IsActive,CreatedAt,UpdatedAt,IsDeleted)
VALUES
(@ClientId,@Username,@PasswordHash,@FullName,@Email,@Phone,@RoleId,@IsActive,SYSUTCDATETIME(),SYSUTCDATETIME(),0);
SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await cn.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<int> CountByClientIdAsync(int clientId)
    {
        using var cn = factory.Create();
        return await cn.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM Users WHERE ClientId=@clientId AND IsDeleted=0", new { clientId });
    }
}

public class RoleRepository(ISqlConnectionFactory factory) : IRoleRepository
{
    public async Task<IEnumerable<Role>> GetAllAsync() { using var cn = factory.Create(); return await cn.QueryAsync<Role>("SELECT * FROM Roles WHERE IsActive=1"); }
    public async Task<Role?> GetByNameAsync(string name)
    {
        using var cn = factory.Create();
        return await cn.QueryFirstOrDefaultAsync<Role>("SELECT TOP 1 * FROM Roles WHERE Name=@name AND IsActive=1", new { name });
    }
}

public class ClientRepository(ISqlConnectionFactory factory) : IClientRepository
{
    public async Task<Client?> GetByIdAsync(int id) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<Client>("SELECT * FROM Clients WHERE Id=@id AND IsDeleted=0", new { id }); }
    public async Task<Client?> GetByUserIdAsync(int userId)
    {
        using var cn = factory.Create();
        const string sql = @"SELECT TOP 1 c.*
FROM Users u
INNER JOIN Clients c ON c.Id=u.ClientId
WHERE u.Id=@userId AND u.IsDeleted=0 AND c.IsDeleted=0";
        return await cn.QueryFirstOrDefaultAsync<Client>(sql, new { userId });
    }

    public async Task<IEnumerable<ClientListItem>> SearchAsync(string? query, string? status, bool? isActive)
    {
        using var cn = factory.Create();
        const string sql = @"SELECT c.Id,
       c.CompanyName,
       c.VatNumber,
       c.Status,
       c.IsActive,
       p.Name AS CurrentPlanName,
       (SELECT COUNT(1) FROM QrCodes q WHERE q.ClientId=c.Id AND q.IsDeleted=0) AS ActiveQrCount
FROM Clients c
OUTER APPLY (
    SELECT TOP 1 sp.Name
    FROM ClientSubscriptions cs
    INNER JOIN SubscriptionPlans sp ON sp.Id=cs.SubscriptionPlanId
    WHERE cs.ClientId=c.Id
    ORDER BY cs.StartDate DESC
) p
WHERE c.IsDeleted=0
  AND (@query IS NULL OR c.CompanyName LIKE '%' + @query + '%' OR c.Email LIKE '%' + @query + '%' OR c.VatNumber LIKE '%' + @query + '%')
  AND (@status IS NULL OR c.Status = @status)
  AND (@isActive IS NULL OR c.IsActive = @isActive)
ORDER BY c.CompanyName";
        return await cn.QueryAsync<ClientListItem>(sql, new { query, status, isActive });
    }

    public async Task<int> CreateAsync(Client client)
    {
        using var cn = factory.Create();
        const string sql = @"INSERT INTO Clients (CompanyName,VatNumber,TaxCode,ContactName,Email,Phone,Status,IsActive,CreatedAt,UpdatedAt,IsDeleted)
VALUES (@CompanyName,@VatNumber,@TaxCode,@ContactName,@Email,@Phone,@Status,@IsActive,SYSUTCDATETIME(),SYSUTCDATETIME(),0);
SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await cn.ExecuteScalarAsync<int>(sql, client);
    }

    public async Task UpdateAsync(Client client)
    {
        using var cn = factory.Create();
        const string sql = @"UPDATE Clients
SET CompanyName=@CompanyName,
    VatNumber=@VatNumber,
    TaxCode=@TaxCode,
    ContactName=@ContactName,
    Email=@Email,
    Phone=@Phone,
    Status=@Status,
    IsActive=@IsActive,
    UpdatedAt=SYSUTCDATETIME()
WHERE Id=@Id AND IsDeleted=0";
        await cn.ExecuteAsync(sql, client);
    }
}

public class SubscriptionPlanRepository(ISqlConnectionFactory factory) : ISubscriptionPlanRepository
{
    public async Task<SubscriptionPlan?> GetByIdAsync(int id) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<SubscriptionPlan>("SELECT * FROM SubscriptionPlans WHERE Id=@id", new { id }); }
    public async Task<IEnumerable<SubscriptionPlan>> GetActiveAsync() { using var cn = factory.Create(); return await cn.QueryAsync<SubscriptionPlan>("SELECT * FROM SubscriptionPlans WHERE IsActive=1"); }

    public async Task<IEnumerable<SubscriptionPlan>> SearchAsync(string? query, bool? isActive)
    {
        using var cn = factory.Create();
        const string sql = @"SELECT *
FROM SubscriptionPlans
WHERE (@query IS NULL OR Name LIKE '%' + @query + '%' OR Description LIKE '%' + @query + '%')
  AND (@isActive IS NULL OR IsActive=@isActive)
ORDER BY Price, Name";
        return await cn.QueryAsync<SubscriptionPlan>(sql, new { query, isActive });
    }

    public async Task<int> CreateAsync(SubscriptionPlan plan)
    {
        using var cn = factory.Create();
        const string sql = @"INSERT INTO SubscriptionPlans
(Name,Description,MaxQrCodes,AllowDynamicQr,AllowTracking,MaxMonthlyScans,MaxUsers,AllowExport,AllowMultiLink,AllowCustomBranding,Price,DurationMonths,IsActive,CreatedAt,UpdatedAt)
VALUES
(@Name,@Description,@MaxQrCodes,@AllowDynamicQr,@AllowTracking,@MaxMonthlyScans,@MaxUsers,@AllowExport,@AllowMultiLink,@AllowCustomBranding,@Price,@DurationMonths,@IsActive,SYSUTCDATETIME(),SYSUTCDATETIME());
SELECT CAST(SCOPE_IDENTITY() AS int);";
        return await cn.ExecuteScalarAsync<int>(sql, plan);
    }

    public async Task UpdateAsync(SubscriptionPlan plan)
    {
        using var cn = factory.Create();
        const string sql = @"UPDATE SubscriptionPlans
SET Name=@Name,
    Description=@Description,
    MaxQrCodes=@MaxQrCodes,
    AllowDynamicQr=@AllowDynamicQr,
    AllowTracking=@AllowTracking,
    MaxMonthlyScans=@MaxMonthlyScans,
    MaxUsers=@MaxUsers,
    AllowExport=@AllowExport,
    AllowMultiLink=@AllowMultiLink,
    AllowCustomBranding=@AllowCustomBranding,
    Price=@Price,
    DurationMonths=@DurationMonths,
    IsActive=@IsActive,
    UpdatedAt=SYSUTCDATETIME()
WHERE Id=@Id";
        await cn.ExecuteAsync(sql, plan);
    }
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

    public async Task<IEnumerable<ClientSubscriptionListItem>> SearchAsync(int? clientId)
    {
        using var cn = factory.Create();
        const string sql = @"SELECT cs.Id,
       cs.ClientId,
       c.CompanyName AS ClientName,
       cs.SubscriptionPlanId,
       sp.Name AS PlanName,
       cs.StartDate,
       cs.EndDate,
       cs.Status
FROM ClientSubscriptions cs
INNER JOIN Clients c ON c.Id=cs.ClientId
INNER JOIN SubscriptionPlans sp ON sp.Id=cs.SubscriptionPlanId
WHERE (@clientId IS NULL OR cs.ClientId=@clientId)
ORDER BY cs.StartDate DESC";
        return await cn.QueryAsync<ClientSubscriptionListItem>(sql, new { clientId });
    }

    public async Task SetStatusAsync(int id, string status)
    {
        using var cn = factory.Create();
        await cn.ExecuteAsync("UPDATE ClientSubscriptions SET Status=@status,UpdatedAt=SYSUTCDATETIME() WHERE Id=@id", new { id, status });
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
    public async Task<IEnumerable<QrCodeListItem>> SearchAsync(int? clientId)
    {
        using var cn = factory.Create();
        const string sql = @"SELECT q.Id,
       q.ClientId,
       c.CompanyName AS ClientName,
       q.QrTypeId,
       t.Name AS QrTypeName,
       q.Title,
       q.IsDynamic,
       q.Status,
       q.ScanCount,
       q.CreatedAt,
       q.LastScanAt,
       q.ExpirationDate
FROM QrCodes q
INNER JOIN Clients c ON c.Id=q.ClientId
INNER JOIN QrTypes t ON t.Id=q.QrTypeId
WHERE q.IsDeleted=0
  AND (@clientId IS NULL OR q.ClientId=@clientId)
ORDER BY q.CreatedAt DESC";
        return await cn.QueryAsync<QrCodeListItem>(sql, new { clientId });
    }
    public async Task<QrCode?> GetByShortCodeAsync(string shortCode) { using var cn = factory.Create(); return await cn.QueryFirstOrDefaultAsync<QrCode>("SELECT * FROM QrCodes WHERE ShortCode=@shortCode AND IsDeleted=0", new { shortCode }); }
    public async Task UpdateScanAsync(int id, DateTime when) { using var cn = factory.Create(); await cn.ExecuteAsync("UPDATE QrCodes SET ScanCount=ScanCount+1,LastScanAt=@when,UpdatedAt=@when WHERE Id=@id", new { id, when }); }
    public async Task UpdateAsync(QrCode q)
    {
        using var cn = factory.Create();
        const string sql = @"UPDATE QrCodes
SET QrTypeId=@QrTypeId,
    CategoryId=@CategoryId,
    Title=@Title,
    Description=@Description,
    DestinationUrl=@DestinationUrl,
    PayloadJson=@PayloadJson,
    GeneratedContent=@GeneratedContent,
    Status=@Status,
    ExpirationDate=@ExpirationDate,
    Notes=@Notes,
    UpdatedAt=GETUTCDATE(),
    UpdatedBy=@UpdatedBy
WHERE Id=@Id AND ClientId=@ClientId AND IsDeleted=0";
        await cn.ExecuteAsync(sql, q);
    }
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

    public async Task<IEnumerable<QrScanListItem>> SearchAsync(int? clientId, int take = 200)
    {
        using var cn = factory.Create();
        const string sql = @"SELECT TOP (@take)
       s.Id,
       s.QrCodeId,
       s.ClientId,
       c.CompanyName AS ClientName,
       q.Title AS QrTitle,
       s.ScanDate,
       s.IpAddress,
       s.Country,
       s.City
FROM QrScans s
INNER JOIN Clients c ON c.Id=s.ClientId
INNER JOIN QrCodes q ON q.Id=s.QrCodeId
WHERE (@clientId IS NULL OR s.ClientId=@clientId)
ORDER BY s.ScanDate DESC";
        return await cn.QueryAsync<QrScanListItem>(sql, new { clientId, take });
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
