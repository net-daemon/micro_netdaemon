using System.Text.Json.Nodes;
using FluentAssertions;
using MicroHomeAssistantClient;
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

        HaMessageHelper.GetResultSuccess(result).Should().BeTrue();
    }
}