using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace SimpleSocketIoClient.Tests
{
    [TestClass]
    public class EngineIoClientTests
    {
        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
#if NETCOREAPP3_0
            await using var client = new EngineIoClient("socket.io");
#else
            using var client = new EngineIoClient("socket.io");
#endif

            client.AfterMessage += (sender, args) => Console.WriteLine($"AfterMessage: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");
            client.Opened += (sender, args) => Console.WriteLine($"Opened: {args.Value}");
            client.Closed += (sender, args) => Console.WriteLine($"Closed. Reason: {args.Value.Reason}, Status: {args.Value.Status:G}");

            await client.OpenAsync(new Uri("https://socket-io-chat.now.sh/"), 10);

            await Task.Delay(TimeSpan.FromSeconds(2));

            await client.CloseAsync();
        }
    }
}
