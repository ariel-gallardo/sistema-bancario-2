using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthService.Tests.Infrastructure;
using Xunit;

namespace AuthService.Tests;

public sealed class AuthEndpointsTests : IClassFixture<AuthWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthEndpointsTests(AuthWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task IssueToken_ReturnsToken_ForValidCredentials()
    {
        var payload = new { username = "maria", password = "P@ssw0rd" };

        var response = await _client.PostAsJsonAsync("/auth/token", payload);

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Contains(json.GetProperty("roles").EnumerateArray(), role => role.GetString() == "cliente");
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("token").GetString()));
    }

    [Fact]
    public async Task IssueToken_Returns401_ForInvalidCredentials()
    {
        var payload = new { username = "maria", password = "nope" };

        var response = await _client.PostAsJsonAsync("/auth/token", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_Returns401_WhenMissingToken()
    {
        var response = await _client.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ReturnsProfile_WhenTokenProvided()
    {
        var loginResponse = await _client.PostAsJsonAsync("/auth/token", new { username = "admin", password = "Admin2024" });
        loginResponse.EnsureSuccessStatusCode();
        var json = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("token").GetString();

        var request = new HttpRequestMessage(HttpMethod.Get, "/auth/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();
        var me = await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal("admin", me.GetProperty("name").GetString());
        Assert.Contains(me.GetProperty("roles").EnumerateArray(), role => role.GetString() == "admin");
    }
}
