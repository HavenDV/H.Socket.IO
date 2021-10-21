using H.Socket.IO.Utilities;

namespace H.Socket.IO.Tests;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void SplitByIndexesTest()
    {
        const string text = "[\"message\",\"value\",\"value\",\"value\"]";
        var indexes = new List<int>();
        var index = 0;
        do
        {
            index = text.IndexOf(',', index + 1);

            if (index >= 0)
            {
                indexes.Add(index);
            }
        } while (index >= 0);

        text.SplitByIndexes(indexes.ToArray())
            .Should().BeEquivalentTo(new[] { "[\"message\"", "\"value\"", "\"value\"", "\"value\"]" });
    }
}
