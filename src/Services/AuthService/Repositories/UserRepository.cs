using System;
using System.Threading;
using BankingApp.SharedKernel.Data;
using Dapper;

namespace AuthService.Repositories;

public interface IUserRepository
{
    Task<AuthUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken);
}

public sealed class UserRepository : IUserRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public UserRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<AuthUser?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT TOP 1 Id, Username, PasswordHash, Roles, ClienteId
        FROM dbo.Usuarios
        WHERE Username = @Username AND IsActive = 1
        ORDER BY Id DESC;
        """;

        var user = await connection.QueryFirstOrDefaultAsync<UserRow>(new CommandDefinition(
            sql,
            new { Username = username },
            cancellationToken: cancellationToken));

        if (user is null)
        {
            return null;
        }

        var roles = string.IsNullOrWhiteSpace(user.Roles)
            ? Array.Empty<string>()
            : user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return new AuthUser(user.Id, user.Username, user.PasswordHash, roles, user.ClienteId);
    }

    private sealed record UserRow(int Id, string Username, byte[] PasswordHash, string Roles, int? ClienteId);
}

public sealed record AuthUser(int Id, string Username, byte[] PasswordHash, string[] Roles, int? ClienteId);
