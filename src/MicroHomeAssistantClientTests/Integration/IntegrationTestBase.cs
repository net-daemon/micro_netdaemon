using MicroHomeAssistantClient;
using MicroHomeAssistantClient.Extensions;
using MicroHomeAssistantClient.Internal;
using MicroHomeAssistantClient.Internal.Net;
using MicroHomeAssistantClient.Settings;
using MicroHomeAssistantClientTests.Helpers;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MicroHomeAssistantClientTests.Integration;


public class IntegrationTestBase : IClassFixture<HomeAssistantServiceFixture>
{
    protected readonly CancellationTokenSource TokenSource = new(TestSettings.DefaultTimeout);

    protected IntegrationTestBase(HomeAssistantServiceFixture fixture)
    {
        HaFixture = fixture;
    }

    protected HomeAssistantServiceFixture HaFixture { get; }

    /// <summary>
    ///     Returns a connection Home Assistant instance
    /// </summary>
    /// <param name="haSettings">Provide custom setting</param>
    internal async Task<TestContext> GetConnectedClientContext(HomeAssistantSettings? haSettings = null)
    {
        var mock = HaFixture.HaMock ?? throw new ApplicationException("Unexpected for the mock server to be null");
        var optionsMock = new Mock<IOptions<NetDaemonJsonOptions>>();
        optionsMock.SetupGet(n => n.Value).Returns(new NetDaemonJsonOptions());
        
        var loggerClient = new Mock<ILogger<HaClient>>();
        var loggerConnection = new Mock<ILogger<IHaConnection>>();
        var settings = haSettings ?? new HomeAssistantSettings
        {
            Host = "127.0.0.1",
            Port = mock.ServerPort,
            Ssl = false,
            Token = "ABCDEFGHIJKLMNOPQ"
        };

        var appSettingsOptions = Options.Create(settings);

        var client = new HaClient(
            loggerClient.Object,
            new ClientWebsocketFactory(),
            optionsMock.Object
        );
        var connection = await client.ConnectAsync(
            settings.Host,
            settings.Port,
            settings.Ssl,
            settings.Token,
            settings.WebsocketPath,
            TokenSource.Token
        ).ConfigureAwait(false);

        return new TestContext
        {
            HomeAssistantLogger = loggerClient,
            HomeAssistantConnectionLogger = loggerConnection,
            HomeAssistantConnection = connection
        };
    }
}
