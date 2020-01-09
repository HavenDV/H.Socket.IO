using System;

namespace H.EngineIO
{
    internal class EngineIoPacket
    {
        #region Constants

        public const string OpenPrefix = "0";
        public const string ClosePrefix = "1";
        public const string PingPrefix = "2";
        public const string PongPrefix = "3";
        public const string MessagePrefix = "4";
        public const string UpgradePrefix = "5";
        public const string NoopPrefix = "6";

        #endregion

        #region Properties

        public string Prefix { get; set; }
        public string Value { get; set; }

        #endregion

        #region Constructors

        public EngineIoPacket(string prefix, string? value = null)
        {
            Prefix = prefix ?? throw new ArgumentNullException(nameof(prefix));
            Value = value ?? string.Empty;
        }

        #endregion

        #region Public methods

        public static EngineIoPacket Decode(string message)
        {
            var prefix = message.Substring(0, 1);
            var value = message.Substring(1);

            return new EngineIoPacket(prefix, value);
        }

        public string Encode()
        {
            return $"{Prefix}{Value}";
        }

        #endregion
    }
}
