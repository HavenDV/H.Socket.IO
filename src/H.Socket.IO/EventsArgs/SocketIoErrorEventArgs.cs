using System;

namespace H.Socket.IO.EventsArgs
{
    /// <summary>
    /// Arguments used in <see cref="SocketIoClient.ErrorReceived"/> event
    /// </summary>
    public class SocketIoErrorEventArgs : EventArgs
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
        /// Base constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="namespace"></param>
        public SocketIoErrorEventArgs(string value, string @namespace)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }
    }
}