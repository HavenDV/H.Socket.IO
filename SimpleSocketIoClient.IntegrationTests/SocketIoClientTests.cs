using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.IntegrationTests
{
    [TestClass]
    public class SocketIoClientTests
    {
        private const string LocalCharServerUrl = "ws://localhost:1465/";

        private static async Task BaseTest(Func<SocketIoClient, CancellationToken, Task> func, params string[] additionalEvents)
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

#if NETSTANDARD2_1 || NETCOREAPP3_0 || NETCOREAPP3_1
            await using var client = new SocketIoClient();
#else
            using var client = new SocketIoClient();
#endif

            client.Connected += (sender, args) => Console.WriteLine($"Connected: {args.Namespace}");
            client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
            client.AfterEvent += (sender, args) => Console.WriteLine($"AfterEvent: Namespace: {args.Namespace}, Value: {args.Value}");
            client.AfterUnhandledEvent += (sender, args) => Console.WriteLine($"AfterUnhandledEvent: Namespace: {args.Namespace}, Value: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");

            var results = await client.WaitEventsAsync(async cancellationToken =>
            {
                await func(client, cancellationToken);
            }, cancellationTokenSource.Token, new [] { nameof(client.Connected), nameof(client.Disconnected) }.Concat(additionalEvents).ToArray());

            Console.WriteLine();
            Console.WriteLine($"WebSocket State: {client.EngineIoClient.WebSocketClient.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.EngineIoClient.WebSocketClient.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.EngineIoClient.WebSocketClient.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                Assert.IsTrue(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
            }
        }

        private static async Task BaseLocalTest(Func<SocketIoClient, CancellationToken, Task> func, params string[] additionalEvents)
        {
            try
            {
                await BaseTest(func, additionalEvents);
            }
            catch (WebSocketException exception)
            {
                Console.WriteLine(exception);
            }
        }

        public static void EnableDebug(SocketIoClient client)
        {
            client.EngineIoClient.Opened += (sender, args) => Console.WriteLine("EngineIoClient.Opened");
            client.EngineIoClient.Closed += (sender, args) => Console.WriteLine($"EngineIoClient.Closed. Reason: {args.Reason}, Status: {args.Status:G}");
            client.EngineIoClient.Upgraded += (sender, args) => Console.WriteLine($"EngineIoClient.Upgraded: {args.Value}");
            client.EngineIoClient.AfterException += (sender, args) => Console.WriteLine($"EngineIoClient.AfterException: {args.Value}");
            client.EngineIoClient.AfterMessage += (sender, args) => Console.WriteLine($"EngineIoClient.AfterMessage: {args.Value}");
            client.EngineIoClient.AfterNoop += (sender, args) => Console.WriteLine($"EngineIoClient.AfterNoop: {args.Value}");
            client.EngineIoClient.AfterPing += (sender, args) => Console.WriteLine($"EngineIoClient.AfterPing: {args.Value}");
            client.EngineIoClient.AfterPong += (sender, args) => Console.WriteLine($"EngineIoClient.AfterPong: {args.Value}");

            client.EngineIoClient.WebSocketClient.Connected += (sender, args) => Console.WriteLine("WebSocketClient.Connected");
            client.EngineIoClient.WebSocketClient.Disconnected += (sender, args) => Console.WriteLine($"WebSocketClient.Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
            client.EngineIoClient.WebSocketClient.AfterText += (sender, args) => Console.WriteLine($"WebSocketClient.AfterText: {args.Value}");
            client.EngineIoClient.WebSocketClient.AfterException += (sender, args) => Console.WriteLine($"WebSocketClient.AfterException: {args.Value}");
            client.EngineIoClient.WebSocketClient.AfterBinary += (sender, args) => Console.WriteLine($"WebSocketClient.AfterBinary: {args.Value.Length}");
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        // ReSharper disable UnusedAutoPropertyAccessor.Local
        private class ChatMessage
        {
            public string Username { get; set; }
            public string Message { get; set; }
            public long NumUsers { get; set; }
        }

        private static async Task ConnectToChatBaseTest(string url)
        {
            await BaseTest(async (client, cancellationToken) =>
            {
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

                await client.ConnectAsync(new Uri(url), cancellationToken);

                await client.Emit("add user", "C# SimpleSocketIoClient Test User", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("typing", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("new message", "hello", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("stop typing", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.DisconnectAsync(cancellationToken);
            }, nameof(SocketIoClient.AfterEvent));
        }

        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            await ConnectToChatBaseTest("wss://socket-io-chat.now.sh/");
        }

        [TestMethod]
        [Ignore]
        public async Task ConnectToLocalChatServerTest()
        {
            try
            {
                await ConnectToChatBaseTest(LocalCharServerUrl);
            }
            catch (WebSocketException exception)
            {
                Console.WriteLine(exception);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task ConnectToLocalChatServerNamespaceTest1()
        {
            await BaseLocalTest(async (client, cancellationToken) =>
            {
                EnableDebug(client);

                client.DefaultNamespace = "my";

                await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("message", "hello", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.DisconnectAsync(cancellationToken);
            }, nameof(SocketIoClient.AfterEvent), nameof(SocketIoClient.AfterUnhandledEvent));
        }

        [TestMethod]
        [Ignore]
        public async Task ConnectToLocalChatServerNamespaceTest2()
        {
            await BaseLocalTest(async (client, cancellationToken) =>
            {
                EnableDebug(client);

                await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken, "my");

                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("message", "hello", "my", cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.DisconnectAsync(cancellationToken);
            }, nameof(SocketIoClient.AfterEvent), nameof(SocketIoClient.AfterUnhandledEvent));
        }

        [TestMethod]
        [Ignore]
        public async Task ConnectToLocalChatServerDebugTest()
        {
            await BaseLocalTest(async (client, cancellationToken) =>
            {
                EnableDebug(client);

                await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken);

                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("add user", "C# SimpleSocketIoClient Test User", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("typing", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("new message", "hello", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                await client.Emit("stop typing", cancellationToken: cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.DisconnectAsync(cancellationToken);
            });
        }
    }
}
