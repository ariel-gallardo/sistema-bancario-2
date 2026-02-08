using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankingApp.SharedKernel.Data;

public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;
    private readonly ILogger<SqlConnectionFactory> _logger;

    public SqlConnectionFactory(IConfiguration configuration, ILogger<SqlConnectionFactory> logger)
    {
        _connectionString = configuration.GetConnectionString("SqlServer")
            ?? throw new InvalidOperationException("Missing 'SqlServer' connection string.");
        _logger = logger;
    }

    public IDbConnection CreateConnection()
    {
        _logger.LogDebug("Opening SQL connection using configured SqlServer connection string");
        return new SqlConnection(_connectionString);
    }
}
