namespace H.Socket.IO.EventsArgs;

/// <summary>
/// Arguments used in <see cref="SocketIoClient.ErrorReceived"/> event
/// </summary>
public class SocketIoErrorEventArgs : SocketIoEventArgs
{
    /// <summary>
    /// Base constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="namespace"></param>
    public SocketIoErrorEventArgs(string value, string @namespace) :
        base(value, @namespace)
    {
    }
}
