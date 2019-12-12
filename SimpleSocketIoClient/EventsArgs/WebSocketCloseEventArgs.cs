using System;

namespace SimpleSocketIoClient.EventsArgs
{
    /// <summary>
    /// Arguments used in AfterEvent event
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
        /// Base constructor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="namespace"></param>
        public SocketIoEventEventArgs(string value, string @namespace)
        {
            Value = value;
            Namespace = @namespace;
        }
    }
}
