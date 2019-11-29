using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient.Tests
{
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

            CollectionAssert.AreEqual(
                new[] { "[\"message\"", "\"value\"", "\"value\"", "\"value\"]" },
                text.SplitByIndexes(indexes.ToArray()));
        }
    }
}
