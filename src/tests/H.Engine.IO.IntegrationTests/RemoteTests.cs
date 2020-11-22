using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Engine.IO.IntegrationTests
{
    [TestClass]
    public class RemoteTests
    {
        [TestMethod]
        public async Task ConnectToChatNowShTest()
        {
            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            await BaseTests.ConnectToChatBaseTestAsync("wss://socketio-chat-h9jt.herokuapp.com/", tokenSource.Token);
        }
    }
}
