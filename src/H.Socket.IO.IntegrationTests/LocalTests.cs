using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Socket.IO.IntegrationTests
{
    [TestClass]
    [TestCategory("Local")]
    public class LocalTests
    {
        private const string LocalCharServerUrl = "ws://localhost:1465/";

        [TestMethod]
        public async Task ConnectToLocalChatServerTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = tokenSource.Token;

            await BaseTests.ConnectToChatBaseTestAsync(LocalCharServerUrl, cancellationToken);
        }

        [TestMethod]
        public async Task ConnectToLocalChatServerNamespaceTest1()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = tokenSource.Token;

            await BaseTests.BaseTestAsync(
                async client =>
                {
                    BaseTests.EnableDebug(client);

                    client.DefaultNamespace = "my";

                    await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken);

                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    await client.Emit("message", "hello", cancellationToken: cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                    await client.DisconnectAsync(cancellationToken);
                },
                cancellationToken, 
                nameof(SocketIoClient.EventReceived), 
                nameof(SocketIoClient.UnhandledEventReceived));
        }

        [TestMethod]
        public async Task ConnectToLocalChatServerNamespaceTest2()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = tokenSource.Token;

            await BaseTests.BaseTestAsync(
                async client =>
                {
                    BaseTests.EnableDebug(client);

                    await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken, "my");

                    await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    await client.Emit("message", "hello", "my", cancellationToken);
                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                    await client.DisconnectAsync(cancellationToken);
                },
                cancellationToken,
                nameof(SocketIoClient.EventReceived), 
                nameof(SocketIoClient.UnhandledEventReceived));
        }

        [TestMethod]
        public async Task RoomsTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = tokenSource.Token;

            await BaseTests.BaseTestAsync(
                async client =>
                {
                    await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken);
                    await client.Emit("message", cancellationToken: cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                    await client.DisconnectAsync(cancellationToken);
                },
                cancellationToken,
                nameof(SocketIoClient.EventReceived),
                nameof(SocketIoClient.UnhandledEventReceived));
        }

        [TestMethod]
        public async Task ArraysTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = tokenSource.Token;

            await BaseTests.BaseTestAsync(
                async client =>
                {
                    client.On<BaseTests.ChatMessage[]>("messages", data =>
                    {
                        Console.WriteLine(@"Messages:");
                        foreach (var value in data)
                        {
                            Console.WriteLine(value.Message);
                        };
                    });

                    await client.ConnectAsync(new Uri(LocalCharServerUrl), cancellationToken);
                    await client.Emit("message", "message", cancellationToken: cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                    await client.DisconnectAsync(cancellationToken);
                },
                cancellationToken,
                nameof(SocketIoClient.EventReceived));
        }

        [TestMethod]
        public async Task ConnectToLocalChatServerDebugTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var cancellationToken = tokenSource.Token;

            await BaseTests.BaseTestAsync(
                async client =>
                {
                    BaseTests.EnableDebug(client);

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
                cancellationToken);
        }
    }
}
