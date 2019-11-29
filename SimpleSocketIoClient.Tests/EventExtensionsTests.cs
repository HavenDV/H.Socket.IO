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
            public event EventHandler TestEvent1;
            public event EventHandler TestEvent2;

            public void OnTestEvent1() => TestEvent1?.Invoke(this, EventArgs.Empty);
        }

        [TestMethod]
        public async Task WaitEventAsyncTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var testObject = new TestClass();

            var result = await testObject.WaitEventAsync(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);

                testObject.OnTestEvent1();
            }, nameof(TestClass.TestEvent1), cancellationTokenSource.Token);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task WaitEventsAsyncTest()
        {
            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(250));
            var testObject = new TestClass();

            var eventNames = new[] { nameof(TestClass.TestEvent1), nameof(TestClass.TestEvent2) };
            var results = await testObject.WaitEventsAsync(async cancellationToken =>
            {
                await Task.Delay(TimeSpan.FromMilliseconds(1), cancellationToken);

                testObject.OnTestEvent1();
            }, cancellationTokenSource.Token, eventNames);

            Assert.IsTrue(results[0]);
            Assert.IsFalse(results[1]);
        }
    }
}
