
using MicroHomeAssistantClient.Common;

namespace MicroHomeAssistantClient.Internal;

public class HaClient : IHaClient
{
    private readonly ILogger<IHaClient> _logger;
    private readonly IClientWebsocketFactory _clientWebsocketFactory;

    public HaClient(ILogger<IHaClient> logger, IClientWebsocketFactory clientWebsocketFactory)
    {
        _logger = logger;
        _clientWebsocketFactory = clientWebsocketFactory;
    }
    public async Task<IHaConnection> ConnectAsync(string host, int port, bool ssl, string token, string websocketPath, CancellationToken cancelToken)
    {
        try
        {
            var websocketUri = GetHomeAssistantWebSocketUri(host, port, ssl, websocketPath);
            _logger.LogDebug("Connecting to Home Assistant websocket on {path}", websocketUri);
            var ws = _clientWebsocketFactory.New();
            await ws.ConnectAsync(websocketUri, cancelToken);

            var connection = new HaConnection(_logger, ws, token);
            await connection.InitializeAsync(token, cancelToken);
            return connection;
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Connect to Home Assistant was cancelled");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogDebug(e, "Error connecting to Home Assistant");
            throw;
        }
    }
    
    private static Uri GetHomeAssistantWebSocketUri(string host, int port, bool ssl, string websocketPath)
    {
        return new Uri($"{(ssl ? "wss" : "ws")}://{host}:{port}/{websocketPath}");
    }
}