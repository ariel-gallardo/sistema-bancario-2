using System.Data;
using System.Linq;
using BankingApp.SharedKernel.Data;
using Dapper;
using MovimientoService.Domain.Entities;

namespace MovimientoService.Repositories;

public interface IMovimientoRepository
{
    Task<IReadOnlyCollection<Movimiento>> GetByClienteAsync(int clienteId, int take, CancellationToken cancellationToken);
}

public sealed class MovimientoRepository : IMovimientoRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public MovimientoRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<Movimiento>> GetByClienteAsync(int clienteId, int take, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var movimientos = await connection.QueryAsync<Movimiento>(new CommandDefinition(
            "dbo.usp_GetUltimosMovimientosTarjetaPrincipal",
            new { ClienteId = clienteId, TopMovimientos = take },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return movimientos.ToList();
    }
}
