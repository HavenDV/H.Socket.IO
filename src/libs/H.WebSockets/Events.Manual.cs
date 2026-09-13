
#pragma warning disable CA1034 // Preserve the public API formerly emitted by EventGenerator.

namespace H.WebSockets
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

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
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

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
    {
        /// <summary>
        ///
        /// </summary>
        public class FailedOrCanceledEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public global::System.Exception Exception { get; }

            /// <summary>
            ///
            /// </summary>
            public FailedOrCanceledEventArgs(global::System.Exception exception)
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

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
    {
        /// <summary>
        ///
        /// </summary>
        public class SuccessfulCompletedOrCanceledEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public global::System.OperationCanceledException Exception { get; }

            /// <summary>
            ///
            /// </summary>
            public SuccessfulCompletedOrCanceledEventArgs(global::System.OperationCanceledException exception)
            {
                Exception = exception;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out global::System.OperationCanceledException exception)
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

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
    {
        /// <summary>
        /// When canceled
        /// </summary>
        public event global::System.EventHandler? Canceled;

        /// <summary>
        /// A helper method to subscribe the Canceled event.
        /// </summary>
        public global::System.IDisposable SubscribeToCanceled(global::System.EventHandler handler)
        {
            Canceled += handler;

            return new global::H.WebSockets.EventSubscription(() => Canceled -= handler);
        }

        /// <summary>
        /// A helper method to raise the Canceled event.
        /// </summary>
        protected virtual global::System.EventArgs OnCanceled()
        {
            var args = new global::System.EventArgs();
            Canceled?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
    {
        /// <summary>
        /// When completed(with any result)
        /// </summary>
        public event global::System.EventHandler? Completed;

        /// <summary>
        /// A helper method to subscribe the Completed event.
        /// </summary>
        public global::System.IDisposable SubscribeToCompleted(global::System.EventHandler handler)
        {
            Completed += handler;

            return new global::H.WebSockets.EventSubscription(() => Completed -= handler);
        }

        /// <summary>
        /// A helper method to raise the Completed event.
        /// </summary>
        protected virtual global::System.EventArgs OnCompleted()
        {
            var args = new global::System.EventArgs();
            Completed?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
    {
        /// <summary>
        /// When a exception occurs(without OperationCanceledException's)
        /// </summary>
        public event global::System.EventHandler<global::H.WebSockets.Utilities.TaskWorker.ExceptionOccurredEventArgs>? ExceptionOccurred;

        /// <summary>
        /// A helper method to subscribe the ExceptionOccurred event.
        /// </summary>
        public global::System.IDisposable SubscribeToExceptionOccurred(global::System.EventHandler<global::H.WebSockets.Utilities.TaskWorker.ExceptionOccurredEventArgs> handler)
        {
            ExceptionOccurred += handler;

            return new global::H.WebSockets.EventSubscription(() => ExceptionOccurred -= handler);
        }

        /// <summary>
        /// A helper method to raise the ExceptionOccurred event.
        /// </summary>
        protected virtual global::H.WebSockets.Utilities.TaskWorker.ExceptionOccurredEventArgs OnExceptionOccurred(global::H.WebSockets.Utilities.TaskWorker.ExceptionOccurredEventArgs args)
        {
            ExceptionOccurred?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the ExceptionOccurred event.
        /// </summary>
        protected virtual global::H.WebSockets.Utilities.TaskWorker.ExceptionOccurredEventArgs OnExceptionOccurred(
            global::System.Exception exception)
        {
            var args = new global::H.WebSockets.Utilities.TaskWorker.ExceptionOccurredEventArgs(exception);
            ExceptionOccurred?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
    {
        /// <summary>
        /// When canceled or exceptions
        /// </summary>
        public event global::System.EventHandler<global::H.WebSockets.Utilities.TaskWorker.FailedOrCanceledEventArgs>? FailedOrCanceled;

        /// <summary>
        /// A helper method to subscribe the FailedOrCanceled event.
        /// </summary>
        public global::System.IDisposable SubscribeToFailedOrCanceled(global::System.EventHandler<global::H.WebSockets.Utilities.TaskWorker.FailedOrCanceledEventArgs> handler)
        {
            FailedOrCanceled += handler;

            return new global::H.WebSockets.EventSubscription(() => FailedOrCanceled -= handler);
        }

        /// <summary>
        /// A helper method to raise the FailedOrCanceled event.
        /// </summary>
        protected virtual global::H.WebSockets.Utilities.TaskWorker.FailedOrCanceledEventArgs OnFailedOrCanceled(global::H.WebSockets.Utilities.TaskWorker.FailedOrCanceledEventArgs args)
        {
            FailedOrCanceled?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the FailedOrCanceled event.
        /// </summary>
        protected virtual global::H.WebSockets.Utilities.TaskWorker.FailedOrCanceledEventArgs OnFailedOrCanceled(
            global::System.Exception exception)
        {
            var args = new global::H.WebSockets.Utilities.TaskWorker.FailedOrCanceledEventArgs(exception);
            FailedOrCanceled?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
    {
        /// <summary>
        /// When completed(without exceptions and cancellations)
        /// </summary>
        public event global::System.EventHandler? SuccessfulCompleted;

        /// <summary>
        /// A helper method to subscribe the SuccessfulCompleted event.
        /// </summary>
        public global::System.IDisposable SubscribeToSuccessfulCompleted(global::System.EventHandler handler)
        {
            SuccessfulCompleted += handler;

            return new global::H.WebSockets.EventSubscription(() => SuccessfulCompleted -= handler);
        }

        /// <summary>
        /// A helper method to raise the SuccessfulCompleted event.
        /// </summary>
        protected virtual global::System.EventArgs OnSuccessfulCompleted()
        {
            var args = new global::System.EventArgs();
            SuccessfulCompleted?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets.Utilities
{
    internal partial class TaskWorker
    {
        /// <summary>
        /// When completed(without exceptions)
        /// </summary>
        public event global::System.EventHandler<global::H.WebSockets.Utilities.TaskWorker.SuccessfulCompletedOrCanceledEventArgs>? SuccessfulCompletedOrCanceled;

        /// <summary>
        /// A helper method to subscribe the SuccessfulCompletedOrCanceled event.
        /// </summary>
        public global::System.IDisposable SubscribeToSuccessfulCompletedOrCanceled(global::System.EventHandler<global::H.WebSockets.Utilities.TaskWorker.SuccessfulCompletedOrCanceledEventArgs> handler)
        {
            SuccessfulCompletedOrCanceled += handler;

            return new global::H.WebSockets.EventSubscription(() => SuccessfulCompletedOrCanceled -= handler);
        }

        /// <summary>
        /// A helper method to raise the SuccessfulCompletedOrCanceled event.
        /// </summary>
        protected virtual global::H.WebSockets.Utilities.TaskWorker.SuccessfulCompletedOrCanceledEventArgs OnSuccessfulCompletedOrCanceled(global::H.WebSockets.Utilities.TaskWorker.SuccessfulCompletedOrCanceledEventArgs args)
        {
            SuccessfulCompletedOrCanceled?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the SuccessfulCompletedOrCanceled event.
        /// </summary>
        protected virtual global::H.WebSockets.Utilities.TaskWorker.SuccessfulCompletedOrCanceledEventArgs OnSuccessfulCompletedOrCanceled(
            global::System.OperationCanceledException exception)
        {
            var args = new global::H.WebSockets.Utilities.TaskWorker.SuccessfulCompletedOrCanceledEventArgs(exception);
            SuccessfulCompletedOrCanceled?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets
{
    public partial class WebSocketClient
    {
        /// <summary>
        ///
        /// </summary>
        public class BytesReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public global::System.Collections.Generic.IReadOnlyCollection<byte> Bytes { get; }

            /// <summary>
            ///
            /// </summary>
            public BytesReceivedEventArgs(global::System.Collections.Generic.IReadOnlyCollection<byte> bytes)
            {
                Bytes = bytes;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out global::System.Collections.Generic.IReadOnlyCollection<byte> bytes)
            {
                bytes = Bytes;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Bytes={Bytes})";
            }
        }
    }
}

#nullable enable

namespace H.WebSockets
{
    public partial class WebSocketClient
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

namespace H.WebSockets
{
    public partial class WebSocketClient
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

namespace H.WebSockets
{
    public partial class WebSocketClient
    {
        /// <summary>
        ///
        /// </summary>
        public class TextReceivedEventArgs : global::System.EventArgs
        {
            /// <summary>
            ///
            /// </summary>
            public string Text { get; }

            /// <summary>
            ///
            /// </summary>
            public TextReceivedEventArgs(string text)
            {
                Text = text;
            }

            /// <summary>
            ///
            /// </summary>
            public void Deconstruct(out string text)
            {
                text = Text;
            }

            /// <summary>
            ///
            /// </summary>
            public override string ToString()
            {
                return $"(Text={Text})";
            }
        }
    }
}

#nullable enable

namespace H.WebSockets
{
    public partial class WebSocketClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.WebSockets.WebSocketClient.BytesReceivedEventArgs>? BytesReceived;

        /// <summary>
        /// A helper method to subscribe the BytesReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToBytesReceived(global::System.EventHandler<global::H.WebSockets.WebSocketClient.BytesReceivedEventArgs> handler)
        {
            BytesReceived += handler;

            return new global::H.WebSockets.EventSubscription(() => BytesReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the BytesReceived event.
        /// </summary>
        private global::H.WebSockets.WebSocketClient.BytesReceivedEventArgs OnBytesReceived(global::H.WebSockets.WebSocketClient.BytesReceivedEventArgs args)
        {
            BytesReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the BytesReceived event.
        /// </summary>
        private global::H.WebSockets.WebSocketClient.BytesReceivedEventArgs OnBytesReceived(
            global::System.Collections.Generic.IReadOnlyCollection<byte> bytes)
        {
            var args = new global::H.WebSockets.WebSocketClient.BytesReceivedEventArgs(bytes);
            BytesReceived?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets
{
    public partial class WebSocketClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler? Connected;

        /// <summary>
        /// A helper method to subscribe the Connected event.
        /// </summary>
        public global::System.IDisposable SubscribeToConnected(global::System.EventHandler handler)
        {
            Connected += handler;

            return new global::H.WebSockets.EventSubscription(() => Connected -= handler);
        }

        /// <summary>
        /// A helper method to raise the Connected event.
        /// </summary>
        private global::System.EventArgs OnConnected()
        {
            var args = new global::System.EventArgs();
            Connected?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets
{
    public partial class WebSocketClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.WebSockets.WebSocketClient.DisconnectedEventArgs>? Disconnected;

        /// <summary>
        /// A helper method to subscribe the Disconnected event.
        /// </summary>
        public global::System.IDisposable SubscribeToDisconnected(global::System.EventHandler<global::H.WebSockets.WebSocketClient.DisconnectedEventArgs> handler)
        {
            Disconnected += handler;

            return new global::H.WebSockets.EventSubscription(() => Disconnected -= handler);
        }

        /// <summary>
        /// A helper method to raise the Disconnected event.
        /// </summary>
        private global::H.WebSockets.WebSocketClient.DisconnectedEventArgs OnDisconnected(global::H.WebSockets.WebSocketClient.DisconnectedEventArgs args)
        {
            Disconnected?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the Disconnected event.
        /// </summary>
        private global::H.WebSockets.WebSocketClient.DisconnectedEventArgs OnDisconnected(
            string reason,
            global::System.Net.WebSockets.WebSocketCloseStatus? status)
        {
            var args = new global::H.WebSockets.WebSocketClient.DisconnectedEventArgs(reason, status);
            Disconnected?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets
{
    public partial class WebSocketClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.WebSockets.WebSocketClient.ExceptionOccurredEventArgs>? ExceptionOccurred;

        /// <summary>
        /// A helper method to subscribe the ExceptionOccurred event.
        /// </summary>
        public global::System.IDisposable SubscribeToExceptionOccurred(global::System.EventHandler<global::H.WebSockets.WebSocketClient.ExceptionOccurredEventArgs> handler)
        {
            ExceptionOccurred += handler;

            return new global::H.WebSockets.EventSubscription(() => ExceptionOccurred -= handler);
        }

        /// <summary>
        /// A helper method to raise the ExceptionOccurred event.
        /// </summary>
        private global::H.WebSockets.WebSocketClient.ExceptionOccurredEventArgs OnExceptionOccurred(global::H.WebSockets.WebSocketClient.ExceptionOccurredEventArgs args)
        {
            ExceptionOccurred?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the ExceptionOccurred event.
        /// </summary>
        private global::H.WebSockets.WebSocketClient.ExceptionOccurredEventArgs OnExceptionOccurred(
            global::System.Exception exception)
        {
            var args = new global::H.WebSockets.WebSocketClient.ExceptionOccurredEventArgs(exception);
            ExceptionOccurred?.Invoke(this, args);

            return args;
        }
    }
}

#nullable enable

namespace H.WebSockets
{
    public partial class WebSocketClient
    {
        /// <summary>
        /// </summary>
        public event global::System.EventHandler<global::H.WebSockets.WebSocketClient.TextReceivedEventArgs>? TextReceived;

        /// <summary>
        /// A helper method to subscribe the TextReceived event.
        /// </summary>
        public global::System.IDisposable SubscribeToTextReceived(global::System.EventHandler<global::H.WebSockets.WebSocketClient.TextReceivedEventArgs> handler)
        {
            TextReceived += handler;

            return new global::H.WebSockets.EventSubscription(() => TextReceived -= handler);
        }

        /// <summary>
        /// A helper method to raise the TextReceived event.
        /// </summary>
        private global::H.WebSockets.WebSocketClient.TextReceivedEventArgs OnTextReceived(global::H.WebSockets.WebSocketClient.TextReceivedEventArgs args)
        {
            TextReceived?.Invoke(this, args);

            return args;
        }

        /// <summary>
        /// A helper method to raise the TextReceived event.
        /// </summary>
        private global::H.WebSockets.WebSocketClient.TextReceivedEventArgs OnTextReceived(
            string text)
        {
            var args = new global::H.WebSockets.WebSocketClient.TextReceivedEventArgs(text);
            TextReceived?.Invoke(this, args);

            return args;
        }
    }
}
