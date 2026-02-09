using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MovimientoService.Tests.Infrastructure;

internal static class JwtTestTokens
{
    private const string Issuer = "BankingAuth";
    private const string Audience = "BankingApi";
    private const string Secret = "F7D2C5A0E1B4D7F9C2A5E8B1D4F7A0C3E6B9D2F5A8C1E4B7D0F3A6C9E2B5D8";

    public static string Create(string username = "tester", params string[] roles)
    {
        if (roles.Length == 0)
        {
            roles = new[] { "cliente" };
        }

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(Issuer, Audience, claims, expires: DateTime.UtcNow.AddMinutes(20), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
