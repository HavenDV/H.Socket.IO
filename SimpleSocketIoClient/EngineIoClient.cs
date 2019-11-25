using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient
{
    public sealed class EngineIoClient :
#if NETSTANDARD2_1
        IAsyncDisposable 
#else
        IDisposable
#endif
    {
        #region Constants

        public const string OpenPrefix = "0";
        public const string ClosePrefix = "1";
        public const string PingPrefix = "2";
        public const string PongPrefix = "3";
        public const string MessagePrefix = "4";
        public const string UpgradePrefix = "5";
        public const string NoopPrefix = "6";

        public const string PingMessage = "ping";

        #endregion

        #region Properties

        public WebSocketClient WebSocketClient { get; private set; } = new WebSocketClient();

        public IWebProxy Proxy {
            get => WebSocketClient.Proxy;
            set => WebSocketClient.Proxy = value;
        }

        public EngineIoOpenMessage OpenMessage { get; private set; }

        public bool IsOpened { get; set; }

        private string Framework { get; }
        private Uri Uri { get; set; }
        private System.Timers.Timer Timer { get; set; } = new System.Timers.Timer(25000);

        #endregion

        #region Events

        public event EventHandler<DataEventArgs<EngineIoOpenMessage>> Opened;
        public event EventHandler<DataEventArgs<(string Reason, WebSocketCloseStatus? Status)>> Closed;
        public event EventHandler<DataEventArgs<string>> AfterPing;
        public event EventHandler<DataEventArgs<string>> AfterPong;
        public event EventHandler<DataEventArgs<string>> AfterMessage;
        public event EventHandler<DataEventArgs<string>> Upgraded;
        public event EventHandler<DataEventArgs<string>> AfterNoop;

        public event EventHandler<DataEventArgs<Exception>> AfterException;

        private void OnOpened(EngineIoOpenMessage value)
        {
            Opened?.Invoke(this, new DataEventArgs<EngineIoOpenMessage>(value));
        }

        private void OnClosed((string Reason, WebSocketCloseStatus? Status) value)
        {
            Closed?.Invoke(this, new DataEventArgs<(string Reason, WebSocketCloseStatus? Status)>(value));
        }

        private void OnAfterPing(string value)
        {
            AfterPing?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnAfterPong(string value)
        {
            AfterPong?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnAfterMessage(string value)
        {
            AfterMessage?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnUpgraded(string value)
        {
            Upgraded?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnAfterNoop(string value)
        {
            AfterNoop?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnAfterException(Exception value)
        {
            AfterException?.Invoke(this, new DataEventArgs<Exception>(value));
        }

        #endregion

        #region Constructors

        public EngineIoClient(string framework = "engine.io")
        {
            Framework = framework ?? throw new ArgumentNullException(nameof(framework));

            Timer.Elapsed += Timer_Elapsed;

            WebSocketClient.AfterText += WebSocket_AfterText;
            WebSocketClient.AfterException += (sender, args) => OnAfterException(args.Value);
            WebSocketClient.Disconnected += (sender, args) => OnClosed(args.Value);
        }

        #endregion

        #region Event Handlers

        private async void Timer_Elapsed(object sender, ElapsedEventArgs args)
        {
            try
            {
                // Reconnect if required
                await WebSocketClient.ConnectAsync(WebSocketClient.LastConnectUri);

                await WebSocketClient.SendTextAsync($"{PingPrefix}{PingMessage}");

                OnAfterPing(PingMessage);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnAfterException(exception);
            }
        }

        private void WebSocket_AfterText(object sender, DataEventArgs<string> args)
        {
            try
            {
                var text = args.Value;
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidDataException("Empty or null Engine.IO string");
                }

                var prefix = text.Substring(0, 1);
                var value = text.Substring(1);

                switch (prefix)
                {
                    case OpenPrefix:
                        OpenMessage = JsonConvert.DeserializeObject<EngineIoOpenMessage>(value);
                        IsOpened = true;
                        OnOpened(OpenMessage);
                        break;

                    case ClosePrefix:
                        IsOpened = false;
                        OnClosed(("Received close message from server", null));
                        break;

                    case PingPrefix:
                        OnAfterPing(value);
                        break;

                    case PongPrefix:
                        OnAfterPong(value);
                        break;

                    case MessagePrefix:
                        OnAfterMessage(value);
                        break;

                    case UpgradePrefix:
                        OnUpgraded(value);
                        break;

                    case NoopPrefix:
                        OnAfterNoop(value);
                        break;
                }
            }
            catch (Exception exception)
            {
                OnAfterException(exception);
            }
        }

        #endregion

        #region Public methods

        public async Task<bool> OpenAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            if (WebSocketClient.Socket.State == WebSocketState.Open)
            {
                return true;
            }

            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            var scheme = uri.Scheme switch
            {
                "http" => "ws",
                "https" => "wss",
                "ws" => "ws",
                "wss" => "wss",
                _ => throw new ArgumentException($"Scheme is not supported: {uri.Scheme}"),
            };
            var socketIoUri = new Uri($"{scheme}://{Uri.Host}:{Uri.Port}/{Framework}/?EIO=3&transport=websocket&{Uri.Query.TrimStart('?')}");

            if (!await this.WaitEventAsync(async token =>
            {
                await WebSocketClient.ConnectAsync(socketIoUri, token);
            }, nameof(Opened), cancellationToken))
            {
                return false;
            }

            Timer.Interval = OpenMessage.PingInterval;
            Timer.Start();

            return true;
        }

        public async Task<bool> OpenAsync(Uri uri, TimeSpan timeout)
        {
            using var cancellationSource = new CancellationTokenSource(timeout);

            return await OpenAsync(uri, cancellationSource.Token);
        }

        public async Task<bool> OpenAsync(Uri uri, int timeoutInSeconds)
        {
            return await OpenAsync(uri, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            Timer.Stop();

            await WebSocketClient.SendTextAsync(ClosePrefix, cancellationToken);

            await WebSocketClient.DisconnectAsync(cancellationToken);
        }

        public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            await WebSocketClient.SendTextAsync($"{MessagePrefix}{message}", cancellationToken);
        }

#if NETSTANDARD2_1
        public async ValueTask DisposeAsync()
        {
            if (WebSocketClient != null)
            {
                await WebSocketClient.DisposeAsync();
                WebSocketClient = null;
            }

            Timer?.Dispose();
            Timer = null;
        }
#else
        public void Dispose()
        {
            WebSocketClient?.Dispose();
            WebSocketClient = null;

            Timer?.Dispose();
            Timer = null;
        }
#endif


        #endregion
    }
}
