namespace MicroHomeAssistantClient;

/// <summary>
///     The reason websocket connection disconnected
/// </summary>
public enum HaDisconnectReason
{
    /// <summary>
    ///     Client disconnected
    /// </summary>
    Client,

    /// <summary>
    ///     Remote host disconnected
    /// </summary>
    Remote,

    /// <summary>
    ///     Remote host disconnected
    /// </summary>
    NotReady,

    /// <summary>
    ///     Error caused by unauthorized token
    /// </summary>
    Unauthorized,

    /// <summary>
    ///     Error caused disconnect
    /// </summary>
    Error
}