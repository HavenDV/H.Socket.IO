using System;

namespace SimpleSocketIoClient.Utilities
{
    public class DataEventArgs<T> : EventArgs
    {
        public T Value { get; set; }

        public DataEventArgs(T value)
        {
            Value = value;
        }
    }
}
