

using MicroHomeAssistantClient.Common.Exceptions;
using MicroHomeAssistantClient.Internal.Helpers;
using MicroHomeAssistantClient.Internal.Json;
using MicroHomeAssistantClient.Internal.Net;
using MicroHomeAssistantClient.Model;

namespace MicroHomeAssistantClient.Internal;

internal class HaConnection : IHaConnection
{
    private readonly ILogger _logger;
    private readonly WebSocketPipeline _wsPipeLine;
    private readonly CancellationTokenSource _internalCancelSource = new();
    private readonly SemaphoreSlim _messageIdSemaphore = new(1, 1);
    private readonly SemaphoreSlim _sendSemaphore = new(1, 1);
    private int _messageId;
    
    private readonly Subject<JsonElement> _haMessageSubject = new();
    private Task _processTask = Task.CompletedTask;

    public IObservable<JsonElement> HaMessages => _haMessageSubject;

    public HassCommandResultSerializationContext _hassCommandResultSerializationContext = new();
    public async Task<HassCommandResult> SendCommandAndWaitForResultAsync(JsonNode jsonCommand, CancellationToken cancelToken)
    {
        await _sendSemaphore.WaitAsync(cancelToken).ConfigureAwait(false);
        jsonCommand["id"] = ++_messageId;
        try
        {
            // We make a task that subscribe for the return result message
            // this task will be returned and handled by caller
            // We dont want to pass the incoming CancellationToken here because it will throw a TaskCanceledException
            // and hide possible actual exceptions
            var resultEvent = _haMessageSubject
                .Where(n => HaMessageHelper.GetHaMessageType(n) == "result" && HaMessageHelper.GetMessageId(n)== _messageId)
                .FirstAsync().ToTask(CancellationToken.None);
         
            await _wsPipeLine.SendJsonNodeAsync(jsonCommand, cancelToken);

            var jsonResult = await resultEvent;

            return (HassCommandResult?) jsonResult.Deserialize(typeof(HassCommandResult), _hassCommandResultSerializationContext) ?? throw new InvalidOperationException("Failed to deserialize result message");
        }
        finally
        {
            _sendSemaphore.Release();
        }
    }

    public async Task SendCommandAsync(JsonNode jsonCommand, CancellationToken cancelToken)
    {
        await _sendSemaphore.WaitAsync(cancelToken).ConfigureAwait(false);
        jsonCommand["id"] = ++_messageId;
        try
        {
            await _wsPipeLine.SendJsonNodeAsync(jsonCommand, cancelToken);
        }
        finally
        {
            _sendSemaphore.Release();
        }
    }

    public Task<HassCommandResult> SendCommandAndWaitForResultAsync(object command, CancellationToken cancelToken)
    {
        throw new NotImplementedException();
    }

    public string HaVersion { get; private set; } = string.Empty;

    public async Task InitializeAsync(string token, CancellationToken cancelToken)
    {
        await HandleAuthorization(token, cancelToken);
        if (Version.Parse(HaVersion) >= new Version(2022, 9))
        {
            await HandleCoalesceSupport(cancelToken).ConfigureAwait(false);
        } 
        _processTask = ProcessNewMessagesFromWebsocket(cancelToken);
    }
    public HaConnection(ILogger logger, ClientWebSocket webSocketClient, string token)
    {
        _logger = logger;
        _wsPipeLine = new WebSocketPipeline(webSocketClient);
    }

    private async Task HandleCoalesceSupport(CancellationToken cancelToken)
    {
        // We do not need to protect message id with semaphore here since we are sure no other 
        // messages are sent
        await _wsPipeLine.SendRawAsync(
                HaCommandHelper.GetSupportedFeaturesMessageBytes(++_messageId), cancelToken)
            .ConfigureAwait(false);
        
        var commandResultMessage = await _wsPipeLine.ReadAsync(cancelToken).ConfigureAwait(false);

        if (!HaMessageHelper.GetResultSuccess(commandResultMessage))
            throw new HaConncetionException("Could not set coalesce support!");
    }
    
    private async Task HandleAuthorization(string token,
        CancellationToken cancelToken)
    {
        // Begin the authorization sequence
        // Expect 'auth_required' 
        var msg = await _wsPipeLine.ReadAsync(cancelToken)
            .ConfigureAwait(false);

        var msgType = HaMessageHelper.GetHaMessageType(msg);

        if (msgType != "auth_required")
            throw new ApplicationException($"Unexpected type: '{msgType}' expected 'auth_required'");

        // Now send the auth message to Home Assistant
        await _wsPipeLine.SendRawAsync(
                HaCommandHelper.GetAuthorizationMessageBytes(token), cancelToken)
            .ConfigureAwait(false);

        // Get the auto result
        var authResultMessage = await _wsPipeLine.ReadAsync(cancelToken).ConfigureAwait(false);

        msgType = HaMessageHelper.GetHaMessageType(authResultMessage);
        switch (msgType)
        {
            case "auth_ok":

                HaVersion = HaMessageHelper.GetHaVersion(msg);
                return;

            case "auth_invalid":
                await _wsPipeLine.CloseAsync().ConfigureAwait(false);
                throw new HaConncetionException(HaDisconnectReason.Unauthorized);

            default:
                throw new HaConncetionException($"Unexpected response ({msgType})");
        }
    }

    private async Task ProcessNewMessagesFromWebsocket(CancellationToken cancelToken)
    {
        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _internalCancelSource.Token,
            cancelToken
        );
        try
        {
            while (!combinedTokenSource.IsCancellationRequested)
            {

                var msg = await _wsPipeLine.ReadAsync(combinedTokenSource.Token);
                if (msg.ValueKind == JsonValueKind.Array)
                {
                    // We have coalesce messages
                    foreach (var element in msg.EnumerateArray())
                    {
                        _logger.LogTrace("Coalesce message received: \r\n {Element}", element);
                        _haMessageSubject.OnNext(element);
                    }
                }
                else
                {
                    _logger.LogTrace("Message received: \r\n {Element}", msg);
                    _haMessageSubject.OnNext(msg);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Normal just ignore
        }
        finally
        {
            _logger.LogTrace("Stop processing new messages");
            // make sure we always cancel any blocking operations
            if (!_internalCancelSource.IsCancellationRequested)
                await _internalCancelSource.CancelAsync();
        }
    }
    public async ValueTask DisposeAsync()
    {
        if (!_internalCancelSource.IsCancellationRequested)
            await _internalCancelSource.CancelAsync();
        await _wsPipeLine.DisposeAsync();
    }
}