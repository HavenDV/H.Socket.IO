using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient
{
    public sealed class SocketIoClient :
#if NETSTANDARD2_1
        IAsyncDisposable 
#else
        IDisposable
#endif
    {
        #region Constants

        public const string ConnectPrefix = "0";
        public const string DisconnectPrefix = "1";
        public const string EventPrefix = "2";
        public const string AckPrefix = "3";
        public const string ErrorPrefix = "4";
        public const string BinaryEventPrefix = "5";
        public const string BinaryAckPrefix = "6";

        public const string Message = "message";

        #endregion

        #region Properties

        public EngineIoClient EngineIoClient { get; private set; } = new EngineIoClient("socket.io");

        public IWebProxy Proxy {
            get => EngineIoClient.Proxy;
            set => EngineIoClient.Proxy = value;
        }

        private Dictionary<string, List<(Action<object, string> Action, Type Type)>> Actions { get; } = new Dictionary<string, List<(Action<object, string> Action, Type Type)>>();

        #endregion

        #region Events

        public event EventHandler<EventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;
        public event EventHandler<DataEventArgs<string>> AfterEvent;

        public event EventHandler<DataEventArgs<Exception>> AfterException;

        private void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        private void OnAfterEvent(string value)
        {
            AfterEvent?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnAfterException(Exception value)
        {
            AfterException?.Invoke(this, new DataEventArgs<Exception>(value));
        }

        #endregion

        #region Constructors

        public SocketIoClient()
        {
            EngineIoClient.AfterMessage += EngineIoClient_AfterMessage;
            EngineIoClient.AfterException += (sender, args) => OnAfterException(args.Value);
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
                        OnDisconnected();
                        break;

                    case EventPrefix:
                        {
                            OnAfterEvent(value);

                            try
                            {
                                var name = value.Extract("[\"", "\"");
                                var text = value.Extract(",").TrimEnd(']');

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

        #region Public methods

        public async Task<bool> ConnectAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            var source = new TaskCompletionSource<bool>();
            using var cancellationSource = new CancellationTokenSource();

            cancellationSource.Token.Register(() => source.TrySetCanceled(), false);
            cancellationToken.Register(() => source.TrySetCanceled(), false);

            void OnConnected(object sender, EventArgs args)
            {
                source.TrySetResult(true);
            }

            try
            {
                Connected += OnConnected;

                if (!await EngineIoClient.OpenAsync(uri, cancellationToken))
                {
                    return false;
                }

                return await source.Task;
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                Connected -= OnConnected;
            }

            return false;
        }

        public async Task<bool> ConnectAsync(Uri uri, TimeSpan timeout)
        {
            using var cancellationSource = new CancellationTokenSource(timeout);

            return await ConnectAsync(uri, cancellationSource.Token);
        }

        public async Task<bool> ConnectAsync(Uri uri, int timeoutInSeconds)
        {
            return await ConnectAsync(uri, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            await EngineIoClient.CloseAsync(cancellationToken);
        }

        public async Task SendEventAsync(string message, CancellationToken token = default)
        {
            await EngineIoClient.SendMessageAsync($"{EventPrefix}{message}", token);
        }

        public async Task Emit(string name, string message, CancellationToken token = default)
        {
            await SendEventAsync($"[\"{name}\",{message}]", token);
        }

        public async Task Emit(string name, object value, CancellationToken token = default)
        {
            var message = JsonConvert.SerializeObject(value);

            await Emit(name, message, token);
        }

        public async Task EmitMessage(object value, CancellationToken token = default)
        {
            await Emit(Message, value, token);
        }

        public async Task EmitMessage(string message, CancellationToken token = default)
        {
            await Emit(Message, message, token);
        }

        public void On<T>(string name, Action<T, string> action)
        {
            if (!Actions.ContainsKey(name))
            {
                Actions[name] = new List<(Action<object, string> Action, Type Type)>();
            }

            Actions[name].Add(((obj, text) => action?.Invoke((T)obj, text), typeof(T)));
        }

        public void On<T>(string name, Action<T> action)
        {
            On<T>(name, (obj, text) => action?.Invoke(obj));
        }

        public void On(string name, Action<string> action)
        {
            On<object>(name, (obj, text) => action?.Invoke(text));
        }

        public void On(string name, Action action)
        {
            On<object>(name, (obj, text) => action?.Invoke());
        }

#if NETSTANDARD2_1
        public async ValueTask DisposeAsync()
        {
            if (EngineIoClient != null)
            {
                await EngineIoClient.DisposeAsync();
                EngineIoClient = null;
            }
        }
#else
        public void Dispose()
        {
            EngineIoClient?.Dispose();
            EngineIoClient = null;
        }
#endif

        #endregion
    }
}
