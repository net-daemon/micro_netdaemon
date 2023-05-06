using MicroHomeAssistantClient;
using MicroHomeAssistantClient.Internal;
using Moq;

namespace MicroHomeAssistantClientTests.Integration;

internal record TestContext : IAsyncDisposable
{
    public Mock<ILogger<HaClient>> HomeAssistantLogger { get; init; } = new();
    public Mock<ILogger<IHaConnection>> HomeAssistantConnectionLogger { get; init; } = new();
    public IHaConnection HomeAssistantConnection { get; init; } = new Mock<IHaConnection>().Object;

    public async ValueTask DisposeAsync()
    {
        await HomeAssistantConnection.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}