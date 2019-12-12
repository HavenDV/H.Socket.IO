using System;
using System.Net.WebSockets;

namespace SimpleSocketIoClient.EventsArgs
{
    /// <summary>
    /// Arguments used in <see cref="WebSocket"/> close event
    /// </summary>
    public class WebSocketCloseEventArgs : EventArgs
    {
        /// <summary>
        /// Reason of disconnect
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Status of WebSocket
        /// </summary>
        public WebSocketCloseStatus? Status { get; set; }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="reason"></param>
        /// <param name="status"></param>
        public WebSocketCloseEventArgs(string? reason, WebSocketCloseStatus? status)
        {
            Reason = reason;
            Status = status;
        }
    }
}
