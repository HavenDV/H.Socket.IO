
#pragma warning disable CA1034 // Preserve the public API formerly emitted by EventGenerator.

namespace H.Socket.IO
{
    internal sealed class EventSubscription : global::System.IDisposable
    {
        private readonly global::System.Action action;

        public EventSubscription(global::System.Action action)
        {
            this.action = action;
        }

        public void Dispose()
        {
            action();
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class ConnectedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Value { get; }

            /// <summary>
            ///
            /// </summary>
            public string Namespace { get; }

            /// <summary>
            ///
            /// </summary>
            public bool IsHandled { get; }

            /// <summary>
            ///
            /// </summary>
            public ConnectedEventArgs(string value, string @namespace, bool isHandled)
            {
                Value = value;
                Namespace = @namespace;
                IsHandled = isHandled;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string value, out string @namespace, out bool isHandled)
            {
                value = Value;
                @namespace = Namespace;
                isHandled = IsHandled;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Value={Value}, Namespace={Namespace}, IsHandled={IsHandled})";
            }
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class DisconnectedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Reason { get; }

            /// <summary>
            ///
            /// </summary>
            public global::System.Net.WebSockets.WebSocketCloseStatus? Status { get; }

            /// <summary>
            ///
            /// </summary>
            public DisconnectedEventArgs(string reason, global::System.Net.WebSockets.WebSocketCloseStatus? status)
            {
                Reason = reason;
                Status = status;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string reason, out global::System.Net.WebSockets.WebSocketCloseStatus? status)
            {
                reason = Reason;
                status = Status;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Reason={Reason}, Status={Status})";
            }
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class ErrorReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Value { get; }

            /// <summary>
            ///
            /// </summary>
            public string Namespace { get; }

            /// <summary>
            ///
            /// </summary>
            public ErrorReceivedEventArgs(string value, string @namespace)
            {
                Value = value;
                Namespace = @namespace;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string value, out string @namespace)
            {
                value = Value;
                @namespace = Namespace;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Value={Value}, Namespace={Namespace})";
            }
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class EventReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Value { get; }

            /// <summary>
            ///
            /// </summary>
            public string Namespace { get; }

            /// <summary>
            ///
            /// </summary>
            public bool IsHandled { get; }

            /// <summary>
            ///
            /// </summary>
            public EventReceivedEventArgs(string value, string @namespace, bool isHandled)
            {
                Value = value;
                Namespace = @namespace;
                IsHandled = isHandled;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string value, out string @namespace, out bool isHandled)
            {
                value = Value;
                @namespace = Namespace;
                isHandled = IsHandled;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Value={Value}, Namespace={Namespace}, IsHandled={IsHandled})";
            }
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class ExceptionOccurredEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public global::System.Exception Exception { get; }

            /// <summary>
            ///
            /// </summary>
            public ExceptionOccurredEventArgs(global::System.Exception exception)
            {
                Exception = exception;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out global::System.Exception exception)
            {
                exception = Exception;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Exception={Exception})";
            }
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class HandledEventReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Value { get; }

            /// <summary>
            ///
            /// </summary>
            public string Namespace { get; }

            /// <summary>
            ///
            /// </summary>
            public bool IsHandled { get; }

            /// <summary>
            ///
            /// </summary>
            public HandledEventReceivedEventArgs(string value, string @namespace, bool isHandled)
            {
                Value = value;
                Namespace = @namespace;
                IsHandled = isHandled;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string value, out string @namespace, out bool isHandled)
            {
                value = Value;
                @namespace = Namespace;
                isHandled = IsHandled;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Value={Value}, Namespace={Namespace}, IsHandled={IsHandled})";
            }
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class UnhandledEventReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Value { get; }

            /// <summary>
            ///
            /// </summary>
            public string Namespace { get; }

            /// <summary>
            ///
            /// </summary>
            public bool IsHandled { get; }

            /// <summary>
            ///
            /// </summary>
            public UnhandledEventReceivedEventArgs(string value, string @namespace, bool isHandled)
            {
                Value = value;
                Namespace = @namespace;
                IsHandled = isHandled;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string value, out string @namespace, out bool isHandled)
            {
                value = Value;
                @namespace = Namespace;
                isHandled = IsHandled;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Value={Value}, Namespace={Namespace}, IsHandled={IsHandled})";
            }
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        /// Occurs after a successful connection to each namespace.
        /// </summary>
        public event global::System.EventHandler<global::H.Socket.IO.SocketIoClient.ConnectedEventArgs>? Connected;

        /// <summary>
        /// A helper method to subscribe the Connected event.
        /// </summary>
        public global::System.IDisposable SubscribeToConnected(global::System.EventHandler<global::H.Socket.IO.SocketIoClient.ConnectedEventArgs> handler)
        {
            Connected += handler;

            return new global::H.Socket.IO.EventSubscription(() => Connected -= handler);
        }

        /// <summary>
        /// A helper method to raise the Connected event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.ConnectedEventArgs OnConnected(global::H.Socket.IO.SocketIoClient.ConnectedEventArgs args)
        {
            Connected?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the Connected event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.ConnectedEventArgs OnConnected(
            string value,
            string @namespace,
            bool isHandled)
        {
            var args = new global::H.Socket.IO.SocketIoClient.ConnectedEventArgs(value, @namespace, isHandled);
            Connected?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        /// Occurs after a disconnection.
        /// </summary>
        public event global::System.EventHandler<global::H.Socket.IO.SocketIoClient.DisconnectedEventArgs>? Disconnected;

        /// <summary>
        /// A helper method to subscribe the Disconnected event.
        /// </summary>
        public global::System.IDisposable SubscribeToDisconnected(global::System.EventHandler<global::H.Socket.IO.SocketIoClient.DisconnectedEventArgs> handler)
        {
            Disconnected += handler;

            return new global::H.Socket.IO.EventSubscription(() => Disconnected -= handler);
        }

        /// <summary>
        /// A helper method to raise the Disconnected event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.DisconnectedEventArgs OnDisconnected(global::H.Socket.IO.SocketIoClient.DisconnectedEventArgs args)
        {
            Disconnected?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the Disconnected event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.DisconnectedEventArgs OnDisconnected(
            string reason,
            global::System.Net.WebSockets.WebSocketCloseStatus? status)
        {
            var args = new global::H.Socket.IO.SocketIoClient.DisconnectedEventArgs(reason, status);
            Disconnected?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        /// Occurs after new error.
        /// </summary>
        public event global::System.EventHandler<global::H.Socket.IO.SocketIoClient.ErrorReceivedEventArgs>? ErrorReceived;

        /// <summary>
        /// A helper method to subscribe the ErrorReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToErrorReceived(global::System.EventHandler<global::H.Socket.IO.SocketIoClient.ErrorReceivedEventArgs> handler)
        {
            ErrorReceived += handler;

            return new global::H.Socket.IO.EventSubscription(() => ErrorReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the ErrorReceived event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.ErrorReceivedEventArgs OnErrorReceived(global::H.Socket.IO.SocketIoClient.ErrorReceivedEventArgs args)
        {
            ErrorReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the ErrorReceived event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.ErrorReceivedEventArgs OnErrorReceived(
            string value,
            string @namespace)
        {
            var args = new global::H.Socket.IO.SocketIoClient.ErrorReceivedEventArgs(value, @namespace);
            ErrorReceived?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        /// Occurs after new event.
        /// </summary>
        public event global::System.EventHandler<global::H.Socket.IO.SocketIoClient.EventReceivedEventArgs>? EventReceived;

        /// <summary>
        /// A helper method to subscribe the EventReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToEventReceived(global::System.EventHandler<global::H.Socket.IO.SocketIoClient.EventReceivedEventArgs> handler)
        {
            EventReceived += handler;

            return new global::H.Socket.IO.EventSubscription(() => EventReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the EventReceived event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.EventReceivedEventArgs OnEventReceived(global::H.Socket.IO.SocketIoClient.EventReceivedEventArgs args)
        {
            EventReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the EventReceived event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.EventReceivedEventArgs OnEventReceived(
            string value,
            string @namespace,
            bool isHandled)
        {
            var args = new global::H.Socket.IO.SocketIoClient.EventReceivedEventArgs(value, @namespace, isHandled);
            EventReceived?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        /// Occurs after new exception.
        /// </summary>
        public event global::System.EventHandler<global::H.Socket.IO.SocketIoClient.ExceptionOccurredEventArgs>? ExceptionOccurred;

        /// <summary>
        /// A helper method to subscribe the ExceptionOccurred event.
        /// </summary>
        public global::System.IDisposable SubscribeToExceptionOccurred(global::System.EventHandler<global::H.Socket.IO.SocketIoClient.ExceptionOccurredEventArgs> handler)
        {
            ExceptionOccurred += handler;

            return new global::H.Socket.IO.EventSubscription(() => ExceptionOccurred -= handler);
        }

        /// <summary>
        /// A helper method to raise the ExceptionOccurred event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.ExceptionOccurredEventArgs OnExceptionOccurred(global::H.Socket.IO.SocketIoClient.ExceptionOccurredEventArgs args)
        {
            ExceptionOccurred?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the ExceptionOccurred event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.ExceptionOccurredEventArgs OnExceptionOccurred(
            global::System.Exception exception)
        {
            var args = new global::H.Socket.IO.SocketIoClient.ExceptionOccurredEventArgs(exception);
            ExceptionOccurred?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        /// Occurs after new handled event(captured by any On).
        /// </summary>
        public event global::System.EventHandler<global::H.Socket.IO.SocketIoClient.HandledEventReceivedEventArgs>? HandledEventReceived;

        /// <summary>
        /// A helper method to subscribe the HandledEventReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToHandledEventReceived(global::System.EventHandler<global::H.Socket.IO.SocketIoClient.HandledEventReceivedEventArgs> handler)
        {
            HandledEventReceived += handler;

            return new global::H.Socket.IO.EventSubscription(() => HandledEventReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the HandledEventReceived event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.HandledEventReceivedEventArgs OnHandledEventReceived(global::H.Socket.IO.SocketIoClient.HandledEventReceivedEventArgs args)
        {
            HandledEventReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the HandledEventReceived event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.HandledEventReceivedEventArgs OnHandledEventReceived(
            string value,
            string @namespace,
            bool isHandled)
        {
            var args = new global::H.Socket.IO.SocketIoClient.HandledEventReceivedEventArgs(value, @namespace, isHandled);
            HandledEventReceived?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Socket.IO
{
    public partial class SocketIoClient
    {
        /// <summary>
        /// Occurs after new unhandled event(not captured by any On).
        /// </summary>
        public event global::System.EventHandler<global::H.Socket.IO.SocketIoClient.UnhandledEventReceivedEventArgs>? UnhandledEventReceived;

        /// <summary>
        /// A helper method to subscribe the UnhandledEventReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToUnhandledEventReceived(global::System.EventHandler<global::H.Socket.IO.SocketIoClient.UnhandledEventReceivedEventArgs> handler)
        {
            UnhandledEventReceived += handler;

            return new global::H.Socket.IO.EventSubscription(() => UnhandledEventReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the UnhandledEventReceived event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.UnhandledEventReceivedEventArgs OnUnhandledEventReceived(global::H.Socket.IO.SocketIoClient.UnhandledEventReceivedEventArgs args)
        {
            UnhandledEventReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the UnhandledEventReceived event.
        /// </summary>
        private global::H.Socket.IO.SocketIoClient.UnhandledEventReceivedEventArgs OnUnhandledEventReceived(
            string value,
            string @namespace,
            bool isHandled)
        {
            var args = new global::H.Socket.IO.SocketIoClient.UnhandledEventReceivedEventArgs(value, @namespace, isHandled);
            UnhandledEventReceived?.Invoke(this, args);

            return args;
        }
    }
}
