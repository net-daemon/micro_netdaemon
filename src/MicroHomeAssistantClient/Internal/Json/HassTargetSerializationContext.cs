using MicroHomeAssistantClient.Model;

namespace MicroHomeAssistantClient.Internal.Json;

[JsonSerializable(typeof(HassTarget))]
public partial class HassTargetSerializationContext : JsonSerializerContext
{
}

