namespace H.Engine.IO.IntegrationTests;

[TestClass]
public class RemoteTests
{
    [TestMethod]
    public async Task ConnectToChatNowShTest()
    {
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var cancellationToken = cancellationTokenSource.Token;

        await BaseTests.ConnectToChatBaseTestAsync("wss://socketio-chat-h9jt.herokuapp.com/", cancellationToken);
    }
}
