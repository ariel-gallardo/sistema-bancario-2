namespace BankingApp.SharedKernel.Dtos;

public sealed record MovimientoResumenDto(DateTime Fecha, decimal Monto, string Descripcion);
