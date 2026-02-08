using System.Data;
using System.Linq;
using BankingApp.SharedKernel.Data;
using BankingApp.SharedKernel.Dtos;
using Dapper;

namespace TarjetaService.Repositories;

public interface ITarjetaRepository
{
    Task<TarjetaDto?> GetPrincipalByClienteAsync(int clienteId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TarjetaDto>> GetByCuentaAsync(int cuentaId, CancellationToken cancellationToken);
}

public sealed class TarjetaRepository : ITarjetaRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public TarjetaRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<TarjetaDto?> GetPrincipalByClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT TOP 1 t.Id, t.CuentaId, t.Numero, t.EsPrincipal
        FROM Tarjetas t
        INNER JOIN Cuentas c ON c.Id = t.CuentaId
        WHERE c.ClienteId = @ClienteId AND t.EsPrincipal = 1
        ORDER BY t.Id DESC;
        """;

        return await connection.QueryFirstOrDefaultAsync<TarjetaDto>(new CommandDefinition(
            sql,
            new { ClienteId = clienteId },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<TarjetaDto>> GetByCuentaAsync(int cuentaId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT Id, CuentaId, Numero, EsPrincipal
        FROM Tarjetas
        WHERE CuentaId = @CuentaId
        ORDER BY EsPrincipal DESC, Id DESC;
        """;

        var tarjetas = await connection.QueryAsync<TarjetaDto>(new CommandDefinition(
            sql,
            new { CuentaId = cuentaId },
            cancellationToken: cancellationToken));

        return tarjetas.ToList();
    }
}
