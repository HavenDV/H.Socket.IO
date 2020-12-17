using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using H.Engine.IO;
using H.Socket.IO.EventsArgs;
using H.Socket.IO.Utilities;
using H.WebSockets.Args;
using H.WebSockets.Utilities;
using Newtonsoft.Json;

namespace H.Socket.IO
{
    /// <summary>
    /// Socket.IO Client.
    /// </summary>
    public sealed class SocketIoClient : IDisposable
#if NETSTANDARD2_1
        , IAsyncDisposable
#endif
    {
        #region Properties

        /// <summary>
        /// Internal Engine.IO Client.
        /// </summary>
        public EngineIoClient EngineIoClient { get; private set; }

        /// <summary>
        /// Using proxy.
        /// </summary>
        public IWebProxy? Proxy {
            get => EngineIoClient.Proxy;
            set {
                EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));
                EngineIoClient.Proxy = value;
            }
        }

        /// <summary>
        /// Will be sent with all messages(Unless otherwise stated). <br/>
        /// Also automatically connects to it. <br/>
        /// Default is <see langword="null"/>.
        /// </summary>
        public string? DefaultNamespace { get; set; }

        private Dictionary<string, List<(Action<object, string> Action, Type Type)>> JsonDeserializeActions { get; } = new ();

        private Dictionary<string, List<Action<string>>> TextActions { get; } = new ();

        private Dictionary<string, List<Action>> Actions { get; } = new ();

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
        public event EventHandler<SocketIoEventEventArgs>? EventReceived;

        /// <summary>
        /// Occurs after new handled event(captured by any On).
        /// </summary>
        public event EventHandler<SocketIoEventEventArgs>? HandledEventReceived;

        /// <summary>
        /// Occurs after new unhandled event(not captured by any On).
        /// </summary>
        public event EventHandler<SocketIoEventEventArgs>? UnhandledEventReceived;

        /// <summary>
        /// Occurs after new error.
        /// </summary>
        public event EventHandler<SocketIoErrorEventArgs>? ErrorReceived;

        /// <summary>
        /// Occurs after new exception.
        /// </summary>
        public event EventHandler<DataEventArgs<Exception>>? ExceptionOccurred;

        private void OnConnected(string value)
        {
            Connected?.Invoke(this, new SocketIoEventEventArgs(string.Empty, value, false));
        }

        private void OnDisconnected(string reason, WebSocketCloseStatus? status)
        {
            Disconnected?.Invoke(this, new WebSocketCloseEventArgs(reason, status));
        }

        private void OnEventReceived(string value, string @namespace, bool isHandled)
        {
            EventReceived?.Invoke(this, new SocketIoEventEventArgs(value, @namespace, isHandled));

            if (isHandled)
            {
                HandledEventReceived?.Invoke(this, new SocketIoEventEventArgs(value, @namespace, true));
            }
            else
            {
                UnhandledEventReceived?.Invoke(this, new SocketIoEventEventArgs(value, @namespace, false));
            }
        }

        private void OnErrorReceived(string value, string @namespace)
        {
            ErrorReceived?.Invoke(this, new SocketIoErrorEventArgs(value, @namespace));
        }

        private void OnExceptionOccurred(Exception value)
        {
            ExceptionOccurred?.Invoke(this, new DataEventArgs<Exception>(value));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates Engine.IO client internally.
        /// </summary>
        public SocketIoClient()
        {
            EngineIoClient = new EngineIoClient("socket.io");
            EngineIoClient.MessageReceived += EngineIoClient_MessageReceived;
            EngineIoClient.ExceptionOccurred += (_, args) => OnExceptionOccurred(args.Value);
            EngineIoClient.Closed += (_, args) => OnDisconnected(args.Reason, args.Status);
        }

        #endregion

        #region Event Handlers

        private void EngineIoClient_MessageReceived(object? sender, DataEventArgs<string>? args)
        {
            try
            {
                if (args?.Value == null)
                {
                    throw new InvalidOperationException("Engine.IO message is null");
                }

                if (args.Value.Length < 1)
                {
                    // ignore - it's Engine.IO message
                    return;
                }

                var packet = SocketIoPacket.Decode(args.Value);
                switch (packet.Prefix)
                {
                    case SocketIoPacket.ConnectPrefix:
                        OnConnected(packet.Namespace);
                        break;

                    case SocketIoPacket.DisconnectPrefix:
                        OnDisconnected("Received disconnect message from server", null);
                        break;

                    case SocketIoPacket.EventPrefix:
                        var isHandled = false;
                        try
                        {
                            if (string.IsNullOrWhiteSpace(packet.Value))
                            {
                                break;
                            }

                            var values = packet.Value.GetJsonArrayValues();
                            var name = values.ElementAt(0);
                            var text = values.ElementAtOrDefault(1);

                            var key = GetOnKey(name, packet.Namespace);
                            if (Actions.TryGetValue(key, out var actions))
                            {
                                foreach (var action in actions)
                                {
                                    isHandled = true;

                                    try
                                    {
                                        action?.Invoke();
                                    }
                                    catch (Exception exception)
                                    {
                                        OnExceptionOccurred(exception);
                                    }
                                }
                            }

                            if (TextActions.TryGetValue(key, out var textActions))
                            {
                                foreach (var action in textActions)
                                {
                                    isHandled = true;

                                    try
                                    {
                                        if (text == null)
                                        {
                                            throw new InvalidOperationException($"Received json text for event named \"{name}\" is null");
                                        }

                                        action?.Invoke(text);
                                    }
                                    catch (Exception exception)
                                    {
                                        OnExceptionOccurred(exception);
                                    }
                                }
                            }

                            if (JsonDeserializeActions.TryGetValue(key, out var jsonDeserializeActions))
                            {
                                foreach (var (action, type) in jsonDeserializeActions)
                                {
                                    isHandled = true;

                                    try
                                    {
                                        if (text == null)
                                        {
                                            throw new InvalidOperationException($"Received json text for event named \"{name}\" is null");
                                        }

                                        var obj = JsonConvert.DeserializeObject(text, type);
                                        if (obj == null)
                                        {
                                            throw new InvalidOperationException($"Deserialized object for json text(\"{text}\") and for event named \"{name}\" is null");
                                        }

                                        action(obj, text);
                                    }
                                    catch (Exception exception)
                                    {
                                        OnExceptionOccurred(exception);
                                    }
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            OnExceptionOccurred(exception);
                        }
                        finally
                        {
                            OnEventReceived(packet.Value, packet.Namespace, isHandled);
                        }
                        break;

                    case SocketIoPacket.ErrorPrefix:
                        OnErrorReceived(packet.Value.Trim('\"'), packet.Namespace);
                        break;
                }
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        #endregion

        #region Private methods

        private static string GetOnKey(string name, string? customNamespace = null)
        {
            return $"{name}{customNamespace ?? "/"}";
        }

        #endregion

        #region Public methods

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="namespaces"></param>
        /// <exception cref="InvalidOperationException">if AfterError event occurs while wait connect message</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, CancellationToken cancellationToken = default, params string[] namespaces)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            if (!EngineIoClient.IsOpened)
            {
                var results = await this.WaitAnyEventAsync<EventArgs>(async () =>
                {
                    await EngineIoClient.OpenAsync(uri, cancellationToken).ConfigureAwait(false);
                }, cancellationToken, nameof(Connected), nameof(ErrorReceived)).ConfigureAwait(false);

                if (results[nameof(ErrorReceived)] is SocketIoErrorEventArgs error)
                {
                    throw new InvalidOperationException($"Socket.IO returns error: {error.Value}");
                }
                if (results[nameof(Connected)] == null)
                {
                    return false;
                }
            }

            return await ConnectToNamespacesAsync(cancellationToken, DefaultNamespace != null
                ? namespaces.Concat(new[] { DefaultNamespace }).Distinct().ToArray()
                : namespaces).ConfigureAwait(false);
        }

        /// <summary>
        /// It connects to selected namespaces and asynchronously waits for a connection message.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="namespaces"></param>
        /// <exception cref="ObjectDisposedException"></exception>
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

            return await this.WaitEventAsync<SocketIoEventEventArgs>(async () =>
            {
                foreach (var @namespace in namespaces)
                {
                    var packet = new SocketIoPacket(SocketIoPacket.ConnectPrefix, @namespace: @namespace);

                    await EngineIoClient.SendMessageAsync(packet.Encode(), cancellationToken).ConfigureAwait(false);
                }
            }, nameof(Connected), cancellationToken).ConfigureAwait(false) != null;
        }

        /// <summary>
        /// It connects to selected namespaces and asynchronously waits for a connection message.
        /// </summary>
        /// <param name="customNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <returns></returns>
        public async Task<bool> ConnectToNamespaceAsync(string customNamespace, CancellationToken cancellationToken = default)
        {
            customNamespace = customNamespace ?? throw new ArgumentNullException(nameof(customNamespace));
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            return await ConnectToNamespacesAsync(cancellationToken, customNamespace).ConfigureAwait(false);
        }

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message with the selected timeout. <br/>
        /// Throws <see cref="OperationCanceledException"/> if the waiting time exceeded
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <param name="namespaces"></param>
        /// <exception cref="InvalidOperationException">if AfterError event occurs while wait connect message</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, TimeSpan timeout, params string[] namespaces)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));

            using var cancellationSource = new CancellationTokenSource(timeout);

            return await ConnectAsync(uri, cancellationSource.Token, namespaces).ConfigureAwait(false);
        }

        /// <summary>
        /// It connects to the server and asynchronously waits for a connection message with the selected timeout. <br/>
        /// Throws <see cref="OperationCanceledException"/> if the waiting time exceeded
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <param name="namespaces"></param>
        /// <exception cref="InvalidOperationException">if AfterError event occurs while wait connect message</exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public async Task<bool> ConnectAsync(Uri uri, int timeoutInSeconds, params string[] namespaces)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));

            return await ConnectAsync(uri, TimeSpan.FromSeconds(timeoutInSeconds), namespaces).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a disconnect message and closes the connection.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <returns></returns>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            if (DefaultNamespace != null)
            {
                var packet = new SocketIoPacket(SocketIoPacket.DisconnectPrefix, @namespace: DefaultNamespace);

                await EngineIoClient.SendMessageAsync(packet.Encode(), cancellationToken).ConfigureAwait(false);
            }

            {
                var packet = new SocketIoPacket(SocketIoPacket.DisconnectPrefix);

                await EngineIoClient.SendMessageAsync(packet.Encode(), cancellationToken).ConfigureAwait(false);
            }

            await EngineIoClient.CloseAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends a new raw message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="customNamespace"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <returns></returns>
        public async Task SendEventAsync(string message, string? customNamespace = null, CancellationToken cancellationToken = default)
        {
            EngineIoClient = EngineIoClient ?? throw new ObjectDisposedException(nameof(EngineIoClient));

            var packet = new SocketIoPacket(SocketIoPacket.EventPrefix, message, customNamespace ?? DefaultNamespace);

            await EngineIoClient.SendMessageAsync(packet.Encode(), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the next event or error asynchronously <br/>
        /// Returns <see cref="SocketIoEventEventArgs"/> if event was received <br/>
        /// Returns <see cref="SocketIoErrorEventArgs"/> if error was received <br/>
        /// Returns null if no event was received and the method was canceled <br/>
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<SocketIoEventArgs?> WaitEventOrErrorAsync(Func<Task>? func = null, CancellationToken cancellationToken = default)
        {
            var dictionary = await this.WaitAnyEventAsync<SocketIoEventArgs?>(
                func ?? (() => Task.FromResult(false)),
                cancellationToken,
                nameof(EventReceived), nameof(ErrorReceived))
                .ConfigureAwait(false);

            return dictionary[nameof(EventReceived)] ?? 
                   dictionary[nameof(ErrorReceived)];
        }

        /// <summary>
        /// Waits for the next event or error asynchronously with specified timeout <br/>
        /// Returns <see cref="SocketIoEventEventArgs"/> if event was received <br/>
        /// Returns <see cref="SocketIoErrorEventArgs"/> if error was received <br/>
        /// Returns null if no event was received and the method was canceled <br/>
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public async Task<SocketIoEventArgs?> WaitEventOrErrorAsync(TimeSpan timeout, Func<Task>? func = null)
        {
            using var tokenSource = new CancellationTokenSource(timeout);

            return await WaitEventOrErrorAsync(func, tokenSource.Token)
                .ConfigureAwait(false);
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
                null           => new[] { $"\"{name}\"" },
                string message => new[] { $"\"{name}\"", $"\"{message}\"" },
                _              => new[] { $"\"{name}\"", JsonConvert.SerializeObject(value) },
            };

            await SendEventAsync($"[{string.Join(",", messages)}]", customNamespace, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <br/>
        /// <paramref name="action"/>.<typeparamref name="T"/> is a json deserialized object, <br/>
        /// <paramref name="action"/>.<see langword="string"/> is raw text.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="customNamespace"></param>
        public void On<T>(string name, Action<T, string> action, string? customNamespace = null) where T : class
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            action = action ?? throw new ArgumentNullException(nameof(action));

            var key = GetOnKey(name, customNamespace);
            if (!JsonDeserializeActions.ContainsKey(key))
            {
                JsonDeserializeActions[key] = new List<(Action<object, string> Action, Type Type)>();
            }

            JsonDeserializeActions[key].Add(((obj, text) => action((T)obj, text), typeof(T)));
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <br/>
        /// <paramref name="action"/>.<typeparamref name="T"/> is a json deserialized object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="customNamespace"></param>
        public void On<T>(string name, Action<T> action, string? customNamespace = null) where T : class
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            action = action ?? throw new ArgumentNullException(nameof(action));

            On<T>(name, (obj, _) => action(obj), customNamespace);
        }

        /// <summary>
        /// Performs an action after receiving a specific event. <br/>
        /// <paramref name="action"/>.<see langword="string"/> is a raw text.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="customNamespace"></param>
        public void On(string name, Action<string> action, string? customNamespace = null)
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            action = action ?? throw new ArgumentNullException(nameof(action));

            var key = GetOnKey(name, customNamespace);
            if (!TextActions.ContainsKey(key))
            {
                TextActions[key] = new List<Action<string>>();
            }

            TextActions[key].Add(action);
        }

        /// <summary>
        /// Performs an action after receiving a specific event.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="customNamespace"></param>
        public void On(string name, Action action, string? customNamespace = null)
        {
            name = name ?? throw new ArgumentNullException(nameof(name));
            action = action ?? throw new ArgumentNullException(nameof(action));

            var key = GetOnKey(name, customNamespace);
            if (!Actions.ContainsKey(key))
            {
                Actions[key] = new List<Action>();
            }

            Actions[key].Add(action);
        }

        /// <summary>
        /// Disposes an object. <br/>
        /// Prefer DisposeAsync if possible
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            EngineIoClient.Dispose();
        }

#if NETSTANDARD2_1
        /// <summary>
        /// Asynchronously disposes an object.
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await EngineIoClient.DisposeAsync().ConfigureAwait(false);
        }
#endif

        #endregion
    }
}
