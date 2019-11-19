using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace SimpleSocketIoClient.Tests
{
    [TestClass]
    public class WebSocketClientTests
    {
        [TestMethod]
        public async Task ConnectToWebSocketOrgTest()
        {
            await using var client = new WebSocketClient();

            client.AfterText += (sender, args) => Console.WriteLine($"AfterText: {args.Value}");
            client.AfterException += (sender, args) => Console.WriteLine($"AfterException: {args.Value}");
            client.AfterBinary += (sender, args) => Console.WriteLine($"AfterBinary: {args.Value?.Length}");

            await client.ConnectAsync(new Uri("ws://echo.websocket.org"));

            await client.SendTextAsync("Test");

            await Task.Delay(TimeSpan.FromSeconds(5));

            await client.DisconnectAsync();
        }
    }
}
