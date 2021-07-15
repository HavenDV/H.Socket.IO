using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace H.Engine.IO.Tests
{
    [TestClass]
    public class EngineIoClientTests
    {
        private static void ToWebSocketUriBaseTest(string expected, string url, string framework = "engine.io")
        {
            EngineIoClient.ToWebSocketUri(new Uri(url), framework)
                .Should().Be(new Uri(expected));
        }

        [TestMethod]
        public void ToWebSocketUriTest()
        {
            ToWebSocketUriBaseTest(
                "wss://socketio-chat-h9jt.herokuapp.com/engine.io/?EIO=3&transport=websocket&", 
                "wss://socketio-chat-h9jt.herokuapp.com/");
            ToWebSocketUriBaseTest(
                "wss://socketio-chat-h9jt.herokuapp.com/engine.io/?EIO=3&transport=websocket&",
                "wss://socketio-chat-h9jt.herokuapp.com");
            ToWebSocketUriBaseTest(
                "wss://socketio-chat-h9jt.herokuapp.com/socket.io/?EIO=3&transport=websocket&",
                "wss://socketio-chat-h9jt.herokuapp.com/",
                "socket.io");
            ToWebSocketUriBaseTest(
                "wss://socketio-chat-h9jt.herokuapp.com/socket.io/?EIO=3&transport=websocket&",
                "wss://socketio-chat-h9jt.herokuapp.com",
                "socket.io");

            ToWebSocketUriBaseTest(
                "wss://abc.com/path/socket.io/?EIO=3&transport=websocket&",
                "https://abc.com/path",
                "socket.io");
            ToWebSocketUriBaseTest(
                "wss://abc.com/path/socket.io/?EIO=3&transport=websocket&",
                "https://abc.com/path/",
                "socket.io");

            ToWebSocketUriBaseTest(
                "wss://abc.com/very/long/path/socket.io/?EIO=3&transport=websocket&",
                "https://abc.com/very/long/path",
                "socket.io");
            ToWebSocketUriBaseTest(
                "wss://abc.com/very/long/path/socket.io/?EIO=3&transport=websocket&",
                "https://abc.com/very/long/path/",
                "socket.io");

            ToWebSocketUriBaseTest(
                "wss://abc.com/path/engine.io/?EIO=3&transport=websocket&arg=value",
                "https://abc.com/path/?arg=value");
            ToWebSocketUriBaseTest(
                "wss://abc.com/path/engine.io/?EIO=3&transport=websocket&arg",
                "https://abc.com/path?arg");
            ToWebSocketUriBaseTest(
                "wss://abc.com/engine.io/?EIO=3&transport=websocket&arg",
                "https://abc.com?arg");
            ToWebSocketUriBaseTest(
                "wss://abc.com/engine.io/?EIO=3&transport=websocket&arg",
                "https://abc.com/?arg");
        }
    }
}
