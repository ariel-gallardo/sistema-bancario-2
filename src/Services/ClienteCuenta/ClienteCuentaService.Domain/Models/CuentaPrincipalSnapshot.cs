namespace ClienteCuentaService.Domain.Models;

public sealed class CuentaPrincipalSnapshot
{
    public int CuentaId { get; init; }
    public int ClienteId { get; init; }
    public decimal SaldoCuentaPrincipal { get; init; }
}
