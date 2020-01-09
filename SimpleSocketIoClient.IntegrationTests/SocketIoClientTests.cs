using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using H.WebSockets.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimpleSocketIoClient.IntegrationTests
{
    [TestClass]
    public class SocketIoClientTests
    {
        private const string LocalCharServerUrl = "ws://localhost:1465/";

        private static async Task BaseTest(Func<SocketIoClient, CancellationToken, Task> func, params string[] additionalEvents)
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await using var client = new SocketIoClient();

            client.Connected += (sender, args) => Console.WriteLine($"Connected: {args.Namespace}");
            client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
            client.AfterEvent += (sender, args) => Console.WriteLine($"AfterEvent: Namespace: {args.Namespace}, Value: {args.Value}, IsHandled: {args.IsHandled}");
            client.AfterHandledEvent += (sender, args) => Console.WriteLine($"AfterHandledEvent: Namespace: {args.Namespace}, Value: {args.Value}");
            client.AfterUnhandledEvent += (sender, args) => Console.WriteLine($"AfterUnhandledEvent: Namespace: {args.Namespace}, Value: {args.Value}");
            client.AfterError += (sender, args) => Console.WriteLine($"AfterError: Namespace: {args.Namespace}, Value: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");

            var results = await client.WaitAllEventsAsync(async cancellationToken =>
            {
                await func(client, cancellationToken);
            }, cancellationTokenSource.Token, new [] { nameof(client.Connected), nameof(client.Disconnected) }.Concat(additionalEvents).ToArray());

            Console.WriteLine();
            Console.WriteLine($"WebSocket State: {client.EngineIoClient.WebSocketClient.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.EngineIoClient.WebSocketClient.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.EngineIoClient.WebSocketClient.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                Assert.IsNotNull(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
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
            client.EngineIoClient.ExceptionOccurred += (sender, args) => Console.WriteLine($"EngineIoClient.ExceptionOccurred: {args.Value}");
            client.EngineIoClient.MessageReceived += (sender, args) => Console.WriteLine($"EngineIoClient.MessageReceived: {args.Value}");
            client.EngineIoClient.NoopReceived += (sender, args) => Console.WriteLine($"EngineIoClient.NoopReceived: {args.Value}");
            client.EngineIoClient.PingReceived += (sender, args) => Console.WriteLine($"EngineIoClient.PingReceived: {args.Value}");
            client.EngineIoClient.PongReceived += (sender, args) => Console.WriteLine($"EngineIoClient.PongReceived: {args.Value}");
            client.EngineIoClient.PingSent += (sender, args) => Console.WriteLine($"EngineIoClient.PingSent: {args.Value}");

            client.EngineIoClient.WebSocketClient.Connected += (sender, args) => Console.WriteLine("WebSocketClient.Connected");
            client.EngineIoClient.WebSocketClient.Disconnected += (sender, args) => Console.WriteLine($"WebSocketClient.Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
            client.EngineIoClient.WebSocketClient.TextReceived += (sender, args) => Console.WriteLine($"WebSocketClient.TextReceived: {args.Value}");
            client.EngineIoClient.WebSocketClient.ExceptionOccurred += (sender, args) => Console.WriteLine($"WebSocketClient.ExceptionOccurred: {args.Value}");
            client.EngineIoClient.WebSocketClient.BytesReceived += (sender, args) => Console.WriteLine($"WebSocketClient.BytesReceived: {args.Value.Count}");
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
        public async Task Test()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () => await ConnectToChatBaseTest("ws://test.ubetia.net:3005/"));
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
