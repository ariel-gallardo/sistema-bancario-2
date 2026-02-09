using System;

namespace MovimientoService.Domain.Entities;

public sealed class Movimiento
{
    public int Id { get; init; }
    public int TarjetaId { get; init; }
    public DateTime Fecha { get; init; }
    public decimal Monto { get; init; }
    public string Descripcion { get; init; } = string.Empty;
}
