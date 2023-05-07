using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using MicroHomeAssistantClient.Settings;
using Microsoft.Extensions.Hosting;

namespace MicroHomeAssistantClient.Extensions;

public static class NetDaemonConfigureJsonOptions
{
    public static IServiceCollection ConfigureNetDaemonJsonOptions(this IServiceCollection services,
        Action<NetDaemonJsonOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }
    
}

public class NetDaemonJsonOptions
{
    public JsonSerializerOptions SerializerOptions { get; } = new();
}