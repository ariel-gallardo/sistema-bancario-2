using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AuthService.Repositories;
using BankingApp.SharedKernel.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;

public sealed class TokenService
{
    private readonly JwtSettings _settings;
    private readonly byte[] _key;
    private readonly IUserRepository _userRepository;

    public TokenService(IOptions<JwtSettings> settings, IUserRepository userRepository)
    {
        _settings = settings.Value;
        _key = Encoding.UTF8.GetBytes(_settings.SecretKey);
        _userRepository = userRepository;
    }

    public async Task<string[]?> TryValidateUserAsync(LoginRequestDto request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var username = request.Username.Trim();
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var hashed = HashPassword(request.Password);
        if (user.PasswordHash is null || user.PasswordHash.Length == 0)
        {
            return null;
        }

        if (!CryptographicOperations.FixedTimeEquals(hashed, user.PasswordHash))
        {
            return null;
        }

        return user.Roles;
    }

    public LoginResponseDto CreateToken(string username, IReadOnlyCollection<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(_key), SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var handler = new JwtSecurityTokenHandler();
        var serialized = handler.WriteToken(token);

        return new LoginResponseDto
        {
            Token = serialized,
            ExpiresAt = expires,
            Roles = roles.ToArray()
        };
    }
    private static byte[] HashPassword(string password)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(password));
    }
}
