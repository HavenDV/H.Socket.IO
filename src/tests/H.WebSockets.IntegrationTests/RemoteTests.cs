//using H.WebSockets.Utilities;

namespace H.WebSockets.IntegrationTests;

[TestClass]
public class RemoteTests
{
    //        [TestMethod]
    //        public async Task WebSocketOrgTest()
    //        {
    //            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    //            var cancellationToken = tokenSource.Token;

    //#if NET5_0
    //            await using var client = new WebSocketClient();
    //#else
    //            using var client = new WebSocketClient();
    //#endif

    //            client.TextReceived += (_, args) => Console.WriteLine($"TextReceived: {args.Value}");
    //            client.ExceptionOccurred += (_, args) => Console.WriteLine($"ExceptionOccurred: {args.Value}");
    //            client.BytesReceived += (_, args) => Console.WriteLine($"BytesReceived: {args.Value.Count}");
    //            client.Connected += (_, _) => Console.WriteLine("Connected");
    //            client.Disconnected += (_, args) => Console.WriteLine($"Disconnected. Reason: {args.Reason}, Status: {args.Status:G}");

    //            var events = new[] { nameof(client.Connected), nameof(client.Disconnected) };
    //            await ConnectDisconnectTestAsync(client, events, cancellationToken);
    //            await ConnectDisconnectTestAsync(client, events, cancellationToken);
    //        }

    //        private static async Task ConnectDisconnectTestAsync(
    //            WebSocketClient client, 
    //            string[] events, 
    //            CancellationToken cancellationToken = default)
    //        {
    //            var results = await client.WaitAllEventsAsync<EventArgs>(async () =>
    //            {
    //                Console.WriteLine("# Before ConnectAsync");

    //                Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));

    //                await client.ConnectAsync(new Uri("wss://echo.websocket.org"), cancellationToken);

    //                Assert.IsTrue(client.IsConnected, nameof(client.IsConnected));

    //                Console.WriteLine("# Before SendTextAsync");

    //                var args = await client.WaitTextAsync(async () =>
    //                {
    //                    await client.SendTextAsync("Test", cancellationToken);
    //                }, cancellationToken);

    //                Console.WriteLine($"WaitTextAsync: {args.Value}");

    //                Console.WriteLine("# Before Delay");

    //                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

    //                Console.WriteLine("# Before DisconnectAsync");

    //                await client.DisconnectAsync(cancellationToken);

    //                Console.WriteLine("# After DisconnectAsync");

    //                Assert.IsFalse(client.IsConnected, nameof(client.IsConnected));
    //            }, cancellationToken, events);

    //            Console.WriteLine();
    //            Console.WriteLine($"WebSocket State: {client.Socket.State}");
    //            Console.WriteLine($"WebSocket CloseStatus: {client.Socket.CloseStatus}");
    //            Console.WriteLine($"WebSocket CloseStatusDescription: {client.Socket.CloseStatusDescription}");

    //            foreach (var pair in results)
    //            {
    //                Assert.IsNotNull(pair.Value, $"Client event(\"{pair.Key}\") did not happen");
    //            }
    //        }
}
