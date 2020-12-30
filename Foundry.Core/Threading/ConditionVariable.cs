using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Threading
{
    /// <summary>
    /// A variable that will block until it is set.
    /// </summary>
    /// <typeparam name="T">The variable type.</typeparam>
    /// <remarks>
    /// This behaves like a synchronous <see cref="System.Threading.Tasks.TaskCompletionSource{TResult}"/>,
    /// or a <see cref="ManualResetEvent"/> with a return type.
    /// </remarks>
    public sealed class ConditionVariable<T> : IDisposable
    {
        /// <summary>
        /// The synchronization object.
        /// </summary>
        public object SyncRoot { get; } = new object();

        private readonly ManualResetEvent ResetEvent = new ManualResetEvent(false);

        [AllowNull, MaybeNull]
        private T Value;

        private bool IsSet;

        /// <summary>
        /// Creates a new instance of <see cref="ConditionVariable{T}"/> that is
        /// not set.
        /// </summary>
        public ConditionVariable()
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="ConditionVariable{T}"/> that is
        /// set with the provided value.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public ConditionVariable(T? initialValue)
        {
            Set(initialValue);
        }

        public void Dispose()
        {
            ResetEvent.Dispose();
        }

        /// <summary>
        /// Waits for the variable to be set and returns the result.
        /// </summary>
        [return: MaybeNull]
        public T Wait()
        {
            ResetEvent.WaitOne();
            return Value;
        }

        /// <summary>
        /// Waits for the variable to be set and returns the result.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before timing out.</param>
        /// <exception cref="TimeoutException">The operation timed out.</exception>
        [return: MaybeNull]
        public T Wait(int millisecondsTimeout)
        {
            if (ResetEvent.WaitOne(millisecondsTimeout))
            {
                return Value;
            }

            throw new TimeoutException("Unable to wait for variable. The operation timed out.");
        }

        /// <summary>
        /// Waits for the variable to be set and returns the result.
        /// </summary>
        /// <param name="timeout">The duration to wait before timing out.</param>
        /// <exception cref="TimeoutException">The operation timed out.</exception>
        [return: MaybeNull]
        public T Wait(TimeSpan timeout)
        {
            if (ResetEvent.WaitOne(timeout))
            {
                return Value;
            }

            throw new TimeoutException("Unable to wait for variable. The operation timed out.");
        }

        /// <summary>
        /// Waits for the variable to be set and returns the result.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="TimeoutException">The operation timed out.</exception>
        public async Task<T?> WaitAsync(CancellationToken cancellationToken = default)
        {
            await ResetEvent.WaitOneAsync(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            return Value!;
        }

        /// <summary>
        /// Waits for the variable to be set and returns the result.
        /// </summary>
        /// <param name="millisecondsTimeout">The number of milliseconds to wait before timing out.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="TimeoutException">The operation timed out.</exception>
        public async Task<T?> WaitAsync(int millisecondsTimeout, CancellationToken cancellationToken = default)
        {
            if (await ResetEvent.WaitOneAsync(millisecondsTimeout, cancellationToken).ConfigureAwait(false))
            {
                return Value;
            }

            throw new TimeoutException("Unable to wait for variable. The operation timed out.");
        }

        /// <summary>
        /// Waits for the variable to be set and returns the result.
        /// </summary>
        /// <param name="timeout">The duration to wait before timing out.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="TimeoutException">The operation timed out.</exception>
        public async Task<T?> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            if (await ResetEvent.WaitOneAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                return Value;
            }

            throw new TimeoutException("Unable to wait for variable. The operation timed out.");
        }

        /// <summary>
        /// Sets the value of this variable.
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException">The variable has already been set.</exception>
        public void Set(T? value)
        {
            lock (SyncRoot)
            {
                if (IsSet)
                {
                    throw new InvalidOperationException("The variable has already been set.");
                }

                Value = value;
                ResetEvent.Set();
                IsSet = true;
            }
        }

        /// <summary>
        /// Unsets the value of this variable.
        /// </summary>
        public void Unset()
        {
            lock (SyncRoot)
            {
                Value = default;
                ResetEvent.Reset();
                IsSet = false;
            }
        }
    }
}
