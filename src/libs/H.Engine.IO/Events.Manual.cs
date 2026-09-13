#nullable enable

#pragma warning disable CA1034 // Preserve the public API formerly emitted by EventGenerator.

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class ClosedEventArgs : global::System.EventArgs
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
            public ClosedEventArgs(string reason, global::System.Net.WebSockets.WebSocketCloseStatus? status)
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

namespace H.Engine.IO
{
    public partial class EngineIoClient
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

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class MessageReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Message { get; }

            /// <summary>
            ///
            /// </summary>
            public MessageReceivedEventArgs(string message)
            {
                Message = message;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string message)
            {
                message = Message;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Message={Message})";
            }
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class NoopReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Message { get; }

            /// <summary>
            ///
            /// </summary>
            public NoopReceivedEventArgs(string message)
            {
                Message = message;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string message)
            {
                message = Message;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Message={Message})";
            }
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class OpenedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public global::H.Engine.IO.EngineIoOpenMessage Message { get; }

            /// <summary>
            ///
            /// </summary>
            public OpenedEventArgs(global::H.Engine.IO.EngineIoOpenMessage message)
            {
                Message = message;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out global::H.Engine.IO.EngineIoOpenMessage message)
            {
                message = Message;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Message={Message})";
            }
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class PingReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Message { get; }

            /// <summary>
            ///
            /// </summary>
            public PingReceivedEventArgs(string message)
            {
                Message = message;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string message)
            {
                message = Message;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Message={Message})";
            }
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class PingSentEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Message { get; }

            /// <summary>
            ///
            /// </summary>
            public PingSentEventArgs(string message)
            {
                Message = message;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string message)
            {
                message = Message;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Message={Message})";
            }
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class PongReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Message { get; }

            /// <summary>
            ///
            /// </summary>
            public PongReceivedEventArgs(string message)
            {
                Message = message;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string message)
            {
                message = Message;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Message={Message})";
            }
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        ///
        /// </summary>
        public class UpgradedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Message { get; }

            /// <summary>
            ///
            /// </summary>
            public UpgradedEventArgs(string message)
            {
                Message = message;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string message)
            {
                message = Message;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Message={Message})";
            }
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.ClosedEventArgs>? Closed;

        /// <summary>
        /// A helper method to subscribe the Closed event.
        /// </summary>
        public global::System.IDisposable SubscribeToClosed(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.ClosedEventArgs> handler)
        {
            Closed += handler;

            return new global::H.Engine.IO.EventSubscription(() => Closed -= handler);
        }

        /// <summary>
        /// A helper method to raise the Closed event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.ClosedEventArgs OnClosed(global::H.Engine.IO.EngineIoClient.ClosedEventArgs args)
        {
            Closed?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the Closed event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.ClosedEventArgs OnClosed(
            string reason,
            global::System.Net.WebSockets.WebSocketCloseStatus? status)
        {
            var args = new global::H.Engine.IO.EngineIoClient.ClosedEventArgs(reason, status);
            Closed?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.ExceptionOccurredEventArgs>? ExceptionOccurred;

        /// <summary>
        /// A helper method to subscribe the ExceptionOccurred event.
        /// </summary>
        public global::System.IDisposable SubscribeToExceptionOccurred(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.ExceptionOccurredEventArgs> handler)
        {
            ExceptionOccurred += handler;

            return new global::H.Engine.IO.EventSubscription(() => ExceptionOccurred -= handler);
        }

        /// <summary>
        /// A helper method to raise the ExceptionOccurred event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.ExceptionOccurredEventArgs OnExceptionOccurred(global::H.Engine.IO.EngineIoClient.ExceptionOccurredEventArgs args)
        {
            ExceptionOccurred?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the ExceptionOccurred event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.ExceptionOccurredEventArgs OnExceptionOccurred(
            global::System.Exception exception)
        {
            var args = new global::H.Engine.IO.EngineIoClient.ExceptionOccurredEventArgs(exception);
            ExceptionOccurred?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.MessageReceivedEventArgs>? MessageReceived;

        /// <summary>
        /// A helper method to subscribe the MessageReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToMessageReceived(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.MessageReceivedEventArgs> handler)
        {
            MessageReceived += handler;

            return new global::H.Engine.IO.EventSubscription(() => MessageReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the MessageReceived event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.MessageReceivedEventArgs OnMessageReceived(global::H.Engine.IO.EngineIoClient.MessageReceivedEventArgs args)
        {
            MessageReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the MessageReceived event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.MessageReceivedEventArgs OnMessageReceived(
            string message)
        {
            var args = new global::H.Engine.IO.EngineIoClient.MessageReceivedEventArgs(message);
            MessageReceived?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.NoopReceivedEventArgs>? NoopReceived;

        /// <summary>
        /// A helper method to subscribe the NoopReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToNoopReceived(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.NoopReceivedEventArgs> handler)
        {
            NoopReceived += handler;

            return new global::H.Engine.IO.EventSubscription(() => NoopReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the NoopReceived event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.NoopReceivedEventArgs OnNoopReceived(global::H.Engine.IO.EngineIoClient.NoopReceivedEventArgs args)
        {
            NoopReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the NoopReceived event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.NoopReceivedEventArgs OnNoopReceived(
            string message)
        {
            var args = new global::H.Engine.IO.EngineIoClient.NoopReceivedEventArgs(message);
            NoopReceived?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.OpenedEventArgs>? Opened;

        /// <summary>
        /// A helper method to subscribe the Opened event.
        /// </summary>
        public global::System.IDisposable SubscribeToOpened(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.OpenedEventArgs> handler)
        {
            Opened += handler;

            return new global::H.Engine.IO.EventSubscription(() => Opened -= handler);
        }

        /// <summary>
        /// A helper method to raise the Opened event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.OpenedEventArgs OnOpened(global::H.Engine.IO.EngineIoClient.OpenedEventArgs args)
        {
            Opened?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the Opened event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.OpenedEventArgs OnOpened(
            global::H.Engine.IO.EngineIoOpenMessage message)
        {
            var args = new global::H.Engine.IO.EngineIoClient.OpenedEventArgs(message);
            Opened?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.PingReceivedEventArgs>? PingReceived;

        /// <summary>
        /// A helper method to subscribe the PingReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToPingReceived(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.PingReceivedEventArgs> handler)
        {
            PingReceived += handler;

            return new global::H.Engine.IO.EventSubscription(() => PingReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the PingReceived event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.PingReceivedEventArgs OnPingReceived(global::H.Engine.IO.EngineIoClient.PingReceivedEventArgs args)
        {
            PingReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the PingReceived event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.PingReceivedEventArgs OnPingReceived(
            string message)
        {
            var args = new global::H.Engine.IO.EngineIoClient.PingReceivedEventArgs(message);
            PingReceived?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.PingSentEventArgs>? PingSent;

        /// <summary>
        /// A helper method to subscribe the PingSent event.
        /// </summary>
        public global::System.IDisposable SubscribeToPingSent(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.PingSentEventArgs> handler)
        {
            PingSent += handler;

            return new global::H.Engine.IO.EventSubscription(() => PingSent -= handler);
        }

        /// <summary>
        /// A helper method to raise the PingSent event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.PingSentEventArgs OnPingSent(global::H.Engine.IO.EngineIoClient.PingSentEventArgs args)
        {
            PingSent?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the PingSent event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.PingSentEventArgs OnPingSent(
            string message)
        {
            var args = new global::H.Engine.IO.EngineIoClient.PingSentEventArgs(message);
            PingSent?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.PongReceivedEventArgs>? PongReceived;

        /// <summary>
        /// A helper method to subscribe the PongReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToPongReceived(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.PongReceivedEventArgs> handler)
        {
            PongReceived += handler;

            return new global::H.Engine.IO.EventSubscription(() => PongReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the PongReceived event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.PongReceivedEventArgs OnPongReceived(global::H.Engine.IO.EngineIoClient.PongReceivedEventArgs args)
        {
            PongReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the PongReceived event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.PongReceivedEventArgs OnPongReceived(
            string message)
        {
            var args = new global::H.Engine.IO.EngineIoClient.PongReceivedEventArgs(message);
            PongReceived?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.Engine.IO
{
    public partial class EngineIoClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.Engine.IO.EngineIoClient.UpgradedEventArgs>? Upgraded;

        /// <summary>
        /// A helper method to subscribe the Upgraded event.
        /// </summary>
        public global::System.IDisposable SubscribeToUpgraded(global::System.EventHandler<global::H.Engine.IO.EngineIoClient.UpgradedEventArgs> handler)
        {
            Upgraded += handler;

            return new global::H.Engine.IO.EventSubscription(() => Upgraded -= handler);
        }

        /// <summary>
        /// A helper method to raise the Upgraded event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.UpgradedEventArgs OnUpgraded(global::H.Engine.IO.EngineIoClient.UpgradedEventArgs args)
        {
            Upgraded?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the Upgraded event.
        /// </summary>
        private global::H.Engine.IO.EngineIoClient.UpgradedEventArgs OnUpgraded(
            string message)
        {
            var args = new global::H.Engine.IO.EngineIoClient.UpgradedEventArgs(message);
            Upgraded?.Invoke(this, args);

            return args;
        }
    }
}


namespace H.Engine.IO
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
