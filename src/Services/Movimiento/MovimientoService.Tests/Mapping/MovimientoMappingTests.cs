using System;
using System.Collections.Generic;
using System.Linq;
using MovimientoService.Application.Mapping;
using MovimientoService.Domain.Entities;
using Xunit;

namespace MovimientoService.Tests.Mapping;

public sealed class MovimientoMappingTests
{
    [Fact]
    public void ToDtoList_MapsEachMovimiento()
    {
        var movimientos = new List<Movimiento>
        {
            new() { Id = 1, TarjetaId = 10, Fecha = new DateTime(2024, 5, 1), Monto = -500, Descripcion = "Compra" },
            new() { Id = 2, TarjetaId = 11, Fecha = new DateTime(2024, 5, 2), Monto = 1000, Descripcion = "Transferencia" }
        };

        var dtoList = movimientos.ToDtoList().ToList();

        Assert.Equal(movimientos.Count, dtoList.Count);
        Assert.Equal(movimientos[0].Descripcion, dtoList[0].Descripcion);
    }
}
