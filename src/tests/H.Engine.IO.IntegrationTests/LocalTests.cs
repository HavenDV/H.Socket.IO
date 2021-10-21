namespace H.Engine.IO.IntegrationTests;

[TestClass]
public class EngineIoClientTests
{
    public const string LocalCharServerUrl = "ws://localhost:1465/";

    [TestMethod]
    public async Task ConnectToLocalChatServerTest()
    {
        TestUtilities.CheckDeveloper();

        using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await BaseTests.ConnectToChatBaseTestAsync(LocalCharServerUrl, tokenSource.Token);
    }
}
