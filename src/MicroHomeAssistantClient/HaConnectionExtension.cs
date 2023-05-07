using System.Text.Json.Nodes;
using System.Xml.XPath;
using MicroHomeAssistantClient.Internal.Json;
using MicroHomeAssistantClient.Model;
using Microsoft.AspNetCore.Http.Json;

namespace MicroHomeAssistantClient;

public static class HaConnectionExtension
{
    public static async Task<HassCommandResult> CallService(this IHaConnection haConnection, string domain, string service, JsonObject serviceDataObject, CancellationToken cancelToken)
    {
        var x = new JsonObject(new[]
        {
            KeyValuePair.Create<string, JsonNode?>("type", "call_service"),
            KeyValuePair.Create<string, JsonNode?>("domain", domain),
            KeyValuePair.Create<string, JsonNode?>("service", service),
            KeyValuePair.Create<string, JsonNode?>("service_data", serviceDataObject),
        });

        return await haConnection.SendCommandAndWaitForResultAsync(x, cancelToken);
    }

    public static async Task<HassCommandResult> CallService(this IHaConnection haConnection, string domain, string service, object? serviceData, HassTarget? target, CancellationToken? cancelToken = null)
    {
        var x = new JsonObject(new[]
        {
            KeyValuePair.Create<string, JsonNode?>("type", "call_service"),
            KeyValuePair.Create<string, JsonNode?>("domain", domain),
            KeyValuePair.Create<string, JsonNode?>("service", service),
        });
        if (target is not null)
        {
            x["target"] = JsonObject.Create(JsonSerializer.SerializeToElement(target, typeof(HassTarget),
                new HassTargetSerializationContext()));
        }

        if (serviceData is not null)
        {
            var element = JsonSerializer.SerializeToElement(serviceData, serviceData.GetType(), haConnection.JsonOptions.SerializerOptions);
            x["service_data"] = JsonObject.Create(element);
        }

        return await haConnection.SendCommandAndWaitForResultAsync(x, cancelToken ?? CancellationToken.None);
    }

    public static async Task<IObservable<JsonElement>> SubscribeEventsAsync(this IHaConnection haConnection, string? eventType, CancellationToken cancelToken)
    {
        eventType ??= "*";
        
        var x = new JsonObject(new[]
        {
            KeyValuePair.Create<string, JsonNode?>("type", "subscribe_events"),
            KeyValuePair.Create<string, JsonNode?>("event_type", eventType),
        });
        
        var result = await haConnection.SendCommandAndWaitForResultAsync(x, cancelToken);

        if (!result.Success)
            throw new InvalidOperationException($"Subscribe to events failed, {result}");

        return haConnection.HaMessages.Where(n => HaMessageHelper.GetMessageId(n) == result.Id);
    }

    public static async Task SendSimpleCommandAsync(this IHaConnection haConnection,
        string? commandType, CancellationToken cancelToken)
    {
        var x = new JsonObject(new[]
        {
            KeyValuePair.Create<string, JsonNode?>("type", commandType),
        });
        
        await haConnection.SendCommandAsync(x, cancelToken);
    }
    
    public static async Task<HassCommandResult> SendSimpleCommandAndWaitForResultAsync(this IHaConnection haConnection,
        string? commandType, CancellationToken cancelToken)
    {
        var x = new JsonObject(new[]
        {
            KeyValuePair.Create<string, JsonNode?>("type", commandType),
        });
        
        var result = await haConnection.SendCommandAndWaitForResultAsync(x, cancelToken);

        if (!result.Success)
            throw new InvalidOperationException($"Subscribe to events failed, {result}");

        return result;
    }
}

