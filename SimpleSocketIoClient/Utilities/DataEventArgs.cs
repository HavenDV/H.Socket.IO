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

    public class DataEventArgs<T1, T2> : EventArgs
    {
        public T1 Value1 { get; set; }
        public T2 Value2 { get; set; }

        public DataEventArgs(T1 value1, T2 value2)
        {
            Value1 = value1;
            Value2 = value2;
        }
    }
}
