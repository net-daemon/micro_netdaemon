using MicroHomeAssistantClient.Model;

namespace MicroHomeAssistantClient.Internal.Json;

[JsonSerializable(typeof(HassCommandResult))]
public partial class HassCommandResultSerializationContext : JsonSerializerContext
{
}

