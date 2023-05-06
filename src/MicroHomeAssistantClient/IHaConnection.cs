using System.Text.Json.Nodes;

namespace MicroHomeAssistantClient;

public interface IHaConnection : IAsyncDisposable
{
    Task<JsonElement> SendCommandAsync(JsonNode command, CancellationToken cancelToken);

    string HaVersion { get; }
    
    IObservable<JsonElement> HaMessages { get; }
}