using System;
using BankingApp.Logging;
using BankingApp.SharedKernel.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.GrpcContracts.Logging;

public static class LoggingClientExtensions
{
    public static IServiceCollection AddGrpcLoggingClient(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["Logging:GrpcEndpoint"] ?? "https://logging";

        services.AddGrpcClient<LogCollector.LogCollectorClient>(options =>
        {
            options.Address = new Uri(endpoint);
        });

        services.AddSingleton<ILoggingClient, GrpcLoggingClient>();
        return services;
    }
}
