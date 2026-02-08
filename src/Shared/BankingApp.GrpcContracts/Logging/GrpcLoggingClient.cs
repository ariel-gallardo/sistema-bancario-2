using BankingApp.Logging;
using BankingApp.SharedKernel.Logging;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace BankingApp.GrpcContracts.Logging;

public sealed class GrpcLoggingClient : ILoggingClient
{
    private readonly LogCollector.LogCollectorClient _client;
    private readonly ILogger<GrpcLoggingClient> _logger;

    public GrpcLoggingClient(LogCollector.LogCollectorClient client, ILogger<GrpcLoggingClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task SendErrorAsync(ErrorLogEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new LogRequest
            {
                ServiceName = entry.ServiceName,
                RequestPath = entry.RequestPath ?? string.Empty,
                Message = entry.Message,
                StackTrace = entry.StackTrace ?? string.Empty,
                Severity = entry.Severity,
                CorrelationId = entry.CorrelationId ?? string.Empty,
                Payload = entry.Payload ?? string.Empty
            };

            await _client.SendErrorLogAsync(request, cancellationToken: cancellationToken);
        }
        catch (RpcException rpcEx)
        {
            _logger.LogWarning(rpcEx, "gRPC logging failed with status {Status}", rpcEx.StatusCode);
        }
    }
}
