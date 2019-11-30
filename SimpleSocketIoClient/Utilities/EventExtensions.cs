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
        /// Asynchronously expects <see langword="event"/>'s until they occur or until canceled
        /// </summary>
        /// <param name="value"></param>
        /// <param name="func"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="eventNames"></param>
        /// <returns></returns>
        public static async Task<Dictionary<string, bool>> WaitEventsAsync(this object value, Func<CancellationToken, Task> func, CancellationToken cancellationToken = default, params string[] eventNames)
        {
            var sources = eventNames.ToDictionary(
                i => i, 
                i => new TaskCompletionSource<bool>());
            using var cancellationSource = new CancellationTokenSource();
            
            cancellationSource.Token.Register(() =>
            {
                foreach (var source in sources.Values)
                {
                    source?.TrySetCanceled();
                }
            }, false);
            cancellationToken.Register(() =>
            {
                foreach (var source in sources.Values)
                {
                    source?.TrySetCanceled();
                }
            }, false);
            
            var objects = sources.Select(pair => new WaitObject
            {
                Source = pair.Value,
            }).ToList();
            var method = typeof(WaitObject).GetMethod(nameof(WaitObject.HandleEvent)) ?? throw new Exception("Method not found");
            var eventInfos = eventNames
                .Select(eventName => value.GetType().GetEvent(eventName))
                .ToList();
            var delegates = eventInfos
                .Select((eventInfo, i) => Delegate.CreateDelegate(eventInfo.EventHandlerType, objects[i], method, true))
                .ToList();

            try
            {
                for (var i = 0; i < eventInfos.Count; i++)
                {
                    eventInfos[i].AddEventHandler(value, delegates[i]);
                }

                await func(cancellationToken);

                await Task.WhenAll(sources.Values.Select(i => i.Task));
            }
            catch (TaskCanceledException)
            {
            }
            finally
            {
                for (var i = 0; i < eventInfos.Count; i++)
                {
                    eventInfos[i].RemoveEventHandler(value, delegates[i]);
                }
            }

            return sources.ToDictionary(i => i.Key, i => i.Value.Task.IsCompleted && !i.Value.Task.IsCanceled && i.Value.Task.Result);
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
            var results = await value.WaitEventsAsync(func, cancellationToken, eventName);

            return results.TryGetValue(eventName, out var result) && result;
        }
    }
}
