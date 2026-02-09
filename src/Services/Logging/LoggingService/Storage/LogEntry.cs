using System;

namespace LoggingService.Storage;

public sealed class LogEntry
{
    public Guid Id { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? RequestPath { get; set; }
    public string? StackTrace { get; set; }
    public string? CorrelationId { get; set; }
    public string? Payload { get; set; }
}
