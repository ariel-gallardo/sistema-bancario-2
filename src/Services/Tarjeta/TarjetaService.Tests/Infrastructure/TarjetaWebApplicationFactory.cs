using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using TarjetaService.Domain.Entities;
using TarjetaService.Repositories;

namespace TarjetaService.Tests.Infrastructure;

internal sealed class TarjetaWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemoryTarjetaRepository Repository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ITarjetaRepository>();
            services.AddSingleton<ITarjetaRepository>(_ => Repository);
        });
    }
}

internal sealed class InMemoryTarjetaRepository : ITarjetaRepository
{
    private readonly List<Tarjeta> _tarjetas;
    private readonly Dictionary<int, int> _principalPorCliente;

    public InMemoryTarjetaRepository()
    {
        _tarjetas = new List<Tarjeta>
        {
            new() { Id = 1, CuentaId = 500, Numero = "4500-0012-3456-7890", EsPrincipal = true },
            new() { Id = 2, CuentaId = 500, Numero = "4500-0099-3245-1111", EsPrincipal = false },
            new() { Id = 3, CuentaId = 700, Numero = "4678-0000-1111-2222", EsPrincipal = true }
        };

        _principalPorCliente = new Dictionary<int, int>
        {
            [1] = 1,
            [2] = 3
        };
    }

    public Task<Tarjeta?> GetPrincipalByClienteAsync(int clienteId, CancellationToken cancellationToken)
    {
        if (_principalPorCliente.TryGetValue(clienteId, out var tarjetaId))
        {
            return Task.FromResult<Tarjeta?>(_tarjetas.First(t => t.Id == tarjetaId));
        }

        return Task.FromResult<Tarjeta?>(null);
    }

    public Task<IReadOnlyCollection<Tarjeta>> GetByCuentaAsync(int cuentaId, CancellationToken cancellationToken)
    {
        var result = _tarjetas.Where(t => t.CuentaId == cuentaId).OrderByDescending(t => t.EsPrincipal).ToList();
        return Task.FromResult<IReadOnlyCollection<Tarjeta>>(result);
    }
}
