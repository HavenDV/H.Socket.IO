using System;
using System.Threading;
using System.Threading.Tasks;
using H.WebSockets.Utilities;
using FluentAssertions;

namespace H.Engine.IO.IntegrationTests
{
    internal class BaseTests
    {
        public static async Task ConnectToChatBaseTestAsync(string url, CancellationToken cancellationToken = default)
        {
#if NET5_0
            await using var client = new EngineIoClient("socket.io");
#else
            using var client = new EngineIoClient("socket.io");
#endif

            client.MessageReceived += (_, args) => Console.WriteLine($"MessageReceived: {args.Value}");
            client.ExceptionOccurred += (_, args) => Console.WriteLine($"ExceptionOccurred: {args.Value}");
            client.Opened += (_, args) => Console.WriteLine($"Opened: {args.Value}");
            client.Closed += (_, args) => Console.WriteLine($"Closed. Reason: {args.Reason}, Status: {args.Status:G}");

            var results = await client.WaitAllEventsAsync<EventArgs>(async () =>
            {
                Console.WriteLine("# Before OpenAsync");

                await client.OpenAsync(new Uri(url), cancellationToken);

                Console.WriteLine("# Before Delay");

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                Console.WriteLine("# Before CloseAsync");

                await client.CloseAsync(cancellationToken);

                Console.WriteLine("# After CloseAsync");
            }, cancellationToken, nameof(client.Opened), nameof(client.Closed));

            Console.WriteLine();
            Console.WriteLine($"WebSocket State: {client.WebSocketClient.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.WebSocketClient.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.WebSocketClient.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                pair.Value.Should().NotBeNull(because: $"Client event(\"{pair.Key}\") did not happen");
            }
        }
    }
}
