using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.IntegrationTests
{
    [TestClass]
    public class EngineIoClientTests
    {
        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

#if NETCOREAPP3_0 || NETCOREAPP3_1
            await using var client = new EngineIoClient("socket.io");
#else
            using var client = new EngineIoClient("socket.io");
#endif

            client.AfterMessage += (sender, args) => Console.WriteLine($"AfterMessage: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");
            client.Opened += (sender, args) => Console.WriteLine($"Opened: {args.Value}");
            client.Closed += (sender, args) => Console.WriteLine($"Closed. Reason: {args.Reason}, Status: {args.Status:G}");

            var events = new[] {nameof(client.Opened), nameof(client.Closed)};
            var results = await client.WaitEventsAsync(async cancellationToken =>
            {
                Console.WriteLine("# Before OpenAsync");

                await client.OpenAsync(new Uri("https://socket-io-chat.now.sh/"), 10);

                Console.WriteLine("# Before Delay");

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                Console.WriteLine("# Before CloseAsync");

                await client.CloseAsync(cancellationToken);

                Console.WriteLine("# After CloseAsync");
            }, cancellationTokenSource.Token, events);

            Console.WriteLine();
            Console.WriteLine($"WebSocket State: {client.WebSocketClient.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.WebSocketClient.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.WebSocketClient.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                Assert.IsTrue(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
            }
        }
    }
}
