using System.Collections.Generic;

namespace BankingApp.SharedKernel.Dtos;

public sealed class ResumenClienteDto
{
    public decimal SaldoCuentaPrincipal { get; set; }
    public List<MovimientoResumenDto> Movimientos { get; set; } = new();
    public bool TieneTarjetaPrincipal => Movimientos.Count > 0;
}
