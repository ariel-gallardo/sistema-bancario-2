namespace BankingApp.SharedKernel.Dtos;

public sealed record TarjetaDto(int Id, int CuentaId, string Numero, bool EsPrincipal);
