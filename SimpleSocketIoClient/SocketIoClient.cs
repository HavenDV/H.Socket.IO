using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleSocketIoClient.EngineIO;
using SimpleSocketIoClient.EventsArgs;
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

        /// <summary>
        /// Optional property which is used when sending a message
        /// </summary>
        public string? DefaultNamespace { get; set; }

        private Dictionary<string, List<(Action<object?, string?> Action, Type Type)>>? Actions { get; } = new Dictionary<string, List<(Action<object?, string?> Action, Type Type)>>();

        #endregion

        #region Events

        /// <summary>
        /// Occurs after a successful connection to each namespace
        /// </summary>
        public event EventHandler<SocketIoEventEventArgs>? Connected;

        /// <summary>
        /// Occurs after a disconnection.
        /// </summary>
        public event EventHandler<WebSocketCloseEventArgs>? Disconnected;

        /// <summary>
        /// Occurs after new event.
        /// </summary>
        public event EventHandler<SocketIoEventEventArgs>? AfterEvent;

        /// <summary>
        /// Occurs after new unhandled event(not captured by any On).
        /// </summary>
        public event EventHandler<SocketIoEventEventArgs>? AfterUnhandledEvent;

        /// <summary>
        /// Occurs after new exception.
        /// </summary>
        public event EventHandler<DataEventArgs<Exception>>? AfterException;

        private void OnConnected(string value)
        {
            Connected?.Invoke(this, new SocketIoEventEventArgs(string.Empty, value));
        }

        private void OnDisconnected(string? reason, WebSocketCloseStatus? status)
        {
            Disconnected?.Invoke(this, new WebSocketCloseEventArgs(reason, status));
        }

        private void OnAfterEvent(string value, string @namespace)
        {
            AfterEvent?.Invoke(this, new SocketIoEventEventArgs(value, @namespace));
        }

        private void OnAfterUnhandledEvent(string value, string @namespace)
        {
            AfterUnhandledEvent?.Invoke(this, new SocketIoEventEventArgs(value, @namespace));
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
            EngineIoClient.Closed += (sender, args) => OnDisconnected(args.Reason, args.Status);
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
                var @namespace = "/";
                var value = args.Value.Substring(1);

                if (args.Value.ElementAtOrDefault(1) == '/')
                {
                    var index = args.Value.IndexOf(',');
                    @namespace = index >= 0
                        ? args.Value.Substring(1, index - 1)
                        : args.Value.Substring(1);
                    value = index >= 0
                        ? args.Value.Substring(index + 1)
                        : string.Empty;
                }

                switch (prefix)
                {
                    case ConnectPrefix:
                        OnConnected(@namespace);
                        break;

                    case DisconnectPrefix:
                        OnDisconnected("Received disconnect message from server", null);
                        break;

                    case EventPrefix:
                        {
                            OnAfterEvent(value, @namespace);

                            if (Actions == null)
                            {
                                break;
                            }
                            try
                            {
                                var values = value.GetEventValues();
                                var name = values.ElementAtOrDefault(0);
                                var text = values.ElementAtOrDefault(1);

                                if (name == null)
                                {
                                    break;
                                }

                                if (Actions.TryGetValue($"{name}{@namespace}", out var actions))
                                {
                                    foreach (var (action, type) in actions)
                                    {
                                        try
                                        {
                                            var obj = text == null
                                                ? null
                                                : JsonConvert.DeserializeObject(text, type);

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
                                    OnAfterUnhandledEvent(value, @namespace);
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


        #endregion

        #region Public methods

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, CancellationToken cancellationToken = default, params string[] namespaces)
        {
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            if (!EngineIoClient.IsOpened && !await this.WaitEventAsync(async token =>
            {
                await EngineIoClient.OpenAsync(uri, token);
            }, nameof(Connected), cancellationToken))
            {
                return false;
            }

            return await ConnectToNamespacesAsync(cancellationToken, DefaultNamespace != null
                ? namespaces.Concat(new []{ DefaultNamespace }).Distinct().ToArray()
                : namespaces);
        }

        /// <summary>
        /// It connects to selected namespaces and asynchronously waits for a connection message.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public async Task<bool> ConnectToNamespacesAsync(CancellationToken cancellationToken = default, params string[] namespaces)
        {
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            if (!EngineIoClient.IsOpened)
            {
                return false;
            }

            if (!namespaces.Any())
            {
                return true;
            }

            return await this.WaitEventAsync(async token =>
            {
                foreach (var @namespace in namespaces)
                {
                    await EngineIoClient.SendMessageAsync($"0/{@namespace?.TrimStart('/')}", token);
                }
            }, nameof(Connected), cancellationToken);
        }

        /// <summary>
        /// It connects to selected namespaces and asynchronously waits for a connection message.
        /// </summary>
        /// <param name="customNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ConnectToNamespaceAsync(string customNamespace, CancellationToken cancellationToken = default)
        {
            customNamespace = customNamespace ?? throw new ArgumentNullException(nameof(customNamespace));
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            return await ConnectToNamespacesAsync(cancellationToken, customNamespace);
        }

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message with the selected timeout.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, TimeSpan timeout, params string[] namespaces)
        {
            using var cancellationSource = new CancellationTokenSource(timeout);

            return await ConnectAsync(uri, cancellationSource.Token, namespaces);
        }

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message with the selected timeout.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <param name="namespaces"></param>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, int timeoutInSeconds, params string[] namespaces)
        {
            return await ConnectAsync(uri, TimeSpan.FromSeconds(timeoutInSeconds), namespaces);
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
        /// <param name="customNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendEventAsync(string message, string? customNamespace = null, CancellationToken cancellationToken = default)
        {
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            customNamespace ??= DefaultNamespace;
            var namespaceBody = customNamespace == null ? string.Empty : $"/{customNamespace.TrimStart('/')}";
            namespaceBody += !string.IsNullOrWhiteSpace(namespaceBody) && !string.IsNullOrWhiteSpace(message)
                ? ","
                : "";

            await EngineIoClient.SendMessageAsync($"{EventPrefix}{namespaceBody}{message}", cancellationToken);
        }

        /// <summary>
        /// Sends a new event where name is the name of the event <br/>
        /// the object can be <see langword="string"/> - so it will be send as simple message <br/>
        /// any other will be serialized to json.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="customNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Emit(string name, object? value = null, string? customNamespace = null, CancellationToken cancellationToken = default)
        {
            var messages = value switch
            {
                null           => new[] {$"\"{name}\""},
                string message => new[] {$"\"{name}\"", $"\"{message}\""},
                _              => new[] {$"\"{name}\"", JsonConvert.SerializeObject(value)},
            };

            await SendEventAsync($"[{string.Join(",", messages)}]", customNamespace, cancellationToken);
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <paramref name="action"/>.<typeparamref name="T"/> is a json deserialized object, <paramref name="action"/>.<see langword="string"/> is raw text.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="customNamespace"></param>
        public void On<T>(string name, Action<T?, string?> action, string? customNamespace = null) where T : class
        {
            if (Actions == null)
            {
                return;
            }

            var key = $"{name}{customNamespace ?? "/"}";
            if (!Actions.ContainsKey(key))
            {
                Actions[key] = new List<(Action<object?, string?> Action, Type Type)>();
            }

            Actions[key].Add(((obj, text) => action?.Invoke((T?)obj, text), typeof(T)));
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <paramref name="action"/>.<typeparamref name="T"/> is a json deserialized object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="customNamespace"></param>
        public void On<T>(string name, Action<T?> action, string? customNamespace = null) where T : class
        {
            On<T>(name, (obj, text) => action?.Invoke(obj), customNamespace);
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <paramref name="action"/>.<see langword="string"/> is a raw text.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="customNamespace"></param>
        public void On(string name, Action<string?> action, string? customNamespace = null)
        {
            On<object>(name, (obj, text) => action?.Invoke(text), customNamespace);
        }

        /// <summary>
        /// Performs an action after receiving a specific event.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="customNamespace"></param>
        public void On(string name, Action action, string? customNamespace = null)
        {
            On<object>(name, (obj, text) => action?.Invoke(), customNamespace);
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
