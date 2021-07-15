using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace H.Engine.IO.Tests
{
    [TestClass]
    public class EngineIoPacketTests
    {
        private static EngineIoPacket BaseDecodeTest(string message, EngineIoPacket expectedPacket)
        {
            var packet = EngineIoPacket.Decode(message);

            packet.Prefix.Should().Be(expectedPacket.Prefix);
            packet.Value.Should().Be(expectedPacket.Value);

            return packet;
        }

        private static string BaseEncodeTest(EngineIoPacket packet, string expectedMessage)
        {
            var message = packet.Encode();

            message.Should().Be(expectedMessage);

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
