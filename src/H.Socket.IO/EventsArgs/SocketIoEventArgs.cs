using System;

namespace H.Socket.IO.EventsArgs
{
    /// <summary>
    /// Arguments used in any event
    /// </summary>
    public class SocketIoEventArgs : EventArgs
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
        protected SocketIoEventArgs(string value, string @namespace)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Namespace = @namespace ?? throw new ArgumentNullException(nameof(@namespace));
        }
    }
}