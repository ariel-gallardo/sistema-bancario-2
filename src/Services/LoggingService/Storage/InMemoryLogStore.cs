using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

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

    public LogPage GetPage(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedSize = Math.Clamp(pageSize, 1, 100);

        var snapshot = _entries.ToArray();
        Array.Reverse(snapshot);

        var total = snapshot.Length;
        var skip = (normalizedPage - 1) * normalizedSize;
        var items = skip >= total
            ? Array.Empty<LogRecord>()
            : snapshot.Skip(skip).Take(normalizedSize).ToArray();

        return new LogPage(normalizedPage, normalizedSize, total, items);
    }
}
