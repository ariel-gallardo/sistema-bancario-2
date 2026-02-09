using System;

namespace AuthService.Domain.Entities;

public sealed class Usuario
{
    public int Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public byte[] PasswordHash { get; init; } = Array.Empty<byte>();
    public string Roles { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int? ClienteId { get; init; }
}
