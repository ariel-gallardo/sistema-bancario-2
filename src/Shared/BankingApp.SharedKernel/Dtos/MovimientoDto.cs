namespace BankingApp.SharedKernel.Dtos;

public sealed record MovimientoDto(int Id, int TarjetaId, DateTime Fecha, decimal Monto, string Descripcion);
