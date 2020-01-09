using System;

namespace H.WebSockets.Utilities
{
    /// <summary>
    /// Single value template class for <see cref="EventArgs"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataEventArgs<T> : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public DataEventArgs(T value)
        {
            Value = value;
        }
    }
}
