namespace MicroHomeAssistantClient;

public static class HaMessageHelper
{
    public static string GetHaMessageType(JsonElement jsonElement) => jsonElement.GetProperty("type").GetString() ?? throw  new InvalidOperationException("Home assistant message expected to have type property");
    public static string GetHaVersion(JsonElement jsonElement) => jsonElement.GetProperty("ha_version").GetString() ?? throw  new InvalidOperationException("Home assistant message expected to have ha_version property");
    public static int GetMessageId(JsonElement jsonElement) => jsonElement.GetProperty("id").GetInt32();
    
    public static bool GetResultSuccess(JsonElement jsonElement) => jsonElement.GetProperty("success").GetBoolean();
}