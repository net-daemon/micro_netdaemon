using MicroHomeAssistantClient.Model;

namespace MicroHomeAssistantClient.Internal.Json;

public record TestServiceData
{
    [JsonPropertyName("testdata")] public string TestDAta { get; init; }
}

[JsonSerializable(typeof(TestServiceData))]
internal partial class TestServiceDataSerializationContext : JsonSerializerContext
{
}

