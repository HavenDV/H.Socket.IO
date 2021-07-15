using H.Socket.IO.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace H.Socket.IO.Tests
{
    [TestClass]
    public class SocketIoUtilitiesTests
    {
        [TestMethod]
        public void GetEventValuesTest()
        {
            "[\"messages\",[{},{},{}]]".GetJsonArrayValues()
                .Should().BeEquivalentTo(new[] { "messages", "[{},{},{}]" });
            "[\"message\",\"value\"]".GetJsonArrayValues()
                .Should().BeEquivalentTo(new[] { "message", "value" });
            "[\"message\"]".GetJsonArrayValues()
                .Should().BeEquivalentTo(new[] { "message" });
            "[\"message\",{}]".GetJsonArrayValues()
                .Should().BeEquivalentTo(new[] { "message", "{}" });
            "[\"new message\",{\"username\":\"1\",\"message\":\"1\"}]".GetJsonArrayValues()
                .Should().BeEquivalentTo(new[] { "new message", "{\"username\":\"1\",\"message\":\"1\"}" });
        }
    }
}
