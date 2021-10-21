using System.Net.WebSockets;

namespace H.WebSockets.Args;

/// <summary>
/// Arguments used in <see cref="WebSocket"/> close event
/// </summary>
public class WebSocketCloseEventArgs : EventArgs
{
    /// <summary>
    /// Reason of disconnect
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Status of WebSocket
    /// </summary>
    public WebSocketCloseStatus? Status { get; set; }

    /// <summary>
    /// Base constructor
    /// </summary>
    /// <param name="reason"></param>
    /// <param name="status"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public WebSocketCloseEventArgs(string reason, WebSocketCloseStatus? status)
    {
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
        Status = status;
    }
}
