namespace MicroHomeAssistantClient;

public interface IClientWebsocketFactory
{
    public ClientWebSocket New() => new();
}