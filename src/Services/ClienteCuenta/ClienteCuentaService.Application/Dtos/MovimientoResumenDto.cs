using System;

namespace ClienteCuentaService.Application.Dtos;

public sealed record MovimientoResumenDto(DateTime Fecha, decimal Monto, string Descripcion);
