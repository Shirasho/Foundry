using System;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Threading
{
    /// <summary>
    /// A <see langword="lock"/> alternative that works in async contexts.
    /// </summary>
    public sealed class AsyncLockMutex : IDisposable
    {
        private readonly Mutex Lock;

        public AsyncLockMutex()
        {
            Lock = new Mutex();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Lock?.Dispose();
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
            if (Lock.WaitOne(timeout, cancellationToken))
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
            if (await Lock.WaitOneAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                return new AsyncLockRelease(this);
            }

            throw new InvalidOperationException("Cannot acquire the API lock.");
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
            if (await Lock.WaitOneAsync(timeout, cancellationToken).ConfigureAwait(false))
            {
                return (true, new AsyncLockRelease(this));
            }

            return (false, Disposable.Empty);
        }

        private sealed class AsyncLockRelease : IDisposable
        {
            private AsyncLockMutex Owner { get; }
            private bool IsDisposed { get; set; }
            private readonly object Lock = new object();

            public AsyncLockRelease(AsyncLockMutex owner)
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
                            Owner.Lock.ReleaseMutex();
                            IsDisposed = true;
                        }
                    }
                }
            }
        }
    }
}
