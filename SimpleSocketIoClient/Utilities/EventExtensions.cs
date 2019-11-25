using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleSocketIoClient.Utilities
{
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

        public static async Task<bool[]> WaitEventsAsync(this object value, Func<CancellationToken, Task> func, CancellationToken cancellationToken = default, params string[] eventNames)
        {
            var sources = eventNames.Select(i => new TaskCompletionSource<bool>()).ToList();
            using var cancellationSource = new CancellationTokenSource();
            
            cancellationSource.Token.Register(() =>
            {
                foreach (var source in sources)
                {
                    source?.TrySetCanceled();
                }
            }, false);
            cancellationToken.Register(() =>
            {
                foreach (var source in sources)
                {
                    source?.TrySetCanceled();
                }
            }, false);
            
            var objects = sources.Select(source => new WaitObject
            {
                Source = source,
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

                return await Task.WhenAll(sources.Select(i => i.Task));
            }
            catch (TaskCanceledException)
            {
                return sources.Select(i => false).ToArray();
            }
            finally
            {
                for (var i = 0; i < eventInfos.Count; i++)
                {
                    eventInfos[i].RemoveEventHandler(value, delegates[i]);
                }
            }
        }

        public static async Task<bool> WaitEventAsync(this object value, Func<CancellationToken, Task> func, string eventName, CancellationToken cancellationToken = default)
        {
            var results = await value.WaitEventsAsync(func, cancellationToken, eventName);

            return results.ElementAtOrDefault(0);
        }
    }
}
