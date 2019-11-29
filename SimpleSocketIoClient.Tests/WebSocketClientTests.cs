using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.Tests
{
    [TestClass]
    public class WebSocketClientTests
    {
        [TestMethod]
        public async Task DoubleConnectToWebSocketOrgTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
#if NETCOREAPP3_0
            await using var client = new WebSocketClient();
#else
            using var client = new WebSocketClient();
#endif

            client.AfterText += (sender, args) => Console.WriteLine($"AfterText: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");
            client.AfterBinary += (sender, args) => Console.WriteLine($"AfterBinary: {args.Value?.Length}");
            client.Connected += (sender, args) => Console.WriteLine("Connected");
            client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Value.Reason}, Status: {args.Value.Status:G}");

            var events = new[] { nameof(client.Connected), nameof(client.Disconnected) };
            var results = await client.WaitEventsAsync(async cancellationToken =>
            {
                Assert.IsFalse(client.IsConnected, "client.IsConnected");

                await client.ConnectAsync(new Uri("ws://echo.websocket.org"), cancellationToken);

                Assert.IsTrue(client.IsConnected, "client.IsConnected");

                await client.SendTextAsync("Test", cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.DisconnectAsync(cancellationToken);

                Assert.IsFalse(client.IsConnected, "client.IsConnected");
            }, cancellationTokenSource.Token, events);
            foreach (var (result, eventName) in results.Zip(events, (a, b) => (a, b)))
            {
                Assert.IsTrue(result, $"Client event(\"{eventName}\") did not happen");
            }

            results = await client.WaitEventsAsync(async cancellationToken =>
            {
                await client.ConnectAsync(new Uri("ws://echo.websocket.org"), cancellationToken);

                Assert.IsTrue(client.IsConnected, "client.IsConnected");

                await client.SendTextAsync("Test", cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.DisconnectAsync(cancellationToken);

                Assert.IsFalse(client.IsConnected, "client.IsConnected");
            }, cancellationTokenSource.Token, events);

            Console.WriteLine($"WebSocket State: {client.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.Socket.CloseStatusDescription}");

            foreach (var (result, eventName) in results.Zip(events, (a, b) => (a, b)))
            {
                Assert.IsTrue(result, $"Client event(\"{eventName}\") did not happen");
            }
        }
    }
}
