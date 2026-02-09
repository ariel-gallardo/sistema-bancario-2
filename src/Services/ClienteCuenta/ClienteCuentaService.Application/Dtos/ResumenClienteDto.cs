using System.Collections.Generic;

namespace ClienteCuentaService.Application.Dtos;

public sealed class ResumenClienteDto
{
    public decimal SaldoCuentaPrincipal { get; set; }
    public List<MovimientoResumenDto> Movimientos { get; set; } = new();
    public bool TieneTarjetaPrincipal { get; set; }
}
