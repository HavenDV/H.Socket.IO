using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using H.WebSockets;
using H.WebSockets.Args;
using H.WebSockets.Utilities;
using Newtonsoft.Json;

namespace H.Engine.IO
{
    /// <summary>
    /// Engine.IO Client
    /// </summary>
    public sealed class EngineIoClient : IDisposable
#if NETSTANDARD2_1
        , IAsyncDisposable
#endif
    {
        #region Constants

        private const string PingMessage = "ping";

        #endregion

        #region Properties

        /// <summary>
        /// Internal WebSocket Client
        /// </summary>
        public WebSocketClient WebSocketClient { get; private set; }

        /// <summary>
        /// Proxy
        /// </summary>
        public IWebProxy? Proxy
        {
            get => WebSocketClient?.Proxy;
            set
            {
                WebSocketClient = WebSocketClient ?? throw new ObjectDisposedException(nameof(WebSocketClient));
                WebSocketClient.Proxy = value;
            }
        }

#pragma warning disable 1574
        /// <summary>
        /// This property contains OpenMessage after successful <seealso cref="OpenAsync(Uri, CancellationToken)"/>
        /// </summary>
#pragma warning restore 1574
        public EngineIoOpenMessage? OpenMessage { get; private set; }

#pragma warning disable 1574
        /// <summary>
        /// This property is true after successful <seealso cref="OpenAsync(Uri, CancellationToken)"/>
        /// </summary>
#pragma warning restore 1574
        public bool IsOpened { get; set; }

        private string Framework { get; }
        /// <summary>
        /// Opened uri.
        /// </summary>
        public Uri? Uri { get; set; }
        private System.Timers.Timer? Timer { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<EngineIoOpenMessage?>>? Opened;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<WebSocketCloseEventArgs>? Closed;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? PingSent;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? PingReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? PongReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? MessageReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? Upgraded;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? NoopReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<Exception>>? ExceptionOccurred;

        private void OnOpened(EngineIoOpenMessage? value)
        {
            if (Timer == null)
            {
                return;
            }

            IsOpened = true;

            Timer.Interval = value?.PingInterval ?? 25000;
            Timer.Start();

            Opened?.Invoke(this, new DataEventArgs<EngineIoOpenMessage?>(value));
        }

        private void OnClosed(string? reason, WebSocketCloseStatus? status)
        {
            IsOpened = false;

            Closed?.Invoke(this, new WebSocketCloseEventArgs(reason, status));
        }

        private void OnPingSent(string value)
        {
            PingSent?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnPingReceived(string value)
        {
            PingReceived?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnPongReceived(string value)
        {
            PongReceived?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnMessageReceived(string value)
        {
            MessageReceived?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnUpgraded(string value)
        {
            Upgraded?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnNoopReceived(string value)
        {
            NoopReceived?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnExceptionOccurred(Exception value)
        {
            ExceptionOccurred?.Invoke(this, new DataEventArgs<Exception>(value));
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="framework"></param>
        public EngineIoClient(string framework = "engine.io")
        {
            Framework = framework ?? throw new ArgumentNullException(nameof(framework));

            Timer = new System.Timers.Timer(25000);
            Timer.Elapsed += Timer_Elapsed;

            WebSocketClient = new WebSocketClient();
            WebSocketClient.TextReceived += WebSocketClient_OnTextReceived;
            WebSocketClient.ExceptionOccurred += (sender, args) => OnExceptionOccurred(args.Value);
            WebSocketClient.Disconnected += (sender, args) => OnClosed(args.Reason, args.Status);
        }

        #endregion

        #region Event Handlers

        private async void Timer_Elapsed(object sender, ElapsedEventArgs args)
        {
            WebSocketClient = WebSocketClient ?? throw new ObjectDisposedException(nameof(WebSocketClient));
            
            try
            {
                // Reconnect if required
                await WebSocketClient.ConnectAsync(WebSocketClient.LastConnectUri).ConfigureAwait(false);

                await WebSocketClient.SendTextAsync(new EngineIoPacket(EngineIoPacket.PingPrefix, PingMessage).Encode()).ConfigureAwait(false);

                OnPingSent(PingMessage);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        private void WebSocketClient_OnTextReceived(object? sender, DataEventArgs<string>? args)
        {
            try
            {
                var text = args?.Value ?? throw new InvalidOperationException("Null Engine.IO string");
                if (string.IsNullOrWhiteSpace(text))
                {
                    throw new InvalidOperationException("Empty Engine.IO string");
                }

                var packet = EngineIoPacket.Decode(text);
                switch (packet.Prefix)
                {
                    case EngineIoPacket.OpenPrefix:
                        OpenMessage = JsonConvert.DeserializeObject<EngineIoOpenMessage>(packet.Value);
                        IsOpened = true;
                        OnOpened(OpenMessage);
                        break;

                    case EngineIoPacket.ClosePrefix:
                        IsOpened = false;
                        OnClosed("Received close message from server", null);
                        break;

                    case EngineIoPacket.PingPrefix:
                        OnPingReceived(packet.Value);
                        break;

                    case EngineIoPacket.PongPrefix:
                        OnPongReceived(packet.Value);
                        break;

                    case EngineIoPacket.MessagePrefix:
                        OnMessageReceived(packet.Value);
                        break;

                    case EngineIoPacket.UpgradePrefix:
                        OnUpgraded(packet.Value);
                        break;

                    case EngineIoPacket.NoopPrefix:
                        OnNoopReceived(packet.Value);
                        break;
                }
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> OpenAsync(Uri uri, CancellationToken cancellationToken = default)
        {
            WebSocketClient = WebSocketClient ?? throw new ObjectDisposedException(nameof(WebSocketClient));
            Timer = Timer ?? throw new ObjectDisposedException(nameof(Timer));

            if (WebSocketClient.IsConnected)
            {
                return true;
            }

            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
            var socketIoUri = ToWebSocketUri(uri, Framework);

            return await this.WaitEventAsync<DataEventArgs<EngineIoOpenMessage>>(async () =>
            {
                await WebSocketClient.ConnectAsync(socketIoUri, cancellationToken).ConfigureAwait(false);
            }, nameof(Opened), cancellationToken).ConfigureAwait(false) != null;
        }

        internal static Uri ToWebSocketUri(Uri uri, string framework)
        {
            uri = uri ?? throw new ArgumentNullException(nameof(uri));
            framework = framework ?? throw new ArgumentNullException(nameof(framework));

            var scheme = uri.Scheme switch
            {
                "http" => "ws",
                "https" => "wss",
                "ws" => "ws",
                "wss" => "wss",
                _ => throw new ArgumentException($"Scheme is not supported: {uri.Scheme}"),
            };

            return new Uri($"{scheme}://{uri.Host}:{uri.Port}{uri.AbsolutePath.TrimEnd('/')}/{framework}/?EIO=3&transport=websocket&{uri.Query.TrimStart('?')}");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<bool> OpenAsync(Uri uri, TimeSpan timeout)
        {
            using var cancellationSource = new CancellationTokenSource(timeout);

            return await OpenAsync(uri, cancellationSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        public async Task<bool> OpenAsync(Uri uri, int timeoutInSeconds)
        {
            return await OpenAsync(uri, TimeSpan.FromSeconds(timeoutInSeconds)).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task CloseAsync(CancellationToken cancellationToken = default)
        {
            WebSocketClient = WebSocketClient ?? throw new ObjectDisposedException(nameof(WebSocketClient));
            Timer = Timer ?? throw new ObjectDisposedException(nameof(Timer));

            Timer.Stop();

            await WebSocketClient.SendTextAsync(new EngineIoPacket(EngineIoPacket.ClosePrefix).Encode(), cancellationToken).ConfigureAwait(false);

            await WebSocketClient.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Sends new data with message prefix
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            WebSocketClient = WebSocketClient ?? throw new ObjectDisposedException(nameof(WebSocketClient));

            await WebSocketClient.SendTextAsync(new EngineIoPacket(EngineIoPacket.MessagePrefix, message).Encode(), cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Dispose internal resources
        /// Prefer DisposeAsync if possible
        /// </summary>
        /// <returns></returns>
        public void Dispose()
        {
            WebSocketClient.Dispose();

            Timer?.Dispose();
            Timer = null;
        }

#if NETSTANDARD2_1
        /// <summary>
        /// Dispose internal resources
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await WebSocketClient.DisposeAsync().ConfigureAwait(false);

            Timer?.Dispose();
            Timer = null;
        }
#endif

#endregion
    }
}
