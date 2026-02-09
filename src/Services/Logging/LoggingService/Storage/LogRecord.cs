using System;

namespace LoggingService.Storage;

public sealed record LogRecord(
    Guid Id,
    DateTime TimestampUtc,
    string ServiceName,
    string Message,
    string Severity,
    string RequestPath,
    string StackTrace,
    string CorrelationId,
    string Payload);
