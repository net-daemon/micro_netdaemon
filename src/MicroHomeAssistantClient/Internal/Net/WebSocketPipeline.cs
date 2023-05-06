using System.Text;
using System.Text.Json.Nodes;
using MicroHomeAssistantClient.Internal.Json;

namespace MicroHomeAssistantClient.Internal.Net;

internal class WebSocketPipeline : IAsyncDisposable
{
    private readonly Pipe _pipe = new();
    private readonly ClientWebSocket _ws;
    private readonly CancellationTokenSource _internalCancelSource = new();
    private readonly JsonElementSerializationContext _jsonElementSerializationContext = new( 
        new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    
    private static int DefaultTimeOut => 5000;
    
    public WebSocketPipeline(ClientWebSocket webSocketClient)
    {
        _ws = webSocketClient ?? throw new ArgumentNullException(nameof(webSocketClient));;
    }

    public async Task CloseAsync()
    {
        await SendCloseFrameToRemoteWebSocket().ConfigureAwait(false);
    }
    
    public async ValueTask DisposeAsync()
    {
        try
        {
            // In case we are just "disposing" without disconnect first
            // we call the close and fail silently if so
            await CloseAsync();
            _ws.Dispose();
        }
        catch
        {
            // Ignore all error in dispose since we do not know the state of the websocket
        }
    }
    
    // public Task SendAsync(object obj, JsonSerializerContext jsonSerializerContext, CancellationToken cancelToken)
    // {
    //     if (cancelToken.IsCancellationRequested || _ws.State != WebSocketState.Open || _ws.CloseStatus.HasValue)
    //         throw new ApplicationException("Sending message on closed socket!");
    //
    //     using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
    //         _internalCancelSource.Token,
    //         cancelToken
    //     );
    //
    //     var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(obj, obj.GetType(),
    //         jsonSerializerContext);
    //
    //     return _ws.SendAsync(jsonBytes, WebSocketMessageType.Text, true, combinedTokenSource.Token);
    // }
    
    public Task SendJsonElementAsync(JsonElement jsonElement, CancellationToken cancelToken)
    {
        if (cancelToken.IsCancellationRequested || _ws.State != WebSocketState.Open || _ws.CloseStatus.HasValue)
            throw new ApplicationException("Sending message on closed socket!");

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _internalCancelSource.Token,
            cancelToken
        );

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(jsonElement, jsonElement.GetType(),
            _jsonElementSerializationContext);

        return _ws.SendAsync(jsonBytes, WebSocketMessageType.Text, true, combinedTokenSource.Token);
    }
    
    public Task SendJsonNodeAsync(JsonNode jsonNode, CancellationToken cancelToken)
    {
        if (cancelToken.IsCancellationRequested || _ws.State != WebSocketState.Open || _ws.CloseStatus.HasValue)
            throw new ApplicationException("Sending message on closed socket!");

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _internalCancelSource.Token,
            cancelToken
        );

        var jsonBytes = Encoding.UTF8.GetBytes(jsonNode.ToJsonString());

        return _ws.SendAsync(jsonBytes, WebSocketMessageType.Text, true, combinedTokenSource.Token);
    }
    
    public Task SendRawAsync(byte[] rawMessage, CancellationToken cancelToken)
    {
        if (cancelToken.IsCancellationRequested || _ws.State != WebSocketState.Open || _ws.CloseStatus.HasValue)
            throw new ApplicationException("Sending message on closed socket!");

        using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _internalCancelSource.Token,
            cancelToken
        );
        
        return _ws.SendAsync(rawMessage, WebSocketMessageType.Text, true, combinedTokenSource.Token);
    }
    
    /// <summary>
    ///     Continuously reads the data from the pipe and serialize to JsonElement
    ///     from the json that are read
    /// </summary>
    /// <param name="cancelToken">Cancellation token</param>
    public async Task<JsonElement> ReadAsync(CancellationToken cancelToken)
    {
        try
        {
            using var combinedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                _internalCancelSource.Token,
                cancelToken
            );

            var serializerTask = SerializeJsonFromPipeStream(combinedTokenSource.Token);
            await ReadMessageFromWebSocketAndWriteToPipelineAsync(combinedTokenSource.Token);
            var jsonElement = await serializerTask;
            combinedTokenSource.Token.ThrowIfCancellationRequested();
            return jsonElement;
        }
        finally
        {
            // Reset pipe after every read
            _pipe.Reset();
        }
    }

    private async Task<JsonElement> SerializeJsonFromPipeStream(CancellationToken cancelToken)
    {
        try
        {
            return (JsonElement?) await JsonSerializer.DeserializeAsync(_pipe.Reader.AsStream(),
                                     typeof(JsonElement), _jsonElementSerializationContext, cancelToken).ConfigureAwait(false)
                                 ?? throw new ApplicationException(
                                     "Deserialization of websocket returned empty result (null)");
        }
        finally
        {
            // Always complete the reader
            await _pipe.Reader.CompleteAsync().ConfigureAwait(false);
        }
    }
    
    /// <summary>
    ///     Read one or more chunks of a message and writes the result
    ///     to the pipeline
    /// </summary>
    /// <remarks>
    ///     A websocket message can be 1 to several chunks of data.
    ///     As data are read it is written on the pipeline for
    ///     the json serializer in function ReadMessageFromPipelineAndSerializeAsync
    ///     to continuously serialize. Using pipes is very efficient
    ///     way to reuse memory and get speedy results
    /// </remarks>
    private async Task ReadMessageFromWebSocketAndWriteToPipelineAsync(CancellationToken cancelToken)
    {
        try
        {
            while (!cancelToken.IsCancellationRequested && !_ws.CloseStatus.HasValue)
            {
                var memory = _pipe.Writer.GetMemory();
                var result = await _ws.ReceiveAsync(memory, cancelToken).ConfigureAwait(false);
                
                if (
                    _ws.State == WebSocketState.Open &&
                    result.MessageType != WebSocketMessageType.Close)
                {
                    _pipe.Writer.Advance(result.Count);

                    await _pipe.Writer.FlushAsync(cancelToken).ConfigureAwait(false);

                    if (result.EndOfMessage) break;
                }
                else if (_ws.State == WebSocketState.CloseReceived)
                {
                    // We got a close message from server or if it still open we got canceled
                    // in both cases it is important to send back the close message
                    await SendCloseFrameToRemoteWebSocket().ConfigureAwait(false);

                    // Cancel so the write thread is canceled before pipe is complete
                    await _internalCancelSource.CancelAsync();
                }
            }
        }
        finally
        {
            // We have successfully read the whole message, 
            // make available to reader 
            // even if failure or we cannot reset the pipe
            await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    ///     Closes correctly the websocket depending on websocket state
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Closing a websocket has special handling. When the client
    ///         wants to close it calls CloseAsync and the websocket takes
    ///         care of the proper close handling.
    ///     </para>
    ///     <para>
    ///         If the remote websocket wants to close the connection dotnet
    ///         implementation requires you to use CloseOutputAsync instead.
    ///     </para>
    ///     <para>
    ///         We do not want to cancel operations until we get closed state
    ///         this is why own timer cancellation token is used and we wait
    ///         for correct state before returning and disposing any connections
    ///     </para>
    /// </remarks>
    private async Task SendCloseFrameToRemoteWebSocket()
    {
        using var timeout = new CancellationTokenSource(DefaultTimeOut);

        try
        {
            switch (_ws.State)
            {
                case WebSocketState.CloseReceived:
                {
                    // after this, the socket state which change to CloseSent
                    await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token)
                        .ConfigureAwait(false);
                    // now we wait for the server response, which will close the socket
                    while (_ws.State != WebSocketState.Closed && !timeout.Token.IsCancellationRequested)
                        await Task.Delay(100, timeout.Token).ConfigureAwait(false);
                    break;
                }
                case WebSocketState.Open:
                {
                    // Do full close 
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", timeout.Token)
                        .ConfigureAwait(false);
                    if (_ws.State != WebSocketState.Closed)
                        throw new ApplicationException("Expected the websocket to be closed!");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // normal upon task/token cancellation, disregard
        }
        finally
        {
            // After the websocket is properly closed
            // we can safely cancel all actions
            if (!_internalCancelSource.IsCancellationRequested)
                await _internalCancelSource.CancelAsync();
        }
    }

}