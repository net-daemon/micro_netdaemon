using System.Text.Json;
using System.Text.Json.Serialization;

namespace MicroHomeAssistantClientTests.Json;

[JsonSerializable(typeof(JsonElement))]
public partial class JsonElementSerializationContext : JsonSerializerContext
{
}
