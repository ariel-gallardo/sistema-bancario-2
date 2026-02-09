using System;

namespace ClienteCuentaService.Domain.Models;

public sealed class MovimientoResumen
{
	public int Id { get; set; }

	public int TarjetaId { get; set; }

	public DateTime Fecha { get; set; }

	public decimal Monto { get; set; }

	public string Descripcion { get; set; } = string.Empty;
}
