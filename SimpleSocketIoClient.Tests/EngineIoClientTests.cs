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
            await using var client = new EngineIoClient("socket.io");

            client.WebSocketClient.AfterText += (sender, args) => Console.WriteLine($"EngineIoClient.WebSocketClient.AfterText: {args.Value}");
            client.AfterMessage += (sender, args) => Console.WriteLine($"AfterMessage: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");

            await client.OpenAsync(new Uri("https://socket-io-chat.now.sh/"));

            await Task.Delay(TimeSpan.FromSeconds(5));

            await client.CloseAsync();
        }
    }
}
