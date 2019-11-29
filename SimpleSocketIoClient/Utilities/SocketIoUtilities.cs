using System;
using System.Linq;

namespace SimpleSocketIoClient.Utilities
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
        public static string[] GetEventValues(this string text)
        {
            text = text ?? throw new ArgumentNullException(nameof(text));
            if (!text.StartsWith("[") || !text.EndsWith("]"))
            {
                throw new ArgumentException("text must begin with \'[\' and end with \']\'");
            }

            return text
                .Trim('[', ']')
                .Split(',')
                .Select(value => value.Trim('\"'))
                .ToArray();
        }
    }
}
