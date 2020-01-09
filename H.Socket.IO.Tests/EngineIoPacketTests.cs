using H.Engine.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Socket.IO.Tests
{
    [TestClass]
    public class EngineIoPacketTests
    {
        private static EngineIoPacket BaseDecodeTest(string message, EngineIoPacket expectedPacket)
        {
            var packet = EngineIoPacket.Decode(message);

            Assert.AreEqual(expectedPacket.Prefix, packet.Prefix, $"{nameof(BaseDecodeTest)}.{nameof(packet.Prefix)}: {message}");
            Assert.AreEqual(expectedPacket.Value, packet.Value, $"{nameof(BaseDecodeTest)}.{nameof(packet.Value)}: {message}");

            return packet;
        }

        private static string BaseEncodeTest(EngineIoPacket packet, string expectedMessage)
        {
            var message = packet.Encode();

            Assert.AreEqual(expectedMessage, message, $"{nameof(BaseEncodeTest)}.{nameof(message)}: {message}");

            return message;
        }

        private static void BaseEncodeDecodeTest(EngineIoPacket packet)
        {
            var message = packet.Encode();

            BaseDecodeTest(message, packet);
        }

        private static void BaseDecodeEncodeTest(string message)
        {
            var packet = EngineIoPacket.Decode(message);

            BaseEncodeTest(packet, message);
        }

        private static void BaseTest(string message, EngineIoPacket packet)
        {
            BaseDecodeTest(message, packet);
            BaseEncodeTest(packet, message);
            BaseEncodeDecodeTest(packet);
            BaseDecodeEncodeTest(message);
        }

        [TestMethod]
        public void Test()
        {
            BaseTest("0{\"sid\":\"lvRP3AYFhuQr-7iVB5T9\",\"upgrades\":[],\"pingInterval\":25000,\"pingTimeout\":60000}", new EngineIoPacket(EngineIoPacket.OpenPrefix, "{\"sid\":\"lvRP3AYFhuQr-7iVB5T9\",\"upgrades\":[],\"pingInterval\":25000,\"pingTimeout\":60000}"));
            BaseTest("1", new EngineIoPacket(EngineIoPacket.ClosePrefix));
            BaseTest("40", new EngineIoPacket(EngineIoPacket.MessagePrefix, "0"));
            BaseTest("42[\"login\",{\"numUsers\":5}]", new EngineIoPacket(EngineIoPacket.MessagePrefix, "2[\"login\",{\"numUsers\":5}]"));
            BaseTest("42/my,[\"message\",{\"message\":\"hello\"}]", new EngineIoPacket(EngineIoPacket.MessagePrefix, "2/my,[\"message\",{\"message\":\"hello\"}]"));
            BaseTest("44\"Authentication error\"", new EngineIoPacket(EngineIoPacket.MessagePrefix, "4\"Authentication error\""));
        }
    }
}
