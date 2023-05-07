using MicroHomeAssistantClient.Model;

namespace MicroHomeAssistantClient.Internal.Json;

[JsonSerializable(typeof(CallServiceCommand))]
internal partial class CallServiceCommandSerializationContext : JsonSerializerContext
{
}

