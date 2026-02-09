using System;
using System.Collections.Generic;

namespace ClienteCuentaService.Domain.Models;

public sealed class ClienteResumen
{
    public decimal SaldoCuentaPrincipal { get; init; }
    public bool TieneTarjetaPrincipal { get; init; }
    public IReadOnlyCollection<MovimientoResumen> Movimientos { get; init; } = Array.Empty<MovimientoResumen>();
}
