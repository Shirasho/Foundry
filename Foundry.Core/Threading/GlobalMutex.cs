using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Threading
{
    /// <summary>
    /// A global <see cref="System.Threading.Mutex"/> class that applies
    /// cross-application.
    /// </summary>
    [DebuggerDisplay("{" + nameof(Name) + "}")]
    public sealed class GlobalMutex : IDisposable
    {
        /// <summary>
        /// The name of the mutex.
        /// </summary>
        public string Name { get; }

        private readonly Mutex Mutex;
        private readonly object Lock = new object();

        /// <summary>
        /// Creates a <see cref="GlobalMutex"/> using the entry assembly's <see cref="GuidAttribute"/>
        /// for the name. If the assembly does not contain this attribute, an <see cref="InvalidOperationException"/>
        /// will be thrown.
        /// </summary>
        /// <exception cref="WaitHandleCannotBeOpenedException">The named mutex cannot be created, perhaps because a wait handle of a different type has the same name.</exception>
        /// <exception cref="IOException">A Win32 error occurred.</exception>
        /// <exception cref="UnauthorizedAccessException">The named mutex exists and has access control security, but the user does not have <see cref="F:System.Security.AccessControl.MutexRights.FullControl"></see>.</exception>
        public GlobalMutex()
            : this(GetAppGuid())
        {
        }

        /// <summary>
        /// Creates a <see cref="GlobalMutex"/>.
        /// </summary>
        /// <param name="mutexName">The name of the mutex.</param>
        /// <exception cref="UnauthorizedAccessException">The named mutex exists and has access control security, but the user does not have <see cref="F:System.Security.AccessControl.MutexRights.FullControl"></see>.</exception>
        /// <exception cref="IOException">A Win32 error occurred.</exception>
        /// <exception cref="WaitHandleCannotBeOpenedException">The named mutex cannot be created, perhaps because a wait handle of a different type has the same name.</exception>
        /// <exception cref="ArgumentException"><paramref name="mutexName"/> is null or empty.</exception>
        public GlobalMutex(string mutexName)
        {
            Guard.IsNotNullOrWhitespace(mutexName, nameof(mutexName));

            Name = mutexName.Replace("\\", "");
            Mutex = new Mutex(false, $"Global\\{Name}");
        }

        /// <inheritdoc />
        ~GlobalMutex()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            lock (Lock)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool disposing)
        {
            lock (Lock)
            {
                if (disposing)
                {
                    Mutex?.Dispose();
                }
            }
        }

        /// <summary>
        /// Attempts to acquire the mutex.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public GlobalMutexLockInfo TryAcquire(CancellationToken cancellationToken = default)
            => TryAcquire(Timeout.InfiniteTimeSpan, cancellationToken);

        /// <summary>
        /// Attempts to acquire the mutex.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public GlobalMutexLockInfo TryAcquire(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            try
            {
                bool hasAcquiredMutex = Mutex.WaitOne(timeout, cancellationToken);
                return new GlobalMutexLockInfo(hasAcquiredMutex, hasAcquiredMutex ? Disposable.Create(new Action(Mutex.ReleaseMutex)) : Disposable.Empty);
            }
            catch (TaskCanceledException)
            {
                return new GlobalMutexLockInfo(false, Disposable.Empty);
            }
            catch (AbandonedMutexException)
            {
                // Abandoned mutexes are still acquired.
                return new GlobalMutexLockInfo(true, Disposable.Create(new Action(Mutex.ReleaseMutex)));
            }
        }

        /// <summary>
        /// Attempts to acquire the mutex.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public Task<GlobalMutexLockInfo> TryAcquireAsync(CancellationToken cancellationToken)
            => TryAcquireAsync(Timeout.InfiniteTimeSpan, cancellationToken);

        /// <summary>
        /// Attempts to acquire the mutex.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        [SuppressMessage("Performance", "HAA0301:Closure Allocation Source", Justification = "Intentional capture.")]
        [SuppressMessage("Performance", "HAA0302:Display class allocation to capture closure", Justification = "Intentional capture.")]
        public async Task<GlobalMutexLockInfo> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            bool acquiredMutex = false;
            var mre = new ManualResetEventSlim();
            var mre2 = new ManualResetEventSlim();

            // Spawn thread to handle mutex creation and disposal.
            // This needs to happen on its own dedicated thread. We cannot
            // use async/await for this due to thread switching even with
            // ConfigureAwait(true).
            // This also means we are non-reentrant.
            var mutexThread = new Thread(() =>
            {
                try
                {
                    acquiredMutex = Mutex.WaitOne(timeout, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    acquiredMutex = false;
                }
                catch (AbandonedMutexException)
                {
                    // Abandoned mutexes are still acquired.
                    acquiredMutex = true;
                }
                catch
                {
                    acquiredMutex = false;
                }

                // Inform the outer method that we have determined
                // whether we have acquired the mutex.
                mre2.Set();

                if (acquiredMutex)
                {
                    // If we have acquired the mutex, wait until the caller
                    // disposes the lock info.
                    mre.Wait();
                    try
                    {
                        Mutex.ReleaseMutex();
                    }
                    catch
                    {
                        // Suppress
                    }
                }
            })
            {
                // Kills the thread if the application exists. This should cause
                // an AbandonedMutexException in other instances, allowing them
                // to acquire it.
                IsBackground = true
            };

            mutexThread.Start();

            // Wait until mutex acquisition either succeeds or fails.
            await mre2.WaitHandle.ConfigureAwait(false);
            mre2.Dispose();

            if (!acquiredMutex)
            {
                // If we did not acquire the mutex we have no more need
                // for this MRE. Dispose of it.
                mre.Dispose();
            }

            // Return a new lock instance that either no-ops if we did not acquire the mutex or 
            // triggers the MRE on our mutex thread and release the mutex if we did acquire it.
            return new GlobalMutexLockInfo(acquiredMutex, acquiredMutex ? new GlobalMutexLockFree(mre) : Disposable.Empty);
        }

        private static string GetAppGuid()
        {
            var guidAttr = Assembly.GetEntryAssembly()?.GetCustomAttribute<GuidAttribute>();
            if (guidAttr is null)
            {
                throw new InvalidOperationException($"Assembly is missing a {nameof(GuidAttribute)}.");
            }

            return Guid.Parse(guidAttr.Value).ToString("B");
        }

        private sealed class GlobalMutexLockFree : IDisposable
        {
            private ManualResetEventSlim ResetEvent { get; }

            public GlobalMutexLockFree(ManualResetEventSlim resetEvent)
            {
                ResetEvent = resetEvent;
            }

            ~GlobalMutexLockFree()
            {
                Dispose();
            }

            public void Dispose()
            {
                ResetEvent.Set();
                ResetEvent.Dispose();
            }
        }
    }
}
