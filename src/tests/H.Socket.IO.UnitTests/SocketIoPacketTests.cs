namespace H.Socket.IO.Tests;

[TestClass]
public class SocketIoPacketTests
{
    private static SocketIoPacket BaseDecodeTest(string message, SocketIoPacket expectedPacket)
    {
        var packet = SocketIoPacket.Decode(message);

        packet.Prefix.Should().Be(expectedPacket.Prefix);
        packet.Namespace.Should().Be(expectedPacket.Namespace);
        packet.Value.Should().Be(expectedPacket.Value);

        return packet;
    }

    private static string BaseEncodeTest(SocketIoPacket packet, string expectedMessage)
    {
        var message = packet.Encode();

        message.Should().Be(expectedMessage);

        return message;
    }

    private static void BaseEncodeDecodeTest(SocketIoPacket packet)
    {
        var message = packet.Encode();

        BaseDecodeTest(message, packet);
    }

    private static void BaseDecodeEncodeTest(string message)
    {
        var packet = SocketIoPacket.Decode(message);

        BaseEncodeTest(packet, message);
    }

    private static void BaseTest(string message, SocketIoPacket packet)
    {
        BaseDecodeTest(message, packet);
        BaseEncodeTest(packet, message);
        BaseEncodeDecodeTest(packet);
        BaseDecodeEncodeTest(message);
    }

    [TestMethod]
    public void Test()
    {
        BaseTest("0", new SocketIoPacket(SocketIoPacket.ConnectPrefix));
        BaseTest("0/my", new SocketIoPacket(SocketIoPacket.ConnectPrefix, @namespace: "/my"));
        BaseTest("1", new SocketIoPacket(SocketIoPacket.DisconnectPrefix));
        BaseTest("2[\"login\",{\"numUsers\":5}]", new SocketIoPacket(SocketIoPacket.EventPrefix, "[\"login\",{\"numUsers\":5}]", "/"));
        BaseTest("2/my,[\"message\",{\"message\":\"hello\"}]", new SocketIoPacket(SocketIoPacket.EventPrefix, "[\"message\",{\"message\":\"hello\"}]", "/my"));
        BaseTest("4\"Authentication error\"", new SocketIoPacket(SocketIoPacket.ErrorPrefix, "\"Authentication error\""));
    }
}
