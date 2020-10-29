using System;

namespace Foundry.Threading
{
    /// <summary>
    /// Information about a <see cref="GlobalMutex"/> lock.
    /// </summary>
    public sealed class GlobalMutexLockInfo : IDisposable
    {
        /// <summary>
        /// A <see cref="GlobalMutexLockInfo"/> instance that did not acquire a lock.
        /// </summary>
        public static GlobalMutexLockInfo Failed => new GlobalMutexLockInfo(false, Disposable.Empty);

        /// <summary>
        /// Whether the lock was successfully acquired.
        /// </summary>
        public bool AcquiredLock { get; }

        private readonly IDisposable LockFree;

        private bool Disposed;

        internal GlobalMutexLockInfo(bool acquiredLock, IDisposable lockFree)
        {
            AcquiredLock = acquiredLock;
            LockFree = lockFree;
        }

        ~GlobalMutexLockInfo()
        {
            DoDispose();
        }

        /// <summary>
        /// Releases the lock if <see cref="AcquiredLock"/> is <see langword="true"/>.
        /// </summary>
        public void Dispose()
        {
            DoDispose();
            GC.SuppressFinalize(this);
        }

        public void ReleaseMutex()
        {
            Dispose();
        }

        private void DoDispose()
        {
            lock (LockFree)
            {
                if (Disposed)
                {
                    return;
                }

                LockFree.Dispose();
                Disposed = true;
            }
        }
    }
}
