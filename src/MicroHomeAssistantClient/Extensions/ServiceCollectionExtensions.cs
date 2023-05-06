using MicroHomeAssistantClient.Internal;
using MicroHomeAssistantClient.Internal.Net;

namespace MicroHomeAssistantClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMicroHomeAssistantClient(this IServiceCollection services)
    {
        services
            .AddSingleton<HaClient>()
            .AddSingleton<IHaClient>(s => s.GetRequiredService<HaClient>())
            .AddSingleton<ClientWebsocketFactory>()
            .AddSingleton<IClientWebsocketFactory>(s => s.GetRequiredService<ClientWebsocketFactory>());

        //     .AddSingleton<IHomeAssistantClient>(s => s.GetRequiredService<HomeAssistantClient>())
        //     .AddSingleton<HomeAssistantRunner>()
        //     .AddSingleton<IHomeAssistantRunner>(s => s.GetRequiredService<HomeAssistantRunner>())
        //     .AddSingleton<HomeAssistantApiManager>()
        //     .AddSingleton<IHomeAssistantApiManager>(s => s.GetRequiredService<HomeAssistantApiManager>())
        //     .AddTransient(s => s.GetRequiredService<IHomeAssistantRunner>().CurrentConnection!)
        //     .AddWebSocketFactory()
        //     .AddPipelineFactory()
        //     .AddConnectionFactory()
        //     .AddHttpClientAndFactory();
        return services;
    }
}