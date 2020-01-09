using System;

namespace H.SocketIO.EventsArgs
{
    /// <summary>
    /// Arguments used in <see cref="SocketIoClient.AfterError"/> event
    /// </summary>
    public class SocketIoErrorEventArgs : EventArgs
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
        /// Base constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="namespace"></param>
        public SocketIoErrorEventArgs(string value, string @namespace)
        {
            Value = value;
            Namespace = @namespace;
        }
    }
}