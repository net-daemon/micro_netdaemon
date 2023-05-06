using Xunit;

namespace MicroHomeAssistantClientTests.Integration;

public class HomeAssistantServiceFixture : IAsyncLifetime
{
    public HomeAssistantMock? HaMock { get; set; }

    public async Task DisposeAsync()
    {
        if (HaMock is not null)
            await HaMock.DisposeAsync().ConfigureAwait(false);
    }

    public Task InitializeAsync()
    {
        HaMock = new HomeAssistantMock();
        return Task.CompletedTask;
    }
}