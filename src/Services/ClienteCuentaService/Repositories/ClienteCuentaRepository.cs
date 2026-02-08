using System.Data;
using System.Linq;
using BankingApp.SharedKernel.Data;
using BankingApp.SharedKernel.Dtos;
using Dapper;

namespace ClienteCuentaService.Repositories;

public interface IClienteCuentaRepository
{
    Task<ResumenClienteDto?> GetResumenClienteAsync(int clienteId, CancellationToken cancellationToken);
}

public sealed class ClienteCuentaRepository : IClienteCuentaRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ClienteCuentaRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ResumenClienteDto?> GetResumenClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var saldo = await connection.QueryFirstOrDefaultAsync<SaldoResult>(new CommandDefinition(
            "dbo.usp_GetSaldoCuentaPrincipal",
            new { ClienteId = clienteId },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        if (saldo is null)
        {
            return null;
        }

        var movimientos = (await connection.QueryAsync<MovimientoResumenDto>(new CommandDefinition(
            "MovimientoDb.dbo.usp_GetUltimosMovimientosTarjetaPrincipal",
            new { ClienteId = clienteId, TopMovimientos = 5 },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken))).ToList();

        return new ResumenClienteDto
        {
            SaldoCuentaPrincipal = saldo.SaldoCuentaPrincipal,
            Movimientos = movimientos,
            TieneTarjetaPrincipal = saldo.TieneTarjetaPrincipal
        };
    }

    private sealed record SaldoResult(decimal SaldoCuentaPrincipal, bool TieneTarjetaPrincipal);
}
