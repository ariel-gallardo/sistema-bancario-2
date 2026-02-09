using System.Data;
using System.Linq;
using BankingApp.SharedKernel.Data;
using ClienteCuentaService.Domain.Models;
using Dapper;

namespace ClienteCuentaService.Repositories;

public interface IClienteCuentaRepository
{
    Task<ClienteResumen?> GetResumenClienteAsync(int clienteId, CancellationToken cancellationToken);
}

public sealed class ClienteCuentaRepository : IClienteCuentaRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ClienteCuentaRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ClienteResumen?> GetResumenClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var cuentaPrincipal = await connection.QueryFirstOrDefaultAsync<CuentaPrincipalSnapshot>(new CommandDefinition(
            "dbo.usp_GetSaldoCuentaPrincipal",
            new { ClienteId = clienteId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        if (cuentaPrincipal is null)
        {
            return null;
        }

        var movimientos = (await connection.QueryAsync<MovimientoResumen>(new CommandDefinition(
            "MovimientoDb.dbo.usp_GetUltimosMovimientosTarjetaPrincipal",
            new { ClienteId = clienteId, TopMovimientos = 5 },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken))).ToList();

        return new ClienteResumen
        {
            SaldoCuentaPrincipal = cuentaPrincipal.SaldoCuentaPrincipal,
            TieneTarjetaPrincipal = false,
            Movimientos = movimientos
        };
    }
}
