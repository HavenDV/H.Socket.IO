using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace SimpleSocketIoClient.Tests
{
    [TestClass]
    public class SocketIoClientTests
    {
        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            await using var client = new SocketIoClient();

            client.EngineIoClient.WebSocketClient.AfterText += (sender, args) => Console.WriteLine($"EngineIoClient.WebSocketClient.AfterText: {args.Value}");
            client.AfterEvent += (sender, args) => Console.WriteLine($"AfterEvent: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");

            await client.ConnectAsync(new Uri("https://socket-io-chat.now.sh/"));

            await Task.Delay(TimeSpan.FromSeconds(5));

            await client.DisconnectAsync();
        }
    }
}
