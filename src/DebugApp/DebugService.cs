using System.Text.Json.Nodes;
using MicroHomeAssistantClient;
using MicroHomeAssistantClient.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DebugApp;

public class DebugService : BackgroundService
{
    private readonly IHaClient _haClient;
    private readonly ILogger<DebugService> _logger;
    private IHaConnection? _haConnection;
    private IObservable<JsonElement> _allEvents;

    public DebugService(IHaClient haClient, ILogger<DebugService> logger)
    {
        _haClient = haClient;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _haConnection = await _haClient.ConnectAsync("localhost", 8124, false, "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJmOWZlMDE1ZTI5OWE0NThlOTRkNjdkZTI0NDJiM2NlOSIsImlhdCI6MTY3NTUxNjIwOCwiZXhwIjoxOTkwODc2MjA4fQ.qMK43ZMjoFc12nVcumwzSFcYwKOOcweLm3a8ZznJ7L0", "api/websocket", cancellationToken);
        _logger.LogInformation("Ha ({Version}) ar connected", _haConnection.HaVersion);
        var serviceData = new JsonObject(
            new[]
            {
                KeyValuePair.Create<string, JsonNode?>("entity_id", "input_boolean.baaaanan"),
            }
        );
        var result = await _haConnection.CallService("input_boolean", "toggle", serviceData, cancellationToken);
        _logger.LogInformation("Result from CallService: {Result}", result);

        _allEvents = await _haConnection.SubscribeEvents("state_changed", cancellationToken);

        _allEvents.Subscribe(s => _logger.LogInformation("New event: {Event}", s));

    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("NetDaemon RuntimeService is stopping");
        if (_haConnection is not null)
            await _haConnection.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
    
}