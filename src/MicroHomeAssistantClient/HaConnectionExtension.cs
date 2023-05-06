using System.Text.Json.Nodes;

namespace MicroHomeAssistantClient;

public static class HaConnectionExtension
{
    public static async Task<JsonElement> CallService(this IHaConnection haConnection, string domain, string service, JsonObject serviceDataObject, CancellationToken cancelToken)
    {
        var x = new JsonObject(new[]
        {
            KeyValuePair.Create<string, JsonNode?>("type", "call_service"),
            KeyValuePair.Create<string, JsonNode?>("domain", domain),
            KeyValuePair.Create<string, JsonNode?>("service", service),
            KeyValuePair.Create<string, JsonNode?>("service_data", serviceDataObject),
        });

        return await haConnection.SendCommandAsync(x, cancelToken);
    }

    public static async Task<IObservable<JsonElement>> SubscribeEvents(this IHaConnection haConnection, string? eventType, CancellationToken cancelToken)
    {
        eventType ??= "*";
        
        var x = new JsonObject(new[]
        {
            KeyValuePair.Create<string, JsonNode?>("type", "subscribe_events"),
            KeyValuePair.Create<string, JsonNode?>("event_type", eventType),
        });
        
        var result = await haConnection.SendCommandAsync(x, cancelToken);

        if (!HaMessageHelper.GetResultSuccess(result))
            throw new InvalidOperationException($"Subscribe to events failed, {result}");

        var messageId = HaMessageHelper.GetMessageId(result);

        return haConnection.HaMessages.Where(n => HaMessageHelper.GetMessageId(n) == messageId);
    }
}