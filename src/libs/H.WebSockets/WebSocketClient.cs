using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using H.WebSockets.Args;
using H.WebSockets.Utilities;

namespace H.WebSockets
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class WebSocketClient : IDisposable
#if NETSTANDARD2_1
        , IAsyncDisposable
#endif
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public ClientWebSocket Socket { get; private set; } = new ();

        /// <summary>
        /// 
        /// </summary>
        public Uri? LastConnectUri { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected => Socket.State == WebSocketState.Open;

        /// <summary>
        /// 
        /// </summary>
        public IWebProxy? Proxy
        {
            get => Socket.Options.Proxy;
            set {
                Socket = Socket ?? throw new ObjectDisposedException(nameof(Socket));
                Socket.Options.Proxy = value;
            }
        }

        private TaskWorker ReceiveWorker { get; } = new ();

        #endregion

        #region Events

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<EventArgs>? Connected;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<WebSocketCloseEventArgs>? Disconnected;
        
        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<string>>? TextReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<IReadOnlyCollection<byte>>>? BytesReceived;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<DataEventArgs<Exception>>? ExceptionOccurred;

        private void OnConnected()
        {
            Connected?.Invoke(this, EventArgs.Empty);
        }

        private void OnDisconnected(string reason, WebSocketCloseStatus? status)
        {
            Disconnected?.Invoke(this, new WebSocketCloseEventArgs(reason, status));
        }

        private void OnTextReceived(string value)
        {
            TextReceived?.Invoke(this, new DataEventArgs<string>(value));
        }

        private void OnBytesReceived(IReadOnlyCollection<byte> value)
        {
            BytesReceived?.Invoke(this, new DataEventArgs<IReadOnlyCollection<byte>>(value));
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
        public WebSocketClient()
        {
            ReceiveWorker.ExceptionOccurred += (_, exception) => OnExceptionOccurred(exception);
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
                Socket.Dispose();
                Socket = new ClientWebSocket();
            }

            LastConnectUri = uri ?? throw new ArgumentNullException(nameof(uri));

            await Socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);

            if (ReceiveWorker.Task.IsCompleted)
            {
                ReceiveWorker.Start(async token => await ReceiveAsync(token).ConfigureAwait(false));
            }

            OnConnected();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="timeout"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ConnectAsync(Uri uri, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            using var cancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cancellationSource.CancelAfter(timeout);

            await ConnectAsync(uri, cancellationSource.Token).ConfigureAwait(false);
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
            
            await this.WaitEventAsync<WebSocketCloseEventArgs>(async () =>
            {
                await Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure, 
                    "Closed by client", 
                    cancellationToken)
                    .ConfigureAwait(false);

                if (Socket.State == WebSocketState.Aborted)
                {
                    OnDisconnected(Socket.CloseStatusDescription ?? string.Empty, Socket.CloseStatus);
                }
            }, nameof(Disconnected), cancellationToken)
                .ConfigureAwait(false);
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

            await Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task SendBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            Socket = Socket ?? throw new ObjectDisposedException(nameof(Socket));
            if (!IsConnected)
            {
                return;
            }

            await Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Binary, true, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the next text asynchronously <br/>
        /// Returns DataEventArgs if text was received <br/>
        /// Throws <see cref="OperationCanceledException"/> if method was canceled <br/>
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public async Task<DataEventArgs<string>> WaitTextAsync(Func<Task>? func = null, CancellationToken cancellationToken = default)
        {
            return await this.WaitEventAsync<DataEventArgs<string>>(
                func ?? (() => Task.FromResult(false)), 
                nameof(TextReceived),
                cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the next text asynchronously with specified timeout <br/>
        /// Returns DataEventArgs if text was received <br/>
        /// Throws <see cref="OperationCanceledException"/> if method was canceled <br/>
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public async Task<DataEventArgs<string>> WaitTextAsync(TimeSpan timeout, Func<Task>? func = null)
        {
            using var tokenSource = new CancellationTokenSource(timeout);
            var cancellationToken = tokenSource.Token;

            return await WaitTextAsync(func, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the next bytes asynchronously <br/>
        /// Returns DataEventArgs if bytes was received <br/>
        /// Throws <see cref="OperationCanceledException"/> if method was canceled <br/>
        /// </summary>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public async Task<DataEventArgs<IReadOnlyCollection<byte>>> WaitBytesAsync(Func<Task>? func = null, CancellationToken cancellationToken = default)
        {
            return await this.WaitEventAsync<DataEventArgs<IReadOnlyCollection<byte>>>(
                func ?? (() => Task.FromResult(false)),
                nameof(BytesReceived),
                cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Waits for the next bytes asynchronously with specified timeout <br/>
        /// Returns DataEventArgs if bytes was received <br/>
        /// Throws <see cref="OperationCanceledException"/> if method was canceled <br/>
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="func"></param>
        /// <exception cref="OperationCanceledException"></exception>
        /// <returns></returns>
        public async Task<DataEventArgs<IReadOnlyCollection<byte>>> WaitBytesAsync(TimeSpan timeout, Func<Task>? func = null)
        {
            using var tokenSource = new CancellationTokenSource(timeout);

            return await WaitBytesAsync(func, tokenSource.Token)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Cancel receive task(if it's not completed) and dispose internal resources
        /// Prefer DisposeAsync if possible
        /// </summary>
        public void Dispose()
        {
            ReceiveWorker.Dispose();
            Socket.Dispose();
        }

#if NETSTANDARD2_1
        /// <summary>
        /// Cancel receive task(if it's not completed) and dispose internal resources
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await ReceiveWorker.DisposeAsync().ConfigureAwait(false);
            Socket.Dispose();
        }
#endif

        #endregion

        #region Private methods

        private async Task ReceiveAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (Socket.State == WebSocketState.Open)
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
                            result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);
                        }
                        catch (WebSocketException exception) when (LastConnectUri != null)
                        {
                            OnExceptionOccurred(exception);

                            await ConnectAsync(LastConnectUri, cancellationToken).ConfigureAwait(false);

                            result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken).ConfigureAwait(false);
                        }

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            OnDisconnected(result.CloseStatusDescription ?? string.Empty, result.CloseStatus);
                            return;
                        }

#if NETSTANDARD2_1
                        await stream.WriteAsync(buffer.AsMemory(0, result.Count), cancellationToken).ConfigureAwait(false);
#else
                        await stream.WriteAsync(buffer, 0, result.Count, cancellationToken).ConfigureAwait(false);
#endif
                    } while (!result.EndOfMessage);

                    stream.Seek(0, SeekOrigin.Begin);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                        {
                            using var reader = new StreamReader(stream, Encoding.UTF8);
                            var message = await reader.ReadToEndAsync().ConfigureAwait(false);
                            OnTextReceived(message);
                            break;
                        }

                        case WebSocketMessageType.Binary:
                            OnBytesReceived(stream.ToArray());
                            break;
                    }

                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
            }

            OnDisconnected(Socket.CloseStatusDescription ?? string.Empty, Socket.CloseStatus);
        }

        #endregion
    }
}
