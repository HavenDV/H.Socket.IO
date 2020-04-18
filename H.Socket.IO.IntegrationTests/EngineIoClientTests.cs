using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using H.Engine.IO;
using H.WebSockets.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Socket.IO.IntegrationTests
{
    [TestClass]
    public class EngineIoClientTests
    {
        private static async Task ConnectToChatBaseTest(string url)
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await using var client = new EngineIoClient("socket.io");

            client.MessageReceived += (sender, args) => Console.WriteLine($"MessageReceived: {args.Value}");
            client.ExceptionOccurred += (sender, args) => Console.WriteLine($"ExceptionOccurred: {args.Value}");
            client.Opened += (sender, args) => Console.WriteLine($"Opened: {args.Value}");
            client.Closed += (sender, args) => Console.WriteLine($"Closed. Reason: {args.Reason}, Status: {args.Status:G}");

            var results = await client.WaitAllEventsAsync<EventArgs>(async cancellationToken =>
            {
                Console.WriteLine("# Before OpenAsync");

                await client.OpenAsync(new Uri(url), 10);

                Console.WriteLine("# Before Delay");

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                Console.WriteLine("# Before CloseAsync");

                await client.CloseAsync(cancellationToken);

                Console.WriteLine("# After CloseAsync");
            }, cancellationTokenSource.Token, nameof(client.Opened), nameof(client.Closed));

            Console.WriteLine();
            Console.WriteLine($"WebSocket State: {client.WebSocketClient.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.WebSocketClient.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.WebSocketClient.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                Assert.IsNotNull(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
            }
        }

        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            var uri = await SocketIoClientTests.GetRedirectedUrlAsync(new Uri("https://socket-io-chat.now.sh/"));

            await ConnectToChatBaseTest($"wss://{uri.Host}/");
        }

        [TestMethod]
        public async Task ConnectToLocalChatServerTest()
        {
            try
            {
                await ConnectToChatBaseTest("ws://localhost:1465/");
            }
            catch (WebSocketException exception)
            {
                Console.WriteLine(exception);
            }
        }
    }
}
