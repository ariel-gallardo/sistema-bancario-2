using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using MovimientoService.Domain.Entities;
using MovimientoService.Repositories;

namespace MovimientoService.Tests.Infrastructure;

internal sealed class MovimientoWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryMovimientoRepository Repository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IMovimientoRepository>();
            services.AddSingleton<IMovimientoRepository>(_ => Repository);
        });
    }
}

internal sealed class InMemoryMovimientoRepository : IMovimientoRepository
{
    private readonly Dictionary<int, List<Movimiento>> _store = new();

    public InMemoryMovimientoRepository()
    {
        _store[1] = Enumerable.Range(1, 10).Select(i => new Movimiento
        {
            Id = i,
            TarjetaId = 10,
            Fecha = DateTime.UtcNow.AddDays(-i),
            Monto = i % 2 == 0 ? 1500 : -900,
            Descripcion = $"Movimiento {i}"
        }).ToList();
    }

    public Task<IReadOnlyCollection<Movimiento>> GetByClienteAsync(int clienteId, int take, CancellationToken cancellationToken)
    {
        if (!_store.TryGetValue(clienteId, out var movimientos))
        {
            return Task.FromResult<IReadOnlyCollection<Movimiento>>(Array.Empty<Movimiento>());
        }

        return Task.FromResult<IReadOnlyCollection<Movimiento>>(movimientos.Take(take).ToList());
    }
}
