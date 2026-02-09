using System.Security.Cryptography;
using System.Text;
using AuthService.Domain.Entities;
using AuthService.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace AuthService.Tests.Infrastructure;

internal sealed class AuthWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging => logging.ClearProviders());

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserRepository>();
            services.AddSingleton<IUserRepository>(_ => new InMemoryUserRepository());
        });
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly IReadOnlyDictionary<string, Usuario> _users;

        public InMemoryUserRepository()
        {
            _users = new Dictionary<string, Usuario>(StringComparer.OrdinalIgnoreCase)
            {
                ["maria"] = new Usuario
                {
                    Id = 1,
                    Username = "maria",
                    PasswordHash = HashPassword("P@ssw0rd"),
                    Roles = "cliente,vip",
                    ClienteId = 1,
                    IsActive = true
                },
                ["admin"] = new Usuario
                {
                    Id = 2,
                    Username = "admin",
                    PasswordHash = HashPassword("Admin2024"),
                    Roles = "admin",
                    ClienteId = null,
                    IsActive = true
                }
            };
        }

        public Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        {
            _users.TryGetValue(username, out var usuario);
            return Task.FromResult(usuario);
        }

        private static byte[] HashPassword(string value) => SHA256.HashData(Encoding.UTF8.GetBytes(value));
    }
}
