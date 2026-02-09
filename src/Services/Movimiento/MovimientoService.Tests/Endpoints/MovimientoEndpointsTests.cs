using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MovimientoService.Application.Dtos;
using MovimientoService.Tests.Infrastructure;
using Xunit;

namespace MovimientoService.Tests.Endpoints;

public sealed class MovimientoEndpointsTests : IClassFixture<MovimientoWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MovimientoEndpointsTests(MovimientoWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetMovimientos_ReturnsLimitedAmount()
    {
        var token = JwtTestTokens.Create();
        var request = new HttpRequestMessage(HttpMethod.Get, "/movimientos/clientes/1?take=3");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var movimientos = await response.Content.ReadFromJsonAsync<List<MovimientoDto>>();

        Assert.NotNull(movimientos);
        Assert.Equal(3, movimientos!.Count);
    }

    [Fact]
    public async Task GetMovimientos_ReturnsEmptyList_WhenClienteSinDatos()
    {
        var token = JwtTestTokens.Create();
        var request = new HttpRequestMessage(HttpMethod.Get, "/movimientos/clientes/99");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var movimientos = await response.Content.ReadFromJsonAsync<List<MovimientoDto>>();

        Assert.NotNull(movimientos);
        Assert.Empty(movimientos!);
    }
}
