using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text.Json.Nodes;
using FluentAssertions;
using MicroHomeAssistantClient;
using MicroHomeAssistantClientTests.Helpers;
using Xunit;

namespace MicroHomeAssistantClientTests.Integration;

public class IntegrationTests : IntegrationTestBase
{
    public IntegrationTests(HomeAssistantServiceFixture fixture) : base(fixture)
    {
        
    }
    
    [Fact]
    public async Task TestSuccessfulConnectShouldReturnConnection()
    {
        await using var ctx = await GetConnectedClientContext().ConfigureAwait(false);
        ctx.HomeAssistantConnection.Should().NotBeNull();
    }
    
    [Fact]
    public async Task TestCallServiceShouldSucceed()
    {
        await using var ctx = await GetConnectedClientContext().ConfigureAwait(false);
        
        var serviceData = new JsonObject(
            new[]
            {
                KeyValuePair.Create<string, JsonNode?>("entity_id", new JsonArray("light.test")),
            }
        );
        
        var result = await ctx.HomeAssistantConnection
            .CallService(
                "domain",
                "service",
                serviceData,
                TokenSource.Token)
            .ConfigureAwait(false);

        result.Success.Should().BeTrue();
    }
    
    [Fact]
    public async Task TestSubscribeAndGetEvent()
    {
        using CancellationTokenSource tokenSource = new(TestSettings.DefaultTimeout + 1000);
        await using var ctx = await GetConnectedClientContext().ConfigureAwait(false);

        var events = await ctx.HomeAssistantConnection
            .SubscribeEventsAsync("*", tokenSource.Token).ConfigureAwait(false);

        var subscribeTask = events
            .FirstAsync()
            .ToTask(tokenSource.Token);

        var haEvent = await subscribeTask.ConfigureAwait(false);

        haEvent.Should().NotBeNull();
        
       haEvent.GetProperty("event").GetProperty("event_type").GetString()!
            .Should()
            .BeEquivalentTo("state_changed");

       haEvent.GetProperty("event")
           .GetProperty("data")
           .GetProperty("entity_id")
           .GetString()!
           .Should()
           .BeEquivalentTo("binary_sensor.vardagsrum_pir");

       haEvent.GetProperty("event")
           .GetProperty("data")
           .GetProperty("new_state")
           .GetProperty("attributes")
           .GetProperty("battery_level")
           .Should().NotBeNull();
    }
    
    [Fact]
    public async Task TestErrorReturnShouldThrowException()
    {
        await using var ctx = await GetConnectedClientContext().ConfigureAwait(false);
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await ctx.HomeAssistantConnection
            .SendSimpleCommandAndWaitForResultAsync(
                "fake_return_error",
                TokenSource.Token
            )
            .ConfigureAwait(false));
    }
    
    [Fact]
    public async Task TestPing()
    {
        await using var ctx = await GetConnectedClientContext().ConfigureAwait(false);

        var getMessageTask = ctx.HomeAssistantConnection.HaMessages
            .Where(n=> HaMessageHelper.GetHaMessageType(n) == "pong")
            .FirstAsync()
            .ToTask();
        
        await ctx.HomeAssistantConnection
            .SendSimpleCommandAsync(
                "ping",
                TokenSource.Token
            )
            .ConfigureAwait(false);

        var resultMsg = await getMessageTask;
    }
}