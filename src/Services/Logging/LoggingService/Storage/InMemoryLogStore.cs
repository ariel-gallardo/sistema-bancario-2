using System;
using System.Collections.Concurrent;

namespace LoggingService.Storage;

public sealed class InMemoryLogStore : ILogStore
{
    private readonly ConcurrentQueue<LogRecord> _entries = new();
    private readonly int _capacity;
    private int _count;

    public InMemoryLogStore(int capacity = 500)
    {
        _capacity = Math.Max(50, capacity);
    }

    public void Add(LogRecord entry)
    {
        _entries.Enqueue(entry);
        var current = Interlocked.Increment(ref _count);

        while (current > _capacity && _entries.TryDequeue(out _))
        {
            current = Interlocked.Decrement(ref _count);
        }
    }

    public LogPage GetPage(
        int page,
        int pageSize,
        string? serviceName = null,
        string? severity = null,
        string? search = null,
        string? correlationId = null,
        string? requestPath = null)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = Math.Clamp(pageSize, 1, 100);

        var snapshot = _entries.ToArray();
        Array.Reverse(snapshot);

        IEnumerable<LogRecord> query = snapshot;

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            query = query.Where(entry => string.Equals(entry.ServiceName, serviceName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(entry => string.Equals(entry.Severity, severity, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            query = query.Where(entry => Contains(entry.CorrelationId, correlationId));
        }

        if (!string.IsNullOrWhiteSpace(requestPath))
        {
            query = query.Where(entry => Contains(entry.RequestPath, requestPath));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(entry =>
                Contains(entry.Message, search) ||
                Contains(entry.Payload, search) ||
                Contains(entry.StackTrace, search) ||
                Contains(entry.RequestPath, search));
        }

        var filtered = query as LogRecord[] ?? query.ToArray();

        var total = filtered.Length;
        var skip = (normalizedPage - 1) * normalizedSize;
        var items = skip >= total
            ? Array.Empty<LogRecord>()
            : filtered.Skip(skip).Take(normalizedSize).ToArray();

        return new LogPage(normalizedPage, normalizedSize, total, items);
    }

    private static bool Contains(string? source, string value)
    {
        return !string.IsNullOrWhiteSpace(source) &&
               source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}
