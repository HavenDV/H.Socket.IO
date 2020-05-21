using System;
using System.Linq;

namespace H.Socket.IO
{
    internal class SocketIoPacket
    {
        #region Constants

        public const string ConnectPrefix = "0";
        public const string DisconnectPrefix = "1";
        public const string EventPrefix = "2";
        //public const string AckPrefix = "3";
        public const string ErrorPrefix = "4";
        //public const string BinaryEventPrefix = "5";
        //public const string BinaryAckPrefix = "6";

        public const string DefaultNamespace = "/";

        #endregion

        #region Properties

        public string Prefix { get; set; }
        public string Namespace { get; set; }
        public string Value { get; set; }

        #endregion

        #region Constructors

        public SocketIoPacket(string prefix, string? value = null, string? @namespace = null)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Namespace = @namespace ?? DefaultNamespace;
            Value = value ?? string.Empty;
        }

        #endregion

        #region Public methods

        public static SocketIoPacket Decode(string message)
        {
            var prefix = message.Substring(0, 1);
            if (message.ElementAtOrDefault(1) == '/')
            {
                var index = message.IndexOf(',');
                var @namespace = index >= 0
                    ? message.Substring(1, index - 1)
                    : message.Substring(1);
                var value = index >= 0
                    ? message.Substring(index + 1)
                    : string.Empty;

                return new SocketIoPacket(prefix, value, @namespace);
            }

            return new SocketIoPacket(prefix, message.Substring(1));
        }

        public string Encode()
        {
            var namespaceBody = Namespace == DefaultNamespace
                ? string.Empty
                : $"/{Namespace.TrimStart('/')}";
            namespaceBody += !string.IsNullOrWhiteSpace(namespaceBody) && !string.IsNullOrWhiteSpace(Value)
                ? ","
                : string.Empty;

            return $"{Prefix}{namespaceBody}{Value}";
        }

        #endregion
    }
}
