using System.Collections.Generic;
using System.Linq;
using TarjetaService.Application.Mapping;
using TarjetaService.Domain.Entities;
using Xunit;

namespace TarjetaService.Tests.Mapping;

public sealed class TarjetaMappingTests
{
    [Fact]
    public void ToDtoList_MapsTarjetas()
    {
        var tarjetas = new List<Tarjeta>
        {
            new() { Id = 10, CuentaId = 1, Numero = "1234", EsPrincipal = true },
            new() { Id = 11, CuentaId = 1, Numero = "5678", EsPrincipal = false }
        };

        var dtos = tarjetas.ToDtoList();

        Assert.Equal(tarjetas.Count, dtos.Count);
        Assert.Equal(tarjetas.First().Numero, dtos.First().Numero);
    }
}
