using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace SimpleSocketIoClient.Tests
{
    [TestClass]
    public class WebSocketClientTests
    {
        [TestMethod]
        public async Task ConnectToWebSocketOrgTest()
        {
#if NETCOREAPP3_0
            await using var client = new WebSocketClient();
#else
            using var client = new WebSocketClient();
#endif

            client.AfterText += (sender, args) => Console.WriteLine($"AfterText: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");
            client.AfterBinary += (sender, args) => Console.WriteLine($"AfterBinary: {args.Value?.Length}");
            client.Connected += (sender, args) => Console.WriteLine("Connected");
            client.Disconnected += (sender, args) => Console.WriteLine("Disconnected");

            Assert.IsFalse(client.IsConnected, "client.IsConnected");

            await client.ConnectAsync(new Uri("ws://echo.websocket.org"), 10);

            Assert.IsTrue(client.IsConnected, "client.IsConnected");

            await client.SendTextAsync("Test");

            await Task.Delay(TimeSpan.FromSeconds(2));

            await client.DisconnectAsync();

            Assert.IsFalse(client.IsConnected, "client.IsConnected");
        }

        [TestMethod]
        public async Task DoubleConnectToWebSocketOrgTest()
        {
#if NETCOREAPP3_0
            await using var client = new WebSocketClient();
#else
            using var client = new WebSocketClient();
#endif

            client.AfterText += (sender, args) => Console.WriteLine($"AfterText: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");
            client.AfterBinary += (sender, args) => Console.WriteLine($"AfterBinary: {args.Value?.Length}");
            client.Connected += (sender, args) => Console.WriteLine("Connected");
            client.Disconnected += (sender, args) => Console.WriteLine("Disconnected");

            Assert.IsFalse(client.IsConnected, "client.IsConnected");

            await client.ConnectAsync(new Uri("ws://echo.websocket.org"), 10);

            Assert.IsTrue(client.IsConnected, "client.IsConnected");

            await client.SendTextAsync("Test");

            await Task.Delay(TimeSpan.FromSeconds(2));

            await client.DisconnectAsync();

            Assert.IsFalse(client.IsConnected, "client.IsConnected");

            await client.ConnectAsync(new Uri("ws://echo.websocket.org"));

            Assert.IsTrue(client.IsConnected, "client.IsConnected");

            await client.SendTextAsync("Test");

            await Task.Delay(TimeSpan.FromSeconds(2));

            await client.DisconnectAsync();

            Assert.IsFalse(client.IsConnected, "client.IsConnected");
        }
    }
}
