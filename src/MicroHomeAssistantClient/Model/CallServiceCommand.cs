namespace MicroHomeAssistantClient.Model;

internal record CallServiceCommand
{

    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("type")] public string Type { get; } = "call_service";
    
    [JsonPropertyName("domain")] public string Domain { get; init; } = string.Empty;

    [JsonPropertyName("service")] public string Service { get; init; } = string.Empty;

    [JsonPropertyName("service_data")] public object? ServiceData { get; init; }

    [JsonPropertyName("target")] public HassTarget? Target { get; init; }
}