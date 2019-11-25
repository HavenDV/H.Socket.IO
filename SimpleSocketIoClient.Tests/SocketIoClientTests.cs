using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.Tests
{
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
            client.Disconnected += (sender, args) => Console.WriteLine($"Disconnected. Reason: {args.Value.Reason}, Status: {args.Value.Status:G}");
            client.AfterEvent += (sender, args) => Console.WriteLine($"AfterEvent: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");

            var events = new[] { nameof(client.Connected), nameof(client.Disconnected) };
            var results = await client.WaitEventsAsync(async cancellationToken =>
            {
                await client.ConnectAsync(new Uri("https://socket-io-chat.now.sh/"), 10);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.DisconnectAsync(cancellationToken);
            }, cancellationTokenSource.Token, events);

            foreach (var (result, eventName) in results.Zip(events, (a, b) => (a, b)))
            {
                Assert.IsTrue(result, $"Client event(\"{eventName}\") did not happen");
            }
        }
    }
}
