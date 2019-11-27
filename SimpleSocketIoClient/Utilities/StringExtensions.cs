using System;

namespace SimpleSocketIoClient.Utilities
{
    /// <summary>
    /// Extensions that work with <see langword="string"/>
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Retrieves the string between the starting fragment and the ending.
        /// The first available fragment is retrieved.
        /// Returns <see langword="null"/> if nothing is found.
        /// If <paramref name="end"/> is not specified, the end is the end of the string.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static string? Extract(this string text, string start, string? end = null)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            start = start ?? throw new ArgumentNullException(nameof(start));

            var index1 = text.IndexOf(start, StringComparison.Ordinal);
            if (index1 < 0)
            {
                return null;
            }

            index1 += start.Length;
            if (end == null)
            {
                return text.Substring(index1);
            }

            var index2 = text.IndexOf(end, index1, StringComparison.Ordinal);
            if (index2 < 0)
            {
                return null;
            }

#if NETSTANDARD2_1
            return text[index1..index2];
#else
            return text.Substring(index1, index2 - index1);
#endif
        }
    }
}
