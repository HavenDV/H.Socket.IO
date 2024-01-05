﻿using System.Net;
using System.Net.WebSockets;
using System.Text;
using H.WebSockets.Utilities;
using EventGenerator;

#if NETSTANDARD2_1 || NET5_0_OR_GREATER
using System.Net.Security;
#endif

namespace H.WebSockets;

/// <summary>
/// 
/// </summary>
[Event("Connected")]
[Event<string, WebSocketCloseStatus?>("Disconnected", PropertyNames = new []{ "Reason", "Status" })]
[Event<string>("TextReceived", PropertyNames = new[] { "Text" })]
[Event<IReadOnlyCollection<byte>>("BytesReceived", PropertyNames = new[] { "Bytes" })]
[Event<Exception>("ExceptionOccurred", PropertyNames = new[] { "Exception" })]
public sealed partial class WebSocketClient : IDisposable
#if NETSTANDARD2_1 || NET5_0_OR_GREATER
        , IAsyncDisposable
#endif
{
    #region Properties

    /// <summary>
    /// 
    /// </summary>
    public ClientWebSocket Socket { get; private set; } = new();
    private readonly SemaphoreSlim SocketSemaphore = new(1, 1);

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
        set
        {
            Socket = Socket ?? throw new ObjectDisposedException(nameof(Socket));
            Socket.Options.Proxy = value;
        }
    }

#if NETSTANDARD2_1 || NET5_0_OR_GREATER
    /// <summary>
    /// 
    /// </summary>
    public RemoteCertificateValidationCallback? RemoteCertificateValidationCallback { get; set; }
#endif

    /// <summary>
    /// 
    /// </summary>
    public void SetHeader(string name, string value)
    {
        Socket = Socket ?? throw new ObjectDisposedException(nameof(Socket));
        Socket.Options.SetRequestHeader(name, value);
    }

    private TaskWorker ReceiveWorker { get; } = new();

    #endregion

    #region Constructors

    /// <summary>
    /// 
    /// </summary>
    public WebSocketClient()
    {
        ReceiveWorker.ExceptionOccurred += (_, args) => OnExceptionOccurred(args.Exception);
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
        await SocketSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
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

#if NETSTANDARD2_1 || NET5_0_OR_GREATER
            if (RemoteCertificateValidationCallback != null)
            {
                Socket.Options.RemoteCertificateValidationCallback += RemoteCertificateValidationCallback;
            }
#endif

            await Socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);

            if (ReceiveWorker.Task.IsCompleted)
            {
                ReceiveWorker.Start(async token => await ReceiveAsync(token).ConfigureAwait(false));
            }

            OnConnected();
        }
        finally
        {
            SocketSemaphore.Release();
        }
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

        await this.WaitEventAsync<DisconnectedEventArgs>(async () =>
        {
            await Socket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Closed by client",
                cancellationToken)
                .ConfigureAwait(false);

            if (Socket.State == WebSocketState.Aborted)
            {
                OnDisconnected(
                    reason: Socket.CloseStatusDescription ?? string.Empty,
                    status: Socket.CloseStatus);
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
    public async Task<TextReceivedEventArgs> WaitTextAsync(Func<Task>? func = null, CancellationToken cancellationToken = default)
    {
        return await this.WaitEventAsync<TextReceivedEventArgs>(
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
    public async Task<TextReceivedEventArgs> WaitTextAsync(TimeSpan timeout, Func<Task>? func = null)
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
    public async Task<BytesReceivedEventArgs> WaitBytesAsync(Func<Task>? func = null, CancellationToken cancellationToken = default)
    {
        return await this.WaitEventAsync<BytesReceivedEventArgs>(
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
    public async Task<BytesReceivedEventArgs> WaitBytesAsync(TimeSpan timeout, Func<Task>? func = null)
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
        SocketSemaphore.Dispose();
    }

#if NETSTANDARD2_1 || NET5_0_OR_GREATER
    /// <summary>
    /// Cancel receive task(if it's not completed) and dispose internal resources
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        await ReceiveWorker.DisposeAsync().ConfigureAwait(false);
        Socket.Dispose();
        SocketSemaphore.Dispose();
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

#pragma warning disable CA2000 // Dispose objects before losing scope
                var stream = new MemoryStream();
#pragma warning restore CA2000 // Dispose objects before losing scope

#if NETSTANDARD2_1 || NET5_0_OR_GREATER
                await using (stream.ConfigureAwait(false))
#else
                using (stream)
#endif
                {
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
                            OnDisconnected(
                                reason: result.CloseStatusDescription ?? string.Empty,
                                status: result.CloseStatus);
                            return;
                        }

#if NETSTANDARD2_1 || NET5_0_OR_GREATER
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
#if NET7_0_OR_GREATER
                                var message = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
#else
                                var message = await reader.ReadToEndAsync().ConfigureAwait(false);
#endif
                                OnTextReceived(message);
                                break;
                            }

                        case WebSocketMessageType.Binary:
                            OnBytesReceived(stream.ToArray());
                            break;
                    }
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

        OnDisconnected(
            reason: Socket.CloseStatusDescription ?? string.Empty,
            status: Socket.CloseStatus);
    }

    #endregion
}
