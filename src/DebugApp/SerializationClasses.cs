using MicroHomeAssistantClient.Model;

namespace MicroHomeAssistantClient.Internal.Json;

public record TtsGoogleTranslateSayParameters
{
    ///<summary>Name(s) of media player entities.</summary>
    [JsonPropertyName("entity_id")]
    public string? EntityId { get; init; }

    ///<summary>Text to speak on devices. eg: My name is hanna</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    ///<summary>Control file cache of this message.</summary>
    [JsonPropertyName("cache")]
    public bool? Cache { get; init; }

    ///<summary>Language to use for speech generation. eg: ru</summary>
    [JsonPropertyName("language")]
    public string? Language { get; init; }

    ///<summary>A dictionary containing platform-specific options. Optional depending on the platform. eg: platform specific</summary>
    [JsonPropertyName("options")]
    public object? Options { get; init; }
}

[JsonSerializable(typeof(TtsGoogleTranslateSayParameters))]
internal partial class TtsDataSerializationContext : JsonSerializerContext
{
}

