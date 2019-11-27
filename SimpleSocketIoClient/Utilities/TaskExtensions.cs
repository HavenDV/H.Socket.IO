using System;
using System.Threading.Tasks;

namespace SimpleSocketIoClient.Utilities
{
    /// <summary>
    /// Extensions that work with <see cref="Task"/>
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Attempts to dispose of a Task, but will not propagate the exception.  
        /// Returns <see langword="true"/> instead if the Task could not be disposed.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="shouldMarkExceptionsHandled"></param>
        /// <returns></returns>
        public static bool TryDispose(this Task source, bool shouldMarkExceptionsHandled = true)
        {
            source = source ?? throw new ArgumentNullException(nameof(source));

            try
            {
                if (source.IsCompleted)
                {
                    if (shouldMarkExceptionsHandled)
                    {
                        source.Exception?.Flatten().Handle(x => true);
                    }

                    source.Dispose();
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
            }

            return false;
        }
    }
}
