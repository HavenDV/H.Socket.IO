using System;

namespace H.Socket.IO.EventsArgs
{
    /// <summary>
    /// Arguments used in <see cref="SocketIoClient.EventReceived"/> event
    /// </summary>
    public class SocketIoEventEventArgs : EventArgs
    {
        /// <summary>
        /// Value
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Namespace
        /// </summary>
        public string Namespace { get; }

        /// <summary>
        /// IsHandled
        /// </summary>
        public bool IsHandled { get; }

        /// <summary>
        /// Base constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="namespace"></param>
        /// <param name="isHandled"></param>
        public SocketIoEventEventArgs(string value, string @namespace, bool isHandled)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
            IsHandled = isHandled;
        }
    }
}