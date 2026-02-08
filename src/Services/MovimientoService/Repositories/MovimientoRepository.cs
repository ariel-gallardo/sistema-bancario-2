using BankingApp.SharedKernel.Data;
using BankingApp.SharedKernel.Dtos;
using Dapper;

namespace MovimientoService.Repositories;

public interface IMovimientoRepository
{
    Task<IReadOnlyCollection<MovimientoDto>> GetByClienteAsync(int clienteId, int take, CancellationToken cancellationToken);
}

public sealed class MovimientoRepository : IMovimientoRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public MovimientoRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<MovimientoDto>> GetByClienteAsync(int clienteId, int take, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        var movimientos = await connection.QueryAsync<MovimientoDto>(new CommandDefinition(
            "dbo.usp_GetUltimosMovimientosTarjetaPrincipal",
            new { ClienteId = clienteId, TopMovimientos = take },
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken));

        return movimientos.ToList();
    }
}
