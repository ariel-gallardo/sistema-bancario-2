namespace TarjetaService.Domain.Entities;

public sealed class Tarjeta
{
    public int Id { get; init; }
    public int CuentaId { get; init; }
    public string Numero { get; init; } = string.Empty;
    public bool EsPrincipal { get; init; }
}
