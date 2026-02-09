using System;
using System.Collections.Generic;
using System.Linq;
using ClienteCuentaService.Application.Mapping;
using ClienteCuentaService.Domain.Models;
using Xunit;

namespace ClienteCuentaService.Tests.Mapping;

public sealed class ClienteCuentaMappingTests
{
    [Fact]
    public void ToDto_MapsSaldoMovimientosYTieneTarjeta()
    {
        var source = new ClienteResumen
        {
            SaldoCuentaPrincipal = 5000m,
            TieneTarjetaPrincipal = true,
            Movimientos = new List<MovimientoResumen>
            {
                new()
                {
                    Id = 1,
                    TarjetaId = 5,
                    Fecha = new DateTime(2024, 6, 1),
                    Monto = -2500m,
                    Descripcion = "Compra"
                }
            }
        };

        var dto = source.ToDto();

        Assert.Equal(source.SaldoCuentaPrincipal, dto.SaldoCuentaPrincipal);
        Assert.Equal(source.TieneTarjetaPrincipal, dto.TieneTarjetaPrincipal);
        Assert.Single(dto.Movimientos);
        Assert.Equal(source.Movimientos.First().Descripcion, dto.Movimientos[0].Descripcion);
    }
}
