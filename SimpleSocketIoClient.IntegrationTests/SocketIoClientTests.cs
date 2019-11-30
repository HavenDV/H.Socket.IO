using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.IntegrationTests
{
    public class ChatMessage
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public long NumUsers { get; set; }
    }

    [TestClass]
    public class SocketIoClientTests
    {
        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

#if NETCOREAPP3_0
            await using var client = new SocketIoClient();
#else
            using var client = new SocketIoClient();
#endif

            client.Connected += (sender, args) => Console.WriteLine("Connected");
            client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
            client.AfterEvent += (sender, args) => Console.WriteLine($"AfterEvent: {args.Value}");
            client.AfterUnhandledEvent += (sender, args) => Console.WriteLine($"AfterUnhandledEvent: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");

            client.On<ChatMessage>("login", message =>
            {
                Console.WriteLine($"You are logged in. Total number of users: {message.NumUsers}");
            });
            client.On<ChatMessage>("user joined", message =>
            {
                Console.WriteLine($"User joined: {message.Username}. Total number of users: {message.NumUsers}");
            });
            client.On<ChatMessage>("user left", message =>
            {
                Console.WriteLine($"User left: {message.Username}. Total number of users: {message.NumUsers}");
            });
            client.On<ChatMessage>("typing", message =>
            {
                Console.WriteLine($"User typing: {message.Username}");
            });
            client.On<ChatMessage>("stop typing", message =>
            {
                Console.WriteLine($"User stop typing: {message.Username}");
            });
            client.On<ChatMessage>("new message", message =>
            {
                Console.WriteLine($"New message from user \"{message.Username}\": {message.Message}");
            });

            var events = new[] { nameof(client.Connected), nameof(client.Disconnected), nameof(client.AfterEvent) };
            var results = await client.WaitEventsAsync(async cancellationToken =>
            {
                await client.ConnectAsync(new Uri("wss://socket-io-chat.now.sh/"), cancellationToken);

                await client.Emit("add user", "C# SimpleSocketIoClient Test User", cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);

                await client.Emit("typing", cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);

                await client.Emit("new message", "hello", cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);

                await client.Emit("stop typing", cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.DisconnectAsync(cancellationToken);
            }, cancellationTokenSource.Token, events);

            Console.WriteLine($"WebSocket State: {client.EngineIoClient.WebSocketClient.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.EngineIoClient.WebSocketClient.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.EngineIoClient.WebSocketClient.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                Assert.IsTrue(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
            }
        }
    }
}
