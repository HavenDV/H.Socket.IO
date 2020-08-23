using System.Collections.Generic;
using System.Linq;
using H.Socket.IO.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Socket.IO.Tests
{
    [TestClass]
    public class SocketIoUtilitiesTests
    {
        [TestMethod]
        public void GetEventValuesTest()
        {
            CollectionAreEqual(
                new[] { "messages", "[{},{},{}]" },
                "[\"messages\",[{},{},{}]]".GetJsonArrayValues());

            CollectionAreEqual(
                new [] { "message", "value" }, 
                "[\"message\",\"value\"]".GetJsonArrayValues());

            CollectionAreEqual(
                new[] { "message" },
                "[\"message\"]".GetJsonArrayValues());

            CollectionAreEqual(
                new[] { "message", "{}" },
                "[\"message\",{}]".GetJsonArrayValues());

            CollectionAreEqual(
                new[] { "new message", "{\"username\":\"1\",\"message\":\"1\"}" },
                "[\"new message\",{\"username\":\"1\",\"message\":\"1\"}]".GetJsonArrayValues());
        }

        private static void CollectionAreEqual<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (var (a, b) in first.Zip(second, (a, b) => (a, b)))
            {
                Assert.AreEqual(a, b);
            }
        }
    }
}
