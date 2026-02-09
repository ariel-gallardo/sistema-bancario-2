using System;

namespace MovimientoService.Application.Dtos;

public sealed record MovimientoDto(int Id, int TarjetaId, DateTime Fecha, decimal Monto, string Descripcion);
