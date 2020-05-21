using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Engine.IO.IntegrationTests
{
    [TestClass]
    [TestCategory("Local")]
    public class EngineIoClientTests
    {
        public const string LocalCharServerUrl = "ws://localhost:1465/";

        [TestMethod]
        public async Task ConnectToLocalChatServerTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await BaseTests.ConnectToChatBaseTestAsync(LocalCharServerUrl, tokenSource.Token);
        }
    }
}
