namespace BankingApp.SharedKernel.Auth;

public sealed class LoginResponseDto
{
    public required string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
}
