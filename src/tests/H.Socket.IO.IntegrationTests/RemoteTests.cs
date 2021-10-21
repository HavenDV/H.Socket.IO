namespace H.Socket.IO.IntegrationTests;

[TestClass]
public class RemoteTests
{
    [TestMethod]
    public async Task ConnectToChatNowShTest()
    {
        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var cancellationToken = tokenSource.Token;

        await BaseTests.ConnectToChatBaseTestAsync("wss://socketio-chat-h9jt.herokuapp.com/", cancellationToken);
    }
}
