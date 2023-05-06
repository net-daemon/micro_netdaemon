namespace MicroHomeAssistantClient;

public interface IHaClient
{
    Task<IHaConnection> ConnectAsync(string host, int port, bool ssl, string token,
        string websocketPath,
        CancellationToken cancelToken);
}

