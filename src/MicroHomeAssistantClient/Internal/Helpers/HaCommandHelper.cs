using System.Text;

namespace MicroHomeAssistantClient.Internal.Helpers;

public static class HaCommandHelper
{
    public static byte[] GetAuthorizationMessageBytes(string token) => Encoding.UTF8.GetBytes( $$"""{"type":"auth","access_token":"{{token}}"}""");
    public static byte[] GetSupportedFeaturesMessageBytes(int id) => Encoding.UTF8.GetBytes($$$"""{"type":"supported_features","id":{{{id}}},"features":{"coalesce_messages":1}}""");
}