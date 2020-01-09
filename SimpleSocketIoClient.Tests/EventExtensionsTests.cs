using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using H.WebSockets.Utilities;

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

            Assert.IsNotNull(result, nameof(TestClass.CommonEvent));
        }

        [TestMethod]
        public async Task WaitAllEventsAsyncTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var testObject = new TestClass();

            var results = await testObject.WaitAllEventsAsync(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);

                testObject.OnCommonEvent();
            }, cancellationTokenSource.Token, nameof(TestClass.CommonEvent), nameof(TestClass.EventThatWillNeverHappen));

            Assert.IsNotNull(results[nameof(TestClass.CommonEvent)], nameof(TestClass.CommonEvent));
            Assert.IsNull(results[nameof(TestClass.EventThatWillNeverHappen)], nameof(TestClass.EventThatWillNeverHappen));
        }

        [TestMethod]
        public async Task WaitAnyEventAsyncTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var testObject = new TestClass();

            var results = await testObject.WaitAnyEventAsync(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);

                testObject.OnCommonEvent();
            }, cancellationTokenSource.Token, nameof(TestClass.CommonEvent), nameof(TestClass.EventThatWillNeverHappen));

            Assert.IsNotNull(results[nameof(TestClass.CommonEvent)], nameof(TestClass.CommonEvent));
            Assert.IsNull(results[nameof(TestClass.EventThatWillNeverHappen)], nameof(TestClass.EventThatWillNeverHappen));
        }
    }
}
