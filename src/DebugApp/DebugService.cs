
using MicroHomeAssistantClient;
using MicroHomeAssistantClient.Internal.Json;
using MicroHomeAssistantClient.Model;
using MicroHomeAssistantClient.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DebugApp;

public class DebugService : BackgroundService
{
    private readonly IHaClient _haClient;
    private readonly ILogger<DebugService> _logger;
    private IHaConnection? _haConnection;
    private IObservable<JsonElement> _allEvents = new Subject<JsonElement>();
    private IObservable<JsonElement> _allEvents2 = new Subject<JsonElement>();
    private readonly string _token = "";
    private readonly string _host;
    private readonly int _port;
    private readonly bool _ssl;

    public DebugService(IHaClient haClient, ILogger<DebugService> logger, IConfiguration configuration)
    {
        _haClient = haClient;
        _logger = logger;

        var section = configuration.GetSection("HomeAssistant");
        
        _token = section["Token"] ?? "";
        _host = section["Host"] ?? "";
        _port = int.Parse(section["Port"] ?? "1");
        _ssl = Boolean.Parse(section["Ssl"] ?? "false");
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _haConnection = await _haClient.ConnectAsync(_host, _port, _ssl, _token, "api/websocket", cancellationToken);
        _logger.LogInformation("Ha ({Version}) ar connected", _haConnection.HaVersion);
        var serviceData = new JsonObject(
            new[]
            {
                KeyValuePair.Create<string, JsonNode?>("entity_id", "input_boolean.baaaanan"),
            }
        );
        // var result = await _haConnection.CallService("input_boolean", "toggle", serviceData, cancellationToken);

        var result = await _haConnection.CallService("tts", "google_translate_say", new TtsGoogleTranslateSayParameters()
        {
            Message = "Hello"
        }, new HassTarget()
        {
            EntityIds = new[] { "media_player.sovrum" }
        });
        
        _logger.LogInformation("Result from CallService: {Result}", result);
        
        _allEvents = await _haConnection.SubscribeEventsAsync("*", cancellationToken);
        _allEvents.Subscribe(s => _logger.LogInformation("New event: {Event}", s));
        _allEvents2 = await _haConnection.SubscribeEventsAsync("*", cancellationToken);
        _allEvents2.Subscribe(s => _logger.LogInformation("New event: {Event}", s));

        await _haConnection
            .SendSimpleCommandAsync(
                "ping",
                cancellationToken
            )
            .ConfigureAwait(false);
        

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