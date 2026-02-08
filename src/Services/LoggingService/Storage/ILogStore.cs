using System.Collections.Generic;

namespace LoggingService.Storage;

public interface ILogStore
{
    void Add(LogRecord entry);
    LogPage GetPage(int page, int pageSize);
}

public sealed record LogPage(int Page, int PageSize, int TotalCount, IReadOnlyList<LogRecord> Items);
