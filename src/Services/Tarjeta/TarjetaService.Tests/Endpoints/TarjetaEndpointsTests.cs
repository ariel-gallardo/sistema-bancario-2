using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TarjetaService.Application.Dtos;
using TarjetaService.Tests.Infrastructure;
using Xunit;

namespace TarjetaService.Tests.Endpoints;

public sealed class TarjetaEndpointsTests : IClassFixture<TarjetaWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TarjetaEndpointsTests(TarjetaWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTarjetaPrincipal_ReturnsTarjetaCuandoExiste()
    {
        var token = JwtTestTokens.Create();
        var request = new HttpRequestMessage(HttpMethod.Get, "/tarjetas/clientes/1/principal");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var dto = await response.Content.ReadFromJsonAsync<TarjetaDto>();

        Assert.NotNull(dto);
        Assert.True(dto!.EsPrincipal);
        Assert.Equal(500, dto.CuentaId);
    }

    [Fact]
    public async Task GetTarjetaPrincipal_Returns404CuandoNoExiste()
    {
        var token = JwtTestTokens.Create();
        var request = new HttpRequestMessage(HttpMethod.Get, "/tarjetas/clientes/99/principal");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetTarjetasPorCuenta_ReturnsListadoOrdenado()
    {
        var token = JwtTestTokens.Create();
        var request = new HttpRequestMessage(HttpMethod.Get, "/tarjetas/cuentas/500");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var tarjetas = await response.Content.ReadFromJsonAsync<List<TarjetaDto>>();

        Assert.NotNull(tarjetas);
        Assert.Equal(2, tarjetas!.Count);
        Assert.True(tarjetas.First().EsPrincipal);
    }
}
