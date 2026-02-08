using BankingApp.SharedKernel.Data;
using BankingApp.SharedKernel.Dtos;
using Dapper;

namespace MovimientoService.Repositories;

public interface IMovimientoRepository
{
    Task<IReadOnlyCollection<MovimientoDto>> GetByTarjetaAsync(int tarjetaId, int take, CancellationToken cancellationToken);
}

public sealed class MovimientoRepository : IMovimientoRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public MovimientoRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyCollection<MovimientoDto>> GetByTarjetaAsync(int tarjetaId, int take, CancellationToken cancellationToken)
    {
        using var connection = _connectionFactory.CreateConnection();

        const string sql = """
        SELECT TOP (@Take) Id, TarjetaId, Fecha, Monto, Descripcion
        FROM Movimientos
        WHERE TarjetaId = @TarjetaId
        ORDER BY Fecha DESC;
        """;

        var movimientos = await connection.QueryAsync<MovimientoDto>(new CommandDefinition(
            sql,
            new { TarjetaId = tarjetaId, Take = take },
            cancellationToken: cancellationToken));

        return movimientos.ToList();
    }
}
