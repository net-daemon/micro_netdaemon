namespace MicroHomeAssistantClient.Model;

public record HassCommandResult
{
    [JsonPropertyName("id")] public int Id { get; init; }
    [JsonPropertyName("type")] public string Type { get; init; } = string.Empty;
    [JsonPropertyName("success")] public bool Success { get; init; }
    [JsonPropertyName("result")] public JsonElement? ResultElement { get; init; }
    [JsonPropertyName("error")] public HassError? Error { get; init; }
}