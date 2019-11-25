using System;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.Tests
{
    [TestClass]
    public class EngineIoClientTests
    {
        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
#if NETCOREAPP3_0
            await using var client = new EngineIoClient("socket.io");
#else
            using var client = new EngineIoClient("socket.io");
#endif
            client.AfterMessage += (sender, args) => Console.WriteLine($"AfterMessage: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");
            client.Opened += (sender, args) => Console.WriteLine($"Opened: {args.Value}");
            client.Closed += (sender, args) => Console.WriteLine($"Closed. Reason: {args.Value.Reason}, Status: {args.Value.Status:G}");

            var events = new[] {nameof(client.Opened), nameof(client.Closed)};
            var results = await client.WaitEventsAsync(async cancellationToken =>
            {
                await client.OpenAsync(new Uri("https://socket-io-chat.now.sh/"), 10);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

                await client.CloseAsync(cancellationToken);
            }, cancellationTokenSource.Token, events);

            foreach (var (result, eventName) in results.Zip(events, (a, b) => (a, b)))
            {
                Assert.IsTrue(result, $"Client event(\"{eventName}\") did not happen");
            }
        }
    }
}
