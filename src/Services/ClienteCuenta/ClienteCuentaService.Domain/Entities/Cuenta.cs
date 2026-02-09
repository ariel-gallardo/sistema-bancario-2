namespace ClienteCuentaService.Domain.Entities;

public sealed class Cuenta
{
    public int Id { get; init; }
    public int ClienteId { get; init; }
    public decimal Saldo { get; init; }
    public bool EsCuentaPrincipal { get; init; }
    public bool EsActiva { get; init; }
}
