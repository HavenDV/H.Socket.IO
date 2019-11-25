using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.Tests
{
    [TestClass]
    public class EventExtensionsTests
    {
        private class TestClass
        {
            public event EventHandler TestEvent;

            public void OnTestEvent() => TestEvent?.Invoke(this, EventArgs.Empty);
        }

        [TestMethod]
        public async Task WaitEventAsyncTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var testObject = new TestClass();

            var result = await testObject.WaitEventAsync(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);

                testObject.OnTestEvent();
            }, nameof(TestClass.TestEvent), cancellationTokenSource.Token);

            Assert.IsTrue(result);
        }
    }
}
