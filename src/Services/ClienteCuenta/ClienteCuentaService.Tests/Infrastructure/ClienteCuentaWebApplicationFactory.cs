using System;
using System.Collections.Generic;
using ClienteCuentaService.Domain.Models;
using ClienteCuentaService.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ClienteCuentaService.Tests.Infrastructure;

public sealed class ClienteCuentaWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryClienteCuentaRepository Repository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IClienteCuentaRepository>();
            services.AddSingleton<IClienteCuentaRepository>(_ => Repository);
        });
    }
}

internal sealed class InMemoryClienteCuentaRepository : IClienteCuentaRepository
{
    private readonly Dictionary<int, ClienteResumen> _store = new();

    public InMemoryClienteCuentaRepository()
    {
        SeedDefault();
    }

    public Task<ClienteResumen?> GetResumenClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        _store.TryGetValue(clienteId, out var resumen);
        return Task.FromResult(resumen);
    }

    public void SetCliente(int clienteId, ClienteResumen resumen)
    {
        _store[clienteId] = resumen;
    }

    public void RemoveCliente(int clienteId) => _store.Remove(clienteId);

    private void SeedDefault()
    {
        SetCliente(1, new ClienteResumen
        {
            SaldoCuentaPrincipal = 157500.25m,
            TieneTarjetaPrincipal = true,
            Movimientos = new List<MovimientoResumen>
            {
                new()
                {
                    Id = 1,
                    TarjetaId = 10,
                    Fecha = DateTime.UtcNow.AddDays(-1),
                    Monto = -9500,
                    Descripcion = "Compra supermercado"
                },
                new()
                {
                    Id = 2,
                    TarjetaId = 10,
                    Fecha = DateTime.UtcNow.AddDays(-2),
                    Monto = 12000,
                    Descripcion = "Transferencia recibida"
                }
            }
        });
    }
}
