namespace BankingApp.SharedKernel.Logging;

public sealed class ErrorLogEntry
{
    public required string ServiceName { get; set; }
    public string? RequestPath { get; set; }
    public required string Message { get; set; }
    public string? StackTrace { get; set; }
    public string Severity { get; set; } = "Error";
    public string? CorrelationId { get; set; }
    public string? Payload { get; set; }
}
