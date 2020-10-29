using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Threading
{
    /// <summary>
    /// A <see langword="lock"/> alternative that works in async contexts.
    /// This implementation is lighter than <see cref="AsyncLock"/> but
    /// does not allow same thread re-entry.
    /// </summary>
    public sealed class AsyncLockSlim : IDisposable
    {
        private readonly SemaphoreSlim Lock;

        public AsyncLockSlim()
            : this(1, 1)
        {
        }

        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
        public AsyncLockSlim(int initialCount, int maxCount)
        {
            Lock = new SemaphoreSlim(initialCount, maxCount);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Lock.Dispose();
        }

        public IDisposable Wait()
            => Wait(Timeout.Infinite);

        public IDisposable Wait(int timeout)
            => Wait(TimeSpan.FromMilliseconds(timeout));

        public IDisposable Wait(TimeSpan timeout)
            => Wait(timeout, CancellationToken.None);

        public IDisposable Wait(CancellationToken cancellationToken)
            => Wait(Timeout.InfiniteTimeSpan, cancellationToken);

        public IDisposable Wait(int timeout, CancellationToken cancellationToken)
            => Wait(TimeSpan.FromMilliseconds(timeout), cancellationToken);

        public IDisposable Wait(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (Lock.Wait(timeout, cancellationToken))
            {
                return new AsyncLockRelease(this);
            }

            throw new InvalidOperationException("Cannot acquire the API lock.");
        }

        public Task<IDisposable> WaitAsync(int timeout)
            => WaitAsync(TimeSpan.FromMilliseconds(timeout));

        public Task<IDisposable> WaitAsync(TimeSpan timeout)
            => WaitAsync(timeout, CancellationToken.None);

        public Task<IDisposable> WaitAsync(CancellationToken cancellationToken)
            => WaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);

        public Task<IDisposable> WaitAsync(int timeout, CancellationToken cancellationToken)
            => WaitAsync(TimeSpan.FromMilliseconds(timeout), cancellationToken);

        public async Task<IDisposable> WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (await Lock.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                return new AsyncLockRelease(this);
            }

            throw new InvalidOperationException("Cannot acquire lock.");
        }

        public Task<(bool AcquiredLock, IDisposable LockFree)> TryWaitAsync(int timeout)
            => TryWaitAsync(TimeSpan.FromMilliseconds(timeout));

        public Task<(bool AcquiredLock, IDisposable LockFree)> TryWaitAsync(TimeSpan timeout)
            => TryWaitAsync(timeout, CancellationToken.None);

        public Task<(bool AcquiredLock, IDisposable LockFree)> TryWaitAsync(CancellationToken cancellationToken)
            => TryWaitAsync(Timeout.InfiniteTimeSpan, cancellationToken);

        public Task<(bool AcquiredLock, IDisposable LockFree)> TryWaitAsync(int timeout, CancellationToken cancellationToken)
            => TryWaitAsync(TimeSpan.FromMilliseconds(timeout), cancellationToken);

        public async Task<(bool AcquiredLock, IDisposable LockFree)> TryWaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (await Lock.AvailableWaitHandle.WaitOneAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                return (true, new AsyncLockRelease(this));
            }

            return (false, Disposable.Empty);
        }

        private sealed class AsyncLockRelease : IDisposable
        {
            private AsyncLockSlim Owner { get; }
            private bool IsDisposed { get; set; }
            private readonly object Lock = new object();

            public AsyncLockRelease(AsyncLockSlim owner)
            {
                Owner = owner;
            }

            public void Dispose()
            {
                if (!IsDisposed)
                {
                    lock (Lock)
                    {
                        if (!IsDisposed)
                        {
                            try
                            {
                                Owner.Lock.Release();
                            }
                            catch (ObjectDisposedException)
                            {
                                // Suppress - the underlying lock may have already
                                // been disposed, in which case we can't do anything here.
                            }
                            IsDisposed = true;
                        }
                    }
                }
            }
        }
    }
}
