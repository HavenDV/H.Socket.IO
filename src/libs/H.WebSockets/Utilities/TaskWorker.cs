using EventGenerator;

namespace H.WebSockets.Utilities;

/// <summary>
/// A class designed to run code using <see cref="Task"/> with <see cref="TaskCreationOptions.LongRunning"/> <br/>
/// and supporting automatic cancellation after Dispose <br/>
/// <![CDATA[Version: 1.0.0.8]]> <br/>
/// </summary>
[Event("Canceled", Description = "When canceled")]
[Event("Completed", Description = "When completed(with any result)")]
[Event("SuccessfulCompleted", Description = "When completed(without exceptions and cancellations)")]
[Event<OperationCanceledException>("SuccessfulCompletedOrCanceled", Description = "When completed(without exceptions)", PropertyNames = new[] { "Exception" })]
[Event<Exception>("FailedOrCanceled", Description = "When canceled or exceptions", PropertyNames = new[] { "Exception" })]
[Event<Exception>("ExceptionOccurred", Description = "When a exception occurs(without OperationCanceledException's)", PropertyNames = new[] { "Exception" })]
internal partial class TaskWorker : IDisposable
#if NETSTANDARD2_1
        , IAsyncDisposable
#endif
{
    #region Fields

    private volatile bool _isDisposed;

    #endregion

    #region Properties

    /// <summary>
    /// Is Disposed
    /// </summary>
    public bool IsDisposed => _isDisposed;

    /// <summary>
    /// Internal task
    /// </summary>
    public Task Task { get; set; } = Task.FromResult(false);

    /// <summary>
    /// Internal task CancellationTokenSource
    /// </summary>
    public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();

    #endregion

    #region Methods

    /// <summary>
    /// Starts <see cref="TaskWorker"/>
    /// </summary>
    /// <param name="func"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Start(Func<CancellationToken, Task> func)
    {
        func = func ?? throw new ArgumentNullException(nameof(func));

        if (_isDisposed)
        {
            throw new InvalidOperationException("The task worker is disposed");
        }
        if (!Task.IsCompleted)
        {
            throw new InvalidOperationException("The task worker already started");
        }

        Task = Task.Factory.StartNew(async () =>
        {
            try
            {
                await func(CancellationTokenSource.Token).ConfigureAwait(false);

                OnSuccessfulCompleted();
                OnSuccessfulCompletedOrCanceled((OperationCanceledException)null!);
            }
            catch (OperationCanceledException exception)
            {
                OnCanceled();
                OnFailedOrCanceled(exception);
                OnSuccessfulCompletedOrCanceled(exception);
            }
            catch (Exception exception)
            {
                OnExceptionOccurred(exception);
                OnFailedOrCanceled(exception);
            }

            OnCompleted();
        }, CancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    /// <summary>
    /// Starts <see cref="TaskWorker"/>
    /// </summary>
    /// <param name="action"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Start(Action<CancellationToken> action)
    {
        action = action ?? throw new ArgumentNullException(nameof(action));

        Start(cancellationToken =>
        {
            action(cancellationToken);

            return Task.FromResult(false);
        });
    }

    #endregion

    #region IDisposable

    /// <summary>
    /// Cancel task(if it's not completed) and dispose internal resources <br/>
    /// Prefer DisposeAsync if possible <br/>
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }

#if NETSTANDARD2_1
    /// <summary>
    /// Cancel task(if it's not completed) and dispose internal resources <br/>
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (Task != Task.CompletedTask)
        {
            CancellationTokenSource.Cancel();

            try
            {
                await Task.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }

            // Some system code can still use CancellationToken, so we wait
            await Task.Delay(TimeSpan.FromMilliseconds(1)).ConfigureAwait(false);
        }

        CancellationTokenSource.Dispose();
        Task.Dispose();
    }
#endif

    #endregion
}
