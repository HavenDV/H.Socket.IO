using System;
using System.Collections.Generic;
using System.Linq;
using H.Socket.IO.Properties;

namespace H.Socket.IO.Utilities
{
    /// <summary>
    /// Utilities for <see cref="SocketIoClient"/>
    /// </summary>
    public static class SocketIoUtilities
    {
        /// <summary>
        /// Return values from strings like ["message","value"] or ["message",{}]
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string[] GetJsonArrayValues(this string text)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            if (!text.StartsWith("[", StringComparison.OrdinalIgnoreCase) || !text.EndsWith("]", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(Resources.SocketIoUtilities_GetJsonArrayValues_text_must_begin_with_____and_end_with____);
            }

            text = text.Trim('[', ']');

            var indexes = new List<int>();
            var countOfObjects = 0;
            for (var i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '{':
                        countOfObjects++;
                        break;

                    case '}':
                        countOfObjects--;
                        break;

                    case ',' when countOfObjects == 0:
                        indexes.Add(i);
                        break;
                }
            }

            return text
                .SplitByIndexes(indexes.ToArray())
                .Select(value => value.Trim('\"'))
                .ToArray();
        }
    }
}
