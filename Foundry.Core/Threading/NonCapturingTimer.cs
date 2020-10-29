using System;
using System.Threading;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Threading
{
    /// <summary>
    /// A convenience API for interacting with System.Threading.Timer in a way
    /// that doesn't capture the ExecutionContext.
    /// </summary>
    /// <remarks>
    /// Use this (or equivalent) everywhere timers are used to avoid rooting any values stored in <see cref="AsyncLocal{T}"/>.
    /// https://github.com/dotnet/runtime/blob/master/src/libraries/Common/src/Extensions/NonCapturingTimer/NonCapturingTimer.cs
    /// </remarks>
    public static class NonCapturingTimer
    {
        public static Timer Create(TimerCallback callback, int dueTime)
            => Create(callback, dueTime, Timeout.Infinite);

        public static Timer Create(TimerCallback callback, TimeSpan dueTime)
            => Create(callback, dueTime, Timeout.InfiniteTimeSpan);

        public static Timer Create(TimerCallback callback, int dueTime, int period)
            => Create(callback, null, dueTime, period);

        public static Timer Create(TimerCallback callback, object? state, int dueTime)
            => Create(callback, state, dueTime, Timeout.Infinite);

        public static Timer Create(TimerCallback callback, object? state, TimeSpan dueTime)
            => Create(callback, state, dueTime, Timeout.InfiniteTimeSpan);

        public static Timer Create(TimerCallback callback, object? state, int dueTime, int period)
        {
            Guard.IsNotNull(callback, nameof(callback));

            // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
            bool restoreFlow = false;
            try
            {
                if (!ExecutionContext.IsFlowSuppressed())
                {
                    ExecutionContext.SuppressFlow();
                    restoreFlow = true;
                }

                return new Timer(callback, state, dueTime, period);
            }
            finally
            {
                // Restore the current ExecutionContext
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }

        public static Timer Create(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
        {
            Guard.IsNotNull(callback, nameof(callback));

            // Don't capture the current ExecutionContext and its AsyncLocals onto the timer
            bool restoreFlow = false;
            try
            {
                if (!ExecutionContext.IsFlowSuppressed())
                {
                    ExecutionContext.SuppressFlow();
                    restoreFlow = true;
                }

                return new Timer(callback, state, dueTime, period);
            }
            finally
            {
                // Restore the current ExecutionContext
                if (restoreFlow)
                {
                    ExecutionContext.RestoreFlow();
                }
            }
        }
    }
}
