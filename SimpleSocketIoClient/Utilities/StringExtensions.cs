using System;
using System.Collections.Generic;

namespace SimpleSocketIoClient.Utilities
{
    /// <summary>
    /// Extensions that work with <see langword="string"/>
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Splits by indexes
        /// </summary>
        /// <param name="text"></param>
        /// <param name="indexes"></param>
        /// <returns></returns>
        public static string[] SplitByIndexes(this string text, int[] indexes)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            indexes = indexes ?? throw new ArgumentNullException(nameof(indexes));

            var values = new List<string>();
            var lastIndex = 0;
            foreach (var index in indexes)
            {
                values.Add(text.Substring(lastIndex, index - lastIndex));

                lastIndex = index + 1;
            }

            values.Add(text.Substring(lastIndex));

            return values.ToArray(); 
        }
    }
}
