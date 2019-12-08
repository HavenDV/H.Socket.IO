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
            public event EventHandler CommonEvent;
            public event EventHandler EventThatWillNeverHappen;

            public void OnCommonEvent() => CommonEvent?.Invoke(this, EventArgs.Empty);
        }

        [TestMethod]
        public async Task WaitEventAsyncTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var testObject = new TestClass();

            var result = await testObject.WaitEventAsync(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);

                testObject.OnCommonEvent();
            }, nameof(TestClass.CommonEvent), cancellationTokenSource.Token);

            Assert.IsTrue(result, nameof(TestClass.CommonEvent));
        }

        [TestMethod]
        public async Task WaitEventsAsyncTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var testObject = new TestClass();

            var results = await testObject.WaitEventsAsync(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);

                testObject.OnCommonEvent();
            }, cancellationTokenSource.Token, nameof(TestClass.CommonEvent), nameof(TestClass.EventThatWillNeverHappen));

            Assert.IsTrue(results[nameof(TestClass.CommonEvent)], nameof(TestClass.CommonEvent));
            Assert.IsFalse(results[nameof(TestClass.EventThatWillNeverHappen)], nameof(TestClass.EventThatWillNeverHappen));
        }
    }
}
