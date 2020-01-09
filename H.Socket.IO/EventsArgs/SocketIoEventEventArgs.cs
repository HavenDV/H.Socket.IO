using System;

namespace H.Socket.IO.EventsArgs
{
    /// <summary>
    /// Arguments used in <see cref="SocketIoClient.AfterEvent"/> event
    /// </summary>
    public class SocketIoEventEventArgs : EventArgs
    {
        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Namespace
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// IsHandled
        /// </summary>
        public bool IsHandled { get; set; }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="namespace"></param>
        /// <param name="isHandled"></param>
        public SocketIoEventEventArgs(string value, string @namespace, bool isHandled)
        {
            Value = value;
            Namespace = @namespace;
            IsHandled = isHandled;
        }
    }
}