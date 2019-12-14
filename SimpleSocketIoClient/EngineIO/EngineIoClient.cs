using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using SimpleSocketIoClient.EventsArgs;
using SimpleSocketIoClient.Utilities;
using SimpleSocketIoClient.WebSocket;

namespace SimpleSocketIoClient.EngineIO
{
    /// <summary>
    /// Engine.IO Client
    /// </summary>
    public sealed class EngineIoClient :
#if NETSTANDARD2_1
        IAsyncDisposable 
#else
        IDisposable
#endif
    {
        #region Constants

        private const string PingMessage = "ping";

        #endregion

        #region Properties

        /// <summary>
        /// Internal WebSocket Client
        /// </summary>
        public WebSocketClient? WebSocketClient { get; private set; }

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

        private string? Framework { get; }
        private Uri? Uri { get; set; }
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
        public event EventHandler<DataEventArgs<string>>? AfterPing;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? AfterPong;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? AfterMessage;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? Upgraded;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? AfterNoop;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<Exception>>? AfterException;

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
            WebSocketClient.AfterText += WebSocket_AfterText;
            WebSocketClient.AfterException += (sender, args) => OnAfterException(args.Value);
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
                await WebSocketClient.ConnectAsync(WebSocketClient.LastConnectUri);

                await WebSocketClient.SendTextAsync(new EngineIoPacket(EngineIoPacket.PingPrefix, PingMessage).Encode());

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
                        OnAfterPing(packet.Value);
                        break;

                    case EngineIoPacket.PongPrefix:
                        OnAfterPong(packet.Value);
                        break;

                    case EngineIoPacket.MessagePrefix:
                        OnAfterMessage(packet.Value);
                        break;

                    case EngineIoPacket.UpgradePrefix:
                        OnUpgraded(packet.Value);
                        break;

                    case EngineIoPacket.NoopPrefix:
                        OnAfterNoop(packet.Value);
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
            var scheme = uri.Scheme switch
            {
                "http" => "ws",
                "https" => "wss",
                "ws" => "ws",
                "wss" => "wss",
                _ => throw new ArgumentException($"Scheme is not supported: {uri.Scheme}"),
            };
            var socketIoUri = new Uri($"{scheme}://{Uri.Host}:{Uri.Port}/{Framework}/?EIO=3&transport=websocket&{Uri.Query.TrimStart('?')}");

            return await this.WaitEventAsync(async token =>
            {
                await WebSocketClient.ConnectAsync(socketIoUri, token);
            }, nameof(Opened), cancellationToken);
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

            return await OpenAsync(uri, cancellationSource.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        public async Task<bool> OpenAsync(Uri uri, int timeoutInSeconds)
        {
            return await OpenAsync(uri, TimeSpan.FromSeconds(timeoutInSeconds));
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

            await WebSocketClient.SendTextAsync(new EngineIoPacket(EngineIoPacket.ClosePrefix).Encode(), cancellationToken);

            await WebSocketClient.DisconnectAsync(cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            WebSocketClient = WebSocketClient ?? throw new ObjectDisposedException(nameof(WebSocketClient));

            await WebSocketClient.SendTextAsync(new EngineIoPacket(EngineIoPacket.MessagePrefix, message).Encode(), cancellationToken);
        }

#if NETSTANDARD2_1
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 
        /// </summary>
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
