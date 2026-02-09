using System.Security.Cryptography;
using System.Text;
using AuthService.Domain.Entities;
using AuthService.Repositories;
using AuthService.Services;
using BankingApp.SharedKernel.Auth;
using Microsoft.Extensions.Options;
using Xunit;

namespace AuthService.Tests;

public sealed class TokenServiceTests
{
    [Fact]
    public async Task TryValidateUserAsync_ReturnsRoles_WhenPasswordMatches()
    {
        var usuario = CreateSeededUser(); // Mirrors database/auth/schema.sql seed data for "maria".
        var repository = new FakeUserRepository(usuario);
        var sut = CreateTokenService(repository);

        var request = new LoginRequestDto
        {
            Username = "maria",
            Password = "P@ssw0rd"
        };

        var result = await sut.TryValidateUserAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(new[] { "cliente", "vip" }, result);
    }

    [Fact]
    public async Task TryValidateUserAsync_ReturnsNull_WhenPasswordDoesNotMatch()
    {
        var usuario = CreateSeededUser();
        var repository = new FakeUserRepository(usuario);
        var sut = CreateTokenService(repository);

        var request = new LoginRequestDto
        {
            Username = "maria",
            Password = "WrongPassword"
        };

        var result = await sut.TryValidateUserAsync(request, CancellationToken.None);

        Assert.Null(result);
    }

    private static Usuario CreateSeededUser() => new()
    {
        Id = 1,
        Username = "maria",
        PasswordHash = HashPassword("P@ssw0rd"),
        Roles = "cliente,vip",
        ClienteId = 1,
        IsActive = true
    };

    private static TokenService CreateTokenService(IUserRepository repository)
    {
        var options = Options.Create(new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "F7D2C5A0E1B4D7F9C2A5E8B1D4F7A0C3E6B9D2F5A8C1E4B7D0F3A6C9E2B5D8",
            ExpirationMinutes = 60
        });

        return new TokenService(options, repository);
    }

    private static byte[] HashPassword(string plaintext) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(plaintext));

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly Usuario? _usuario;

        public FakeUserRepository(Usuario? usuario)
        {
            _usuario = usuario;
        }

        public Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            if (_usuario is not null && string.Equals(_usuario.Username, username, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult<Usuario?>(_usuario);
            }

            return Task.FromResult<Usuario?>(null);
        }
    }
}
