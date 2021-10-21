namespace H.Socket.IO.EventsArgs;

/// <summary>
/// Arguments used in <see cref="SocketIoClient.EventReceived"/> event
/// </summary>
public class SocketIoEventEventArgs : SocketIoEventArgs
{
    /// <summary>
    /// IsHandled
    /// </summary>
    public bool IsHandled { get; }

    /// <summary>
    /// Base constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="namespace"></param>
    /// <param name="isHandled"></param>
    public SocketIoEventEventArgs(string value, string @namespace, bool isHandled) :
        base(value, @namespace)
    {
        IsHandled = isHandled;
    }
}
