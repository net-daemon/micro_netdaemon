using System.Text.Json.Nodes;
using MicroHomeAssistantClient.Extensions;
using MicroHomeAssistantClient.Model;

namespace MicroHomeAssistantClient;

public interface IHaConnection : IAsyncDisposable
{
    Task<HassCommandResult> SendCommandAndWaitForResultAsync(JsonNode command, CancellationToken cancelToken);
    Task SendCommandAsync(JsonNode command, CancellationToken cancelToken);
    
    Task<HassCommandResult> SendCommandAndWaitForResultAsync(object command, CancellationToken cancelToken);

    string HaVersion { get; }
    
    IObservable<JsonElement> HaMessages { get; }
    
    NetDaemonJsonOptions JsonOptions { get; }
}