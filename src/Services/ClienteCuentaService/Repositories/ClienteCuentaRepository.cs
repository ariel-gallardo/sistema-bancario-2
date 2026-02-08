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

        var parameters = new DynamicParameters();
        parameters.Add("@ClienteId", clienteId);

        using var multi = await connection.QueryMultipleAsync(
            "dbo.usp_GetClienteResumen",
            param: parameters,
            commandType: CommandType.StoredProcedure);

        var saldo = await multi.ReadFirstOrDefaultAsync<SaldoResult>();
        var movimientos = (await multi.ReadAsync<MovimientoResumenDto>()).ToList();

        if (saldo is null)
        {
            return null;
        }

        return new ResumenClienteDto
        {
            SaldoCuentaPrincipal = saldo.SaldoCuentaPrincipal,
            Movimientos = movimientos
        };
    }

    private sealed record SaldoResult(decimal SaldoCuentaPrincipal);
}
