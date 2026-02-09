using System;
using System.Threading;
using AuthService.Domain.Entities;
using BankingApp.SharedKernel.Data;
using Dapper;

namespace AuthService.Repositories;

public interface IUserRepository
{
    Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}

public sealed class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public UserRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT TOP 1 Id, Username, PasswordHash, Roles, ClienteId
        FROM dbo.Usuarios WITH (NOLOCK)
        WHERE Username = @Username AND IsActive = @IsActive
        ORDER BY Id DESC;
        """;

        return await connection.QueryFirstOrDefaultAsync<Usuario>(new CommandDefinition(
            sql,
            new { Username = username, IsActive = true },
            cancellationToken: cancellationToken));
    }
}
