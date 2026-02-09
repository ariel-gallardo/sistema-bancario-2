using System.Collections.Generic;

namespace LoggingService.Storage;

public interface ILogStore
{
    void Add(LogRecord entry);
    LogPage GetPage(
        int page,
        int pageSize,
        string? serviceName = null,
        string? severity = null,
        string? search = null,
        string? correlationId = null,
        string? requestPath = null);
}

public sealed record LogPage(int Page, int PageSize, int TotalCount, IReadOnlyList<LogRecord> Items);
