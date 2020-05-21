using System;

namespace H.Engine.IO
{
    /// <summary>
    /// See https://github.com/socketio/engine.io-protocol#packet
    /// </summary>
    public class EngineIoPacket
    {
        #region Constants

        /// <summary>
        /// See https://github.com/socketio/engine.io-protocol#0-open
        /// </summary>
        public const string OpenPrefix = "0";

        /// <summary>
        /// See https://github.com/socketio/engine.io-protocol#1-close
        /// </summary>
        public const string ClosePrefix = "1";

        /// <summary>
        /// See https://github.com/socketio/engine.io-protocol#2-ping
        /// </summary>
        public const string PingPrefix = "2";

        /// <summary>
        /// See https://github.com/socketio/engine.io-protocol#3-pong
        /// </summary>
        public const string PongPrefix = "3";

        /// <summary>
        /// See https://github.com/socketio/engine.io-protocol#4-message
        /// </summary>
        public const string MessagePrefix = "4";

        /// <summary>
        /// See https://github.com/socketio/engine.io-protocol#5-upgrade
        /// </summary>
        public const string UpgradePrefix = "5";

        /// <summary>
        /// See https://github.com/socketio/engine.io-protocol#6-noop
        /// </summary>
        public const string NoopPrefix = "6";

        #endregion

        #region Properties

        /// <summary>
        /// Packet type
        /// </summary>
        public string Prefix { get; set; }

        /// <summary>
        /// Packet value
        /// </summary>
        public string Value { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="value"></param>
        public EngineIoPacket(string prefix, string? value = null)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Value = value ?? string.Empty;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static EngineIoPacket Decode(string message)
        {
            var prefix = message.Substring(0, 1);
            var value = message.Substring(1);

            return new EngineIoPacket(prefix, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string Encode()
        {
            return $"{Prefix}{Value}";
        }

        #endregion
    }
}
