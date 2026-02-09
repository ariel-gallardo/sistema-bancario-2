using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClienteCuentaService.Application.Dtos;
using ClienteCuentaService.Tests.Infrastructure;
using Xunit;

namespace ClienteCuentaService.Tests.Endpoints;

public sealed class ClienteResumenEndpointTests : IClassFixture<ClienteCuentaWebApplicationFactory>
{
    private readonly ClienteCuentaWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ClienteResumenEndpointTests(ClienteCuentaWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetResumen_ReturnsClienteResumen_WhenClienteExiste()
    {
        var token = JwtTestTokens.Create();
        var request = new HttpRequestMessage(HttpMethod.Get, "/clientes/1/resumen");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var resumen = await response.Content.ReadFromJsonAsync<ResumenClienteDto>();

        Assert.NotNull(resumen);
        Assert.Equal(157500.25m, resumen!.SaldoCuentaPrincipal);
        Assert.True(resumen.TieneTarjetaPrincipal);
        Assert.Equal(2, resumen.Movimientos.Count);
    }

    [Fact]
    public async Task GetResumen_Returns404_WhenClienteNoExiste()
    {
        var token = JwtTestTokens.Create();
        var request = new HttpRequestMessage(HttpMethod.Get, "/clientes/99/resumen");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
