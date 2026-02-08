namespace BankingApp.SharedKernel.Dtos;

public sealed record CuentaDto(int Id, int ClienteId, decimal Saldo, bool EsCuentaPrincipal);
