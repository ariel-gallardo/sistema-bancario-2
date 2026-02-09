using System;
using System.Collections.Generic;

namespace AuthService.Application.Dtos;

public sealed class UserRolesDto
{
    public string Username { get; init; } = string.Empty;
    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();
    public int? ClienteId { get; init; }
}
