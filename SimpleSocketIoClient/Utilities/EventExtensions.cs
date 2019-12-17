using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSocketIoClient.Utilities
{
    /// <summary>
    /// Extensions that work with <see langword="event"/>
    /// </summary>
    public static class EventExtensions
    {
        private class WaitObject
        {
            public TaskCompletionSource<bool>? Source { get; set; }

            // ReSharper disable UnusedParameter.Local
            public void HandleEvent(object sender, EventArgs e)
            {
                Source?.TrySetResult(true);
            }
        }

        /// <summary>
        /// Asynchronously expects <see langword="event"/> until they occur or until canceled
        /// </summary>
        /// <param name="value"></param>
        /// <param name="eventName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> WaitEventAsync(this object value, string eventName, CancellationToken cancellationToken = default)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            using var cancellationSource = new CancellationTokenSource();

            cancellationSource.Token.Register(() => taskCompletionSource.TrySetCanceled());
            cancellationToken.Register(() => taskCompletionSource.TrySetCanceled());

            var waitObject = new WaitObject
            {
                Source = taskCompletionSource,
            };
            var method = typeof(WaitObject).GetMethod(nameof(WaitObject.HandleEvent)) ?? throw new InvalidOperationException("Method not found");
            var eventInfo = value.GetType().GetEvent(eventName) ?? throw new InvalidOperationException("Event info not found");
            // ReSharper disable once ConstantNullCoalescingCondition
            var eventHandlerType = eventInfo.EventHandlerType ?? throw new InvalidOperationException("Event Handler Type not found");
            var delegateObject = Delegate.CreateDelegate(eventHandlerType, waitObject, method, true);

            try
            {
                eventInfo.AddEventHandler(value, delegateObject);

                return await taskCompletionSource.Task;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            finally
            {
                eventInfo.RemoveEventHandler(value, delegateObject);
            }
        }

        /// <summary>
        /// Asynchronously expects <see langword="event"/> until they occur or until canceled
        /// </summary>
        /// <param name="value"></param>
        /// <param name="func"></param>
        /// <param name="eventName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> WaitEventAsync(this object value, Func<CancellationToken, Task> func, string eventName, CancellationToken cancellationToken = default)
        {
            try
            {
                var task = value.WaitEventAsync(eventName, cancellationToken);

                await func(cancellationToken);

                return await task;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously expects <see langword="event"/>'s until they occur or until canceled
        /// </summary>
        /// <param name="value"></param>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="eventNames"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, bool>> WaitEventsAsync(this object value, Func<CancellationToken, Task> func, CancellationToken cancellationToken = default, params string[] eventNames)
        {
            var tasks = eventNames
                .Select(name => value.WaitEventAsync(name, cancellationToken))
                .ToList();

            try
            {
                await func(cancellationToken);

                var results = await Task.WhenAll(tasks);

                return eventNames
                    .Zip(results, (name, result) => (name, result))
                    .ToDictionary(i => i.name, i => i.result);
            }
            catch (TaskCanceledException)
            {
                return eventNames
                    .Zip(tasks, (name, task) => (name, task))
                    .ToDictionary(i => i.name, i => i.task.IsCompleted && !i.task.IsCanceled && i.task.Result);
            }
        }
    }
}
