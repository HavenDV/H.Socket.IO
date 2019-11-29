using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.Tests
{
    [TestClass]
    public class SocketIoUtilitiesTests
    {
        [TestMethod]
        public void GetEventValuesTest()
        {
            CollectionAssert.AreEqual(
                new [] { "message", "value" }, 
                "[\"message\",\"value\"]".GetEventValues());

            CollectionAssert.AreEqual(
                new[] { "message" },
                "[\"message\"]".GetEventValues());

            CollectionAssert.AreEqual(
                new[] { "message", "{}" },
                "[\"message\",{}]".GetEventValues());
        }
    }
}
