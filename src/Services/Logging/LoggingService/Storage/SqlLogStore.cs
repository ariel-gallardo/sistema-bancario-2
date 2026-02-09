using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoggingService.Storage;

public sealed class SqlLogStore : ILogStore
{
    private readonly LoggingDbContext _dbContext;
    private readonly ILogger<SqlLogStore> _logger;

    public SqlLogStore(LoggingDbContext dbContext, ILogger<SqlLogStore> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public void Add(LogRecord entry)
    {
        try
        {
            var entity = new LogEntry
            {
                Id = entry.Id,
                TimestampUtc = entry.TimestampUtc,
                ServiceName = entry.ServiceName,
                Message = entry.Message,
                Severity = entry.Severity,
                RequestPath = Normalize(entry.RequestPath),
                StackTrace = Normalize(entry.StackTrace),
                CorrelationId = Normalize(entry.CorrelationId),
                Payload = Normalize(entry.Payload)
            };

            _dbContext.Logs.Add(entity);
            _dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist log entry from {Service}", entry.ServiceName);
            throw;
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

        IQueryable<LogEntry> query = _dbContext.Logs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(serviceName))
        {
            query = query.Where(log => log.ServiceName == serviceName);
        }

        if (!string.IsNullOrWhiteSpace(severity))
        {
            query = query.Where(log => log.Severity == severity);
        }

        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            var like = BuildLikePattern(correlationId);
            query = query.Where(log => log.CorrelationId != null && EF.Functions.Like(log.CorrelationId, like));
        }

        if (!string.IsNullOrWhiteSpace(requestPath))
        {
            var like = BuildLikePattern(requestPath);
            query = query.Where(log => log.RequestPath != null && EF.Functions.Like(log.RequestPath, like));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var like = BuildLikePattern(search);
            query = query.Where(log =>
                EF.Functions.Like(log.Message, like) ||
                (log.Payload != null && EF.Functions.Like(log.Payload, like)) ||
                (log.StackTrace != null && EF.Functions.Like(log.StackTrace, like)) ||
                (log.RequestPath != null && EF.Functions.Like(log.RequestPath, like)));
        }

        var total = query.Count();
        var skip = (normalizedPage - 1) * normalizedSize;

        LogRecord[] items;
        if (skip >= total)
        {
            items = Array.Empty<LogRecord>();
        }
        else
        {
            items = query
                .OrderByDescending(log => log.TimestampUtc)
                .Skip(skip)
                .Take(normalizedSize)
                .AsEnumerable()
                .Select(ToRecord)
                .ToArray();
        }

        return new LogPage(normalizedPage, normalizedSize, total, items);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string BuildLikePattern(string value)
    {
        var trimmed = value.Trim();
        return $"%{trimmed}%";
    }

    private static LogRecord ToRecord(LogEntry entity)
    {
        return new LogRecord(
            entity.Id,
            entity.TimestampUtc,
            entity.ServiceName,
            entity.Message,
            entity.Severity,
            entity.RequestPath ?? string.Empty,
            entity.StackTrace ?? string.Empty,
            entity.CorrelationId ?? string.Empty,
            entity.Payload ?? string.Empty);
    }
}
