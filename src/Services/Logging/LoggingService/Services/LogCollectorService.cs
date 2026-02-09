using BankingApp.Logging;
using Grpc.Core;
using LoggingService.Storage;

namespace LoggingService.Services;

public sealed class LogCollectorService : LogCollector.LogCollectorBase
{
    private readonly ILogger<LogCollectorService> _logger;
    private readonly ILogStore _store;

    public LogCollectorService(ILogger<LogCollectorService> logger, ILogStore store)
    {
        _logger = logger;
        _store = store;
    }

    public override Task<LogResponse> SendErrorLog(LogRequest request, ServerCallContext context)
    {
        _logger.LogError("[{Service}] {Message} (Trace {TraceId})", request.ServiceName, request.Message, request.CorrelationId);

        var entry = new LogRecord(
            Guid.NewGuid(),
            DateTime.UtcNow,
            request.ServiceName,
            request.Message,
            string.IsNullOrWhiteSpace(request.Severity) ? "Error" : request.Severity,
            request.RequestPath,
            request.StackTrace,
            request.CorrelationId,
            request.Payload);

        try
        {
            _store.Add(entry);

            return Task.FromResult(new LogResponse
            {
                Accepted = true,
                Error = string.Empty
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not persist log entry for {Service}", request.ServiceName);

            return Task.FromResult(new LogResponse
            {
                Accepted = false,
                Error = "Failed to persist log entry"
            });
        }
    }
}
