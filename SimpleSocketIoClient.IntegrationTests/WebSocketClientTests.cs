using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSocketIoClient.Utilities;
using SimpleSocketIoClient.WebSocket;

namespace SimpleSocketIoClient.IntegrationTests
{
    [TestClass]
    public class WebSocketClientTests
    {
        [TestMethod]
        public async Task DoubleConnectToWebSocketOrgTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await using var client = new WebSocketClient();

            client.AfterText += (sender, args) => Console.WriteLine($"AfterText: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");
            client.AfterBinary += (sender, args) => Console.WriteLine($"AfterBinary: {args.Value?.Length}");
            client.Connected += (sender, args) => Console.WriteLine("Connected");
            client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");

            var events = new[] { nameof(client.Connected), nameof(client.Disconnected) };
            var results = await client.WaitAllEventsAsync(async cancellationToken =>
            {
                Console.WriteLine("# Before ConnectAsync");

                Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));

                await client.ConnectAsync(new Uri("ws://echo.websocket.org"), cancellationToken);

                Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));

                Console.WriteLine("# Before SendTextAsync");

                await client.SendTextAsync("Test", cancellationToken);

                Console.WriteLine("# Before Delay");

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                Console.WriteLine("# Before DisconnectAsync");

                await client.DisconnectAsync(cancellationToken);

                Console.WriteLine("# After DisconnectAsync");

                Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));
            }, cancellationTokenSource.Token, events);

            Console.WriteLine();
            Console.WriteLine($"WebSocket State: {client.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                Assert.IsNotNull(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
            }

            results = await client.WaitAllEventsAsync(async cancellationToken =>
            {
                Console.WriteLine("# Before ConnectAsync");

                await client.ConnectAsync(new Uri("ws://echo.websocket.org"), cancellationToken);

                Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));

                Console.WriteLine("# Before SendTextAsync");

                await client.SendTextAsync("Test", cancellationToken);

                Console.WriteLine("# Before Delay");

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                Console.WriteLine("# Before DisconnectAsync");

                await client.DisconnectAsync(cancellationToken);

                Console.WriteLine("# After DisconnectAsync");

                Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));
            }, cancellationTokenSource.Token, events);

            Console.WriteLine();
            Console.WriteLine($"WebSocket State: {client.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                Assert.IsNotNull(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
            }
        }
    }
}
