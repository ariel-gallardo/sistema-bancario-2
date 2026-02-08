using System;
using BankingApp.Logging;
using BankingApp.SharedKernel.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BankingApp.GrpcContracts.Logging;

public static class LoggingClientExtensions
{
    public static IServiceCollection AddGrpcLoggingClient(this IServiceCollection services, IConfiguration configuration)
    {

        var configuredEndpoint = configuration["Logging:GrpcEndpoint"] ?? "https://localhost:7198";
        var targetUri = new Uri("http://logging-service");

        services.AddGrpcClient<LogCollector.LogCollectorClient>(options =>
        {
            options.Address = targetUri;
        });

        services.AddSingleton<ILoggingClient, GrpcLoggingClient>();
        return services;
    }
}
