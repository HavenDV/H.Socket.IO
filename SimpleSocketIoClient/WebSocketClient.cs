using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleSocketIoClient.Utilities;

namespace SimpleSocketIoClient
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WebSocketClient :
#if NETSTANDARD2_1
        IAsyncDisposable 
#else
        IDisposable
#endif
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ClientWebSocket? Socket { get; private set; } = new ClientWebSocket();

        /// <summary>
        /// 
        /// </summary>
        public Uri? LastConnectUri { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected => Socket?.State == WebSocketState.Open;

        /// <summary>
        /// 
        /// </summary>
        public IWebProxy? Proxy
        {
            get => Socket?.Options?.Proxy;
            set {
                Socket = Socket ?? throw new ObjectDisposedException(nameof(Socket));
                Socket.Options.Proxy = value;
            }
        }

        private Task? ReceiveTask { get; set; }
        private CancellationTokenSource? CancellationTokenSource { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs>? Connected;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<(string Reason, WebSocketCloseStatus? Status)>>? Disconnected;
        
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? AfterText;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<byte[]>>? AfterBinary;

        /// <summary>
        /// 
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

        private void OnAfterText(string value)
        {
            AfterText?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnAfterBinary(byte[] value)
        {
            AfterBinary?.Invoke(this, new DataEventArgs<byte[]>(value));
        }

        private void OnAfterException(Exception value)
        {
            AfterException?.Invoke(this, new DataEventArgs<Exception>(value));
        }

        #endregion

        #region Public methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ConnectAsync(Uri? uri, CancellationToken cancellationToken = default)
        {
            if (IsConnected)
            {
                return;
            }

            Socket = Socket ?? throw new ObjectDisposedException(nameof(Socket));
            if (Socket.State != WebSocketState.None)
            {
                Socket?.Dispose();
                Socket = new ClientWebSocket();
            }

            LastConnectUri = uri ?? throw new ArgumentNullException(nameof(uri));

            await Socket.ConnectAsync(uri, cancellationToken);

            if (ReceiveTask == null ||
                ReceiveTask.IsCompleted)
            {
                ReceiveTask?.Dispose();
                CancellationTokenSource?.Dispose();

                CancellationTokenSource = new CancellationTokenSource();
                ReceiveTask = Task.Run(async () => await ReceiveAsync(CancellationTokenSource.Token), CancellationTokenSource.Token);
            }

            OnConnected();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task ConnectAsync(Uri uri, TimeSpan timeout)
        {
            using var cancellationSource = new CancellationTokenSource(timeout);

            await ConnectAsync(uri, cancellationSource.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        public async Task ConnectAsync(Uri uri, int timeoutInSeconds)
        {
            await ConnectAsync(uri, TimeSpan.FromSeconds(timeoutInSeconds));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            Socket = Socket ?? throw new ObjectDisposedException(nameof(Socket));
            if (!IsConnected)
            {
                return;
            }
            
            await this.WaitEventAsync(async token =>
            {
                await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", token);

                if (Socket.State == WebSocketState.Aborted)
                {
                    OnDisconnected((Socket.CloseStatusDescription, Socket.CloseStatus));
                }
            }, nameof(Disconnected), cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendTextAsync(string message, CancellationToken cancellationToken = default)
        {
            Socket = Socket ?? throw new ObjectDisposedException(nameof(Socket));
            if (!IsConnected)
            {
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(message);

            await Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
        }

#if NETSTANDARD2_1
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            if (ReceiveTask != null && CancellationTokenSource != null && Socket != null)
            {
                CancellationTokenSource.Cancel();

                await ReceiveTask;
            }

            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;

            ReceiveTask?.Dispose();
            ReceiveTask = null;

            Socket?.Dispose();
            Socket = null;
        }
#else
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (ReceiveTask != null && CancellationTokenSource != null && Socket != null)
            {
                CancellationTokenSource.Cancel();
            }

            CancellationTokenSource?.Dispose();
            CancellationTokenSource = null;

            ReceiveTask?.TryDispose();
            ReceiveTask = null;

            Socket?.Dispose();
            Socket = null;
        }
#endif


        #endregion

        #region Private methods

        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (Socket?.State == WebSocketState.Open)
                {
                    var buffer = new byte[1024];

                    WebSocketReceiveResult result;
#if NETSTANDARD2_1
                    await using var stream = new MemoryStream();
#else
                    using var stream = new MemoryStream();
#endif
                    do
                    {
                        try
                        {
                            result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        }
                        catch (WebSocketException exception) when (LastConnectUri != null)
                        {
                            OnAfterException(exception);

                            await ConnectAsync(LastConnectUri, cancellationToken);

                            result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                        }

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            OnDisconnected((result.CloseStatusDescription, result.CloseStatus));
                            return;
                        }

                        stream.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);

                    stream.Seek(0, SeekOrigin.Begin);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                        {
                            using var reader = new StreamReader(stream, Encoding.UTF8);
                            var message = reader.ReadToEnd();
                            OnAfterText(message);
                            break;
                        }

                        case WebSocketMessageType.Binary:
                            OnAfterBinary(stream.ToArray());
                            break;
                    }

                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnAfterException(exception);
            }
        }

        #endregion
    }
}
