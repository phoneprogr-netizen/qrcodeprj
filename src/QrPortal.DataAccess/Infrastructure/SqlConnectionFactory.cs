using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace QrPortal.DataAccess.Infrastructure;

public interface ISqlConnectionFactory { IDbConnection Create(); }

public class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection Create() => new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
}
