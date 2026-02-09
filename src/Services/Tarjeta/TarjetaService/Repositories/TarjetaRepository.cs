using System.Data;
using System.Linq;
using BankingApp.SharedKernel.Data;
using TarjetaService.Domain.Entities;
using Dapper;

namespace TarjetaService.Repositories;

public interface ITarjetaRepository
{
    Task<Tarjeta?> GetPrincipalByClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Tarjeta>> GetByCuentaAsync(int cuentaId, CancellationToken cancellationToken);
}

public sealed class TarjetaRepository : ITarjetaRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public TarjetaRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Tarjeta?> GetPrincipalByClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT TOP 1 Id, CuentaId, Numero, EsPrincipal
        FROM Tarjetas
        WHERE ClienteId = @ClienteId AND EsPrincipal = 1
        ORDER BY Id DESC;
        """;

        return await connection.QueryFirstOrDefaultAsync<Tarjeta>(new CommandDefinition(
            sql,
            new { ClienteId = clienteId },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<Tarjeta>> GetByCuentaAsync(int cuentaId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT Id, CuentaId, Numero, EsPrincipal
        FROM Tarjetas
        WHERE CuentaId = @CuentaId
        ORDER BY EsPrincipal DESC, Id DESC;
        """;

        var tarjetas = await connection.QueryAsync<Tarjeta>(new CommandDefinition(
            sql,
            new { CuentaId = cuentaId },
            cancellationToken: cancellationToken));

        return tarjetas.ToList();
    }
}
