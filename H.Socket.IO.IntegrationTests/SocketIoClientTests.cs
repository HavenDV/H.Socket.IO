using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using H.Socket.IO.EventsArgs;
using H.WebSockets.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Socket.IO.IntegrationTests
{
    [TestClass]
    public class SocketIoClientTests
    {
        public const string LocalCharServerUrl = "ws://localhost:1465/";

        private static async Task BaseTestAsync(
            Func<SocketIoClient, CancellationToken, Task> func,
            CancellationToken cancellationToken = default, 
            params string[] additionalEvents)
        {
            await using var client = new SocketIoClient();

            client.Connected += (sender, args) => Console.WriteLine($"Connected: {args.Namespace}");
            client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");
            client.EventReceived += (sender, args) => Console.WriteLine($"EventReceived: Namespace: {args.Namespace}, Value: {args.Value}, IsHandled: {args.IsHandled}");
            client.HandledEventReceived += (sender, args) => Console.WriteLine($"HandledEventReceived: Namespace: {args.Namespace}, Value: {args.Value}");
            client.UnhandledEventReceived += (sender, args) => Console.WriteLine($"UnhandledEventReceived: Namespace: {args.Namespace}, Value: {args.Value}");
            client.ErrorReceived += (sender, args) => Console.WriteLine($"ErrorReceived: Namespace: {args.Namespace}, Value: {args.Value}");
            client.ExceptionOccurred += (sender, args) => Console.WriteLine($"ExceptionOccurred: {args.Value}");

            var results = await client.WaitAllEventsAsync<EventArgs>(
                async token =>
                {
                    await func(client, token);
                }, 
                cancellationToken, 
                new [] { nameof(client.Connected), nameof(client.Disconnected) }
                    .Concat(additionalEvents)
                    .ToArray());

            Console.WriteLine();
            Console.WriteLine($"WebSocket State: {client.EngineIoClient.WebSocketClient.Socket.State}");
            Console.WriteLine($"WebSocket CloseStatus: {client.EngineIoClient.WebSocketClient.Socket.CloseStatus}");
            Console.WriteLine($"WebSocket CloseStatusDescription: {client.EngineIoClient.WebSocketClient.Socket.CloseStatusDescription}");

            foreach (var pair in results)
            {
                Assert.IsNotNull(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
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

        private static async Task ConnectToChatBaseTestAsync(string url, CancellationToken cancellationToken = default)
        {
            await BaseTestAsync(async (client, token) =>
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

                await client.ConnectAsync(new Uri(url), token);

                var args = await client.WaitEventOrErrorAsync(async waitToken =>
                {
                    await client.Emit("add user", "C# H.Socket.IO Test User", cancellationToken: waitToken);
                }, token);
                switch (args)
                {
                    case SocketIoEventEventArgs eventArgs:
                        Console.WriteLine($"WaitEventOrErrorAsync: Event received: {eventArgs}");
                        break;

                    case SocketIoErrorEventArgs errorArgs:
                        Assert.Fail($"Error received after add user: {errorArgs}");
                        break;

                    case null:
                        Assert.Fail("No event received after add user");
                        break;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(200), token);

                await client.Emit("typing", cancellationToken: token);
                await Task.Delay(TimeSpan.FromMilliseconds(200), token);
                await client.Emit("new message", "hello", cancellationToken: token);
                await Task.Delay(TimeSpan.FromMilliseconds(200), token);
                await client.Emit("stop typing", cancellationToken: token);
                await Task.Delay(TimeSpan.FromSeconds(2), token);

                await client.DisconnectAsync(token);
            }, cancellationToken, nameof(SocketIoClient.EventReceived));
        }

        public static async Task<Uri> GetRedirectedUrlAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = false,
            }, true);
            using var response = await client.GetAsync(uri, cancellationToken);

            return (int)response.StatusCode == 308
                ? new Uri(response.Headers.GetValues("Location").First())
                : response.RequestMessage.RequestUri;
        }

        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            var uri = await GetRedirectedUrlAsync(new Uri("https://socket-io-chat.now.sh/"), tokenSource.Token);

            await ConnectToChatBaseTestAsync($"wss://{uri.Host}/", tokenSource.Token);
        }

        [TestMethod]
        [Ignore]
        public async Task ConnectToLocalChatServerTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await ConnectToChatBaseTestAsync(LocalCharServerUrl, tokenSource.Token);
        }

        [TestMethod]
        [Ignore]
        public async Task ConnectToLocalChatServerNamespaceTest1()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await BaseTestAsync(
                async (client, cancellationToken) =>
                {
                    EnableDebug(client);

                    client.DefaultNamespace = "my";

                    await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken);

                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    await client.Emit("message", "hello", cancellationToken: cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                    await client.DisconnectAsync(cancellationToken);
                }, 
                tokenSource.Token, 
                nameof(SocketIoClient.EventReceived), 
                nameof(SocketIoClient.UnhandledEventReceived));
        }

        [TestMethod]
        [Ignore]
        public async Task ConnectToLocalChatServerNamespaceTest2()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await BaseTestAsync(
                async (client, cancellationToken) =>
                {
                    EnableDebug(client);

                    await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken, "my");

                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    await client.Emit("message", "hello", "my", cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                    await client.DisconnectAsync(cancellationToken);
                }, 
                tokenSource.Token,
                nameof(SocketIoClient.EventReceived), 
                nameof(SocketIoClient.UnhandledEventReceived));
        }

        [TestMethod]
        [Ignore]
        public async Task ConnectToLocalChatServerDebugTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await BaseTestAsync(
                async (client, cancellationToken) =>
                {
                    EnableDebug(client);

                    await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken);

                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    await client.Emit("add user", "C# H.Socket.IO Test User", cancellationToken: cancellationToken);
                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    await client.Emit("typing", cancellationToken: cancellationToken);
                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    await client.Emit("new message", "hello", cancellationToken: cancellationToken);
                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    await client.Emit("stop typing", cancellationToken: cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                    await client.DisconnectAsync(cancellationToken);
                }, 
                tokenSource.Token);
        }
    }
}
