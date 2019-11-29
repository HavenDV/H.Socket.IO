using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient
{
    /// <summary>
    /// Socket.IO Client.
    /// </summary>
    public sealed class SocketIoClient :
#if NETSTANDARD2_1
        IAsyncDisposable
#else
        IDisposable
#endif
    {
        #region Constants

        private const string ConnectPrefix = "0";
        private const string DisconnectPrefix = "1";
        private const string EventPrefix = "2";
        //private const string AckPrefix = "3";
        //private const string ErrorPrefix = "4";
        //private const string BinaryEventPrefix = "5";
        //private const string BinaryAckPrefix = "6";

        #endregion

        #region Properties

        /// <summary>
        /// Internal Engine.IO Client.
        /// </summary>
        public EngineIoClient? EngineIoClient { get; private set; }

        /// <summary>
        /// Using proxy.
        /// </summary>
        public IWebProxy? Proxy
        {
            get => EngineIoClient?.Proxy;
            set
            {
                EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));
                EngineIoClient.Proxy = value;
            }
        }

        private Dictionary<string, List<(Action<object?, string> Action, Type Type)>>? Actions { get; } = new Dictionary<string, List<(Action<object?, string> Action, Type Type)>>();

        #endregion

        #region Events

        /// <summary>
        /// Occurs after a successful connection.
        /// </summary>
        public event EventHandler<EventArgs>? Connected;

        /// <summary>
        /// Occurs after a disconnection.
        /// </summary>
        public event EventHandler<DataEventArgs<(string Reason, WebSocketCloseStatus? Status)>>? Disconnected;

        /// <summary>
        /// Occurs after new event.
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? AfterEvent;

        /// <summary>
        /// Occurs after new unhandled event(not captured by any On).
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? AfterUnhandledEvent;

        /// <summary>
        /// Occurs after new exception.
        /// </summary>
        public event EventHandler<DataEventArgs<Exception>>? AfterException;

        private void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisconnected((string Reason, WebSocketCloseStatus? Status) value)
        {
            Disconnected?.Invoke(this, new DataEventArgs<(string Reason, WebSocketCloseStatus? Status)>(value));
        }

        private void OnAfterEvent(string value)
        {
            AfterEvent?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnAfterUnhandledEvent(string value)
        {
            AfterUnhandledEvent?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnAfterException(Exception value)
        {
            AfterException?.Invoke(this, new DataEventArgs<Exception>(value));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates Engine.IO client internally.
        /// </summary>
        public SocketIoClient()
        {
            EngineIoClient = new EngineIoClient("socket.io");
            EngineIoClient.AfterMessage += EngineIoClient_AfterMessage;
            EngineIoClient.AfterException += (sender, args) => OnAfterException(args.Value);
            EngineIoClient.Closed += (sender, args) => OnDisconnected(args.Value);
        }

        #endregion

        #region Event Handlers

        private void EngineIoClient_AfterMessage(object sender, DataEventArgs<string> args)
        {
            try
            {
                if (args.Value == null)
                {
                    throw new InvalidDataException("Engine.IO message is null");
                }

                if (args.Value.Length < 1)
                {
                    // ignore - it's Engine.IO message
                    return;
                }

                var prefix = args.Value.Substring(0, 1);
                var value = args.Value.Substring(1);

                switch (prefix)
                {
                    case ConnectPrefix:
                        OnConnected();
                        break;

                    case DisconnectPrefix:
                        OnDisconnected(("Received disconnect message from server", null));
                        break;

                    case EventPrefix:
                        {
                            OnAfterEvent(value);

                            if (Actions == null)
                            {
                                break;
                            }
                            try
                            {
                                var name = value.Extract("[\"", "\"");
                                var text = value.Extract(",")?.TrimEnd(']');

                                if (name == null || text == null)
                                {
                                    break;
                                }

                                if (Actions.TryGetValue(name, out var actions))
                                {
                                    foreach (var (action, type) in actions)
                                    {
                                        try
                                        {
                                            var obj = JsonConvert.DeserializeObject(text, type);

                                            action?.Invoke(obj, text);
                                        }
                                        catch (Exception exception)
                                        {
                                            OnAfterException(exception);
                                        }
                                    }
                                }
                                else
                                {
                                    OnAfterUnhandledEvent(value);
                                }
                            }
                            catch (Exception exception)
                            {
                                OnAfterException(exception);
                            }
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                OnAfterException(exception);
            }
        }

        #endregion

        #region Private methods

        private async Task EmitInternal(CancellationToken cancellationToken = default, params string[] messages)
        {
            await SendEventAsync($"[{string.Join(",", messages)}]", cancellationToken);
        }

        #endregion

        #region Public methods

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            return await this.WaitEventAsync(async token =>
            {
                await EngineIoClient.OpenAsync(uri, token);
            }, nameof(Connected), cancellationToken);
        }

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message with the selected timeout.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, TimeSpan timeout)
        {
            using var cancellationSource = new CancellationTokenSource(timeout);

            return await ConnectAsync(uri, cancellationSource.Token);
        }

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message with the selected timeout.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, int timeoutInSeconds)
        {
            return await ConnectAsync(uri, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        /// <summary>
        /// Sends a disconnect message and closes the connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            await EngineIoClient.SendMessageAsync(DisconnectPrefix, cancellationToken);

            await EngineIoClient.CloseAsync(cancellationToken);
        }

        /// <summary>
        /// Sends a new raw message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendEventAsync(string message, CancellationToken cancellationToken = default)
        {
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            await EngineIoClient.SendMessageAsync($"{EventPrefix}{message}", cancellationToken);
        }

        /// <summary>
        /// Sends a new event where name is the name of the event.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Emit(string name, CancellationToken cancellationToken = default)
        {
            await EmitInternal(cancellationToken, $"\"{name}\"");
        }

        /// <summary>
        /// Sends a new event with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Emit(string name, string message, CancellationToken cancellationToken = default)
        {
            await EmitInternal(cancellationToken, $"\"{name}\"", $"\"{message}\"");
        }

        /// <summary>
        /// Sends a new event where name is the name of the event and the object is serialized in json.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Emit(string name, object value, CancellationToken cancellationToken = default)
        {
            var json = JsonConvert.SerializeObject(value);

            await EmitInternal(cancellationToken, $"\"{name}\"", json);
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <paramref name="action"/>.<typeparamref name="T"/> is a json deserialized object, <paramref name="action"/>.<see langword="string"/> is raw text.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void On<T>(string name, Action<T?, string> action) where T : class
        {
            if (Actions == null)
            {
                return;
            }

            if (!Actions.ContainsKey(name))
            {
                Actions[name] = new List<(Action<object?, string> Action, Type Type)>();
            }

            Actions[name].Add(((obj, text) => action?.Invoke((T?)obj, text), typeof(T)));
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <paramref name="action"/>.<typeparamref name="T"/> is a json deserialized object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void On<T>(string name, Action<T?> action) where T : class
        {
            On<T>(name, (obj, text) => action?.Invoke(obj));
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <paramref name="action"/>.<see langword="string"/> is a raw text.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void On(string name, Action<string> action)
        {
            On<object>(name, (obj, text) => action?.Invoke(text));
        }

        /// <summary>
        /// Performs an action after receiving a specific event.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void On(string name, Action action)
        {
            On<object>(name, (obj, text) => action?.Invoke());
        }

#if NETSTANDARD2_1
        /// <summary>
        /// Asynchronously disposes an object.
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (EngineIoClient != null)
            {
                await EngineIoClient.DisposeAsync();
                EngineIoClient = null;
            }
        }
#else
        /// <summary>
        /// Disposes an object.
        /// </summary>
        public void Dispose()
        {
            EngineIoClient?.Dispose();
            EngineIoClient = null;
        }
#endif

        #endregion
    }
}
