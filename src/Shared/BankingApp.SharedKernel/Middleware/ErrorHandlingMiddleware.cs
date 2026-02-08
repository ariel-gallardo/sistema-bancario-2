using System.Text.Json;
using BankingApp.SharedKernel.Logging;
using BankingApp.SharedKernel.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BankingApp.SharedKernel.Middleware;

public sealed class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly ILoggingClient _loggingClient;
    private readonly string _serviceName;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, ILoggingClient loggingClient, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _loggingClient = loggingClient;
        _serviceName = configuration["ServiceInfo:Name"] ?? "banking-service";
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var traceId = context.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception with trace {TraceId}", traceId);

            await SafeSendToGrpcAsync(ex, context, traceId);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var error = new ErrorResponse
            {
                Message = "Ha ocurrido un error inesperado. Intenta nuevamente más tarde.",
                TraceId = traceId,
                TimestampUtc = DateTime.UtcNow
            };

            await JsonSerializer.SerializeAsync(context.Response.Body, error, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }

    private async Task SafeSendToGrpcAsync(Exception ex, HttpContext context, string traceId)
    {
        try
        {
            await _loggingClient.SendErrorAsync(new ErrorLogEntry
            {
                ServiceName = _serviceName,
                RequestPath = context.Request.Path,
                Message = ex.Message,
                StackTrace = ex.StackTrace,
                CorrelationId = traceId,
                Payload = context.Request.Path.Value,
                Severity = "Critical"
            });
        }
        catch (Exception logEx)
        {
            _logger.LogWarning(logEx, "Unable to send error to logging service");
        }
    }
}
