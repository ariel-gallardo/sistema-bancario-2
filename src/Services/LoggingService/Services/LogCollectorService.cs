using BankingApp.Logging;
using Grpc.Core;

namespace LoggingService.Services;

public sealed class LogCollectorService : LogCollector.LogCollectorBase
{
    private readonly ILogger<LogCollectorService> _logger;

    public LogCollectorService(ILogger<LogCollectorService> logger)
    {
        _logger = logger;
    }

    public override Task<LogResponse> SendErrorLog(LogRequest request, ServerCallContext context)
    {
        _logger.LogError("[{Service}] {Message} (Trace {TraceId})", request.ServiceName, request.Message, request.CorrelationId);

        return Task.FromResult(new LogResponse
        {
            Accepted = true,
            Error = string.Empty
        });
    }
}
