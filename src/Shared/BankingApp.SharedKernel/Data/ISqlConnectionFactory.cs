using System.Data;

namespace BankingApp.SharedKernel.Data;

public interface ISqlConnectionFactory
{
    IDbConnection CreateConnection();
}
