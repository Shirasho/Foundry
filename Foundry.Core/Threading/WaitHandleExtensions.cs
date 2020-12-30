using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Threading
{
    /// <summary>
    /// <see cref="WaitHandle"/> extensions.
    /// </summary>
    public static class WaitHandleExtensions
    {
        /// <summary>
        /// Configures an awaiter used to await this <see cref="WaitHandle"/>.
        /// </summary>
        /// <param name="handle">The <see cref="WaitHandle"/> instance to await.</param>
        /// <param name="continueOnCapturedContext">Whether to marshal the continuation back to the original captured context.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ConfiguredTaskAwaitable<bool> ConfigureAwait(this WaitHandle handle, bool continueOnCapturedContext)
        {
            Guard.IsNotNull(handle, nameof(handle));

            return handle.ToTask().ConfigureAwait(continueOnCapturedContext);
        }

        /// <summary>
        /// Provides await functionality for ordinary <see cref="WaitHandle"/>s.
        /// </summary>
        /// <param name="handle">The handle to wait on.</param>
        /// <returns>The awaiter.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TaskAwaiter<bool> GetAwaiter(this WaitHandle handle)
        {
            Guard.IsNotNull(handle, nameof(handle));

            return handle.ToTask().GetAwaiter();
        }

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="waitHandle"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/>.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="waitHandle"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static bool IsCancelledBefore(this WaitHandle waitHandle, int duration)
            => IsCancelledBefore(waitHandle, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="waitHandle"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/>.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="waitHandle"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static Task<bool> IsCancelledBeforeAsync(this WaitHandle waitHandle, int duration)
            => IsCancelledBeforeAsync(waitHandle, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="waitHandle"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/>.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="waitHandle"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static bool IsCancelledBefore(this WaitHandle waitHandle, TimeSpan duration)
        {
            return !IsNotCancelledAfter(waitHandle, duration);
        }

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="waitHandle"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/>.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="waitHandle"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static async Task<bool> IsCancelledBeforeAsync(this WaitHandle waitHandle, TimeSpan duration)
        {
            return !await IsNotCancelledAfterAsync(waitHandle, duration).ConfigureAwait(false);
        }

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="waitHandle"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/>.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="waitHandle"/> was canceled.</returns>

        public static bool IsNotCancelledAfter(this WaitHandle waitHandle, int duration)
            => IsNotCancelledAfter(waitHandle, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="waitHandle"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/>.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="waitHandle"/> was canceled.</returns>

        public static Task<bool> IsNotCancelledAfterAsync(this WaitHandle waitHandle, int duration)
            => IsNotCancelledAfterAsync(waitHandle, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="waitHandle"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/>.</param>
        /// <param name="duration">The time to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="waitHandle"/> was canceled.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static bool IsNotCancelledAfter(this WaitHandle waitHandle, TimeSpan duration)
        {
            try
            {
                return !waitHandle.WaitOne(duration);
            }
            catch (AbandonedMutexException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (SEHException e) when (e.Message.IndexOf("0x80004005", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // This error is likely due to anti-tampering technology or antivirus software.
                // Nothing we can really do about it.
                throw;
            }
        }

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="waitHandle"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="waitHandle">The <see cref="WaitHandle"/>.</param>
        /// <param name="duration">The time to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="waitHandle"/> was canceled.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static async Task<bool> IsNotCancelledAfterAsync(this WaitHandle waitHandle, TimeSpan duration)
        {
            try
            {
                return !await waitHandle.WaitOneAsync(duration).ConfigureAwait(false);
            }
            catch (AbandonedMutexException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (SEHException e) when (e.Message.IndexOf("0x80004005", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // This error is likely due to anti-tampering technology or antivirus software.
                // Nothing we can really do about it.
                throw;
            }
        }

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will always
        ///     return <see langword="true"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> ToTask(this WaitHandle handle)
            => ToTask(handle, Timeout.Infinite, CancellationToken.None);

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <param name="timeout">The amount of time to wait before timing out.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will be
        ///     <see langword="true"/> if the <see cref="WaitHandle"/> was triggered before <paramref name="timeout"/>
        ///     elapses, <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> ToTask(this WaitHandle handle, int timeout)
            => ToTask(handle, TimeSpan.FromMilliseconds(timeout), CancellationToken.None);

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <param name="timeout">The amount of time to wait before timing out.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will be
        ///     <see langword="true"/> if the <see cref="WaitHandle"/> was triggered before <paramref name="timeout"/>
        ///     elapses, <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> ToTask(this WaitHandle handle, TimeSpan timeout)
            => ToTask(handle, timeout, CancellationToken.None);

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <param name="timeout">The amount of time to wait before timing out.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will be
        ///     <see langword="true"/> if the <see cref="WaitHandle"/> was triggered before <paramref name="timeout"/>
        ///     elapses, <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> ToTask(this WaitHandle handle, int timeout, CancellationToken cancellationToken)
            => ToTask(handle, TimeSpan.FromMilliseconds(timeout), cancellationToken);

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <param name="timeout">The amount of time to wait before timing out.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will be
        ///     <see langword="true"/> if the <see cref="WaitHandle"/> was triggered before <paramref name="timeout"/>
        ///     elapses, <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// https://thomaslevesque.com/2015/06/04/async-and-cancellation-support-for-wait-handles/
        /// https://github.com/StephenCleary/AsyncEx/blob/master/src/Nito.AsyncEx.Interop.WaitHandles/Interop/WaitHandleAsyncFactory.cs
        /// </remarks>
        public static async Task<bool> ToTask(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Guard.IsNotNull(handle, nameof(handle));

            // Handle synchronous cases.
            bool alreadySignaled = handle.WaitOne(0);
            if (alreadySignaled)
            {
                return true;
            }
            if (timeout == TimeSpan.Zero)
            {
                return false;
            }

            cancellationToken.ThrowIfCancellationRequested();

            RegisteredWaitHandle? registeredHandle = null;
            CancellationTokenRegistration tokenRegistration = default;

            try
            {
                var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                registeredHandle = ThreadPool.RegisterWaitForSingleObject(
                    handle,
                    (state, timedOut) => ((TaskCompletionSource<bool>)state!).TrySetResult(!timedOut),
                    tcs,
                    timeout,
                    true);
                tokenRegistration = cancellationToken.Register(state => ((TaskCompletionSource<bool>)state!).TrySetCanceled(), tcs);

                return await tcs.Task.ConfigureAwait(false);
            }
            finally
            {
                registeredHandle?.Unregister(null);
                tokenRegistration.Dispose();
            }
        }

        /// <summary>
        /// Waits for the handle to receive a signal. If the handle successfully receives a signal,
        /// <see langword="true"/> is returned. If <paramref name="timeout"/> elapses or
        /// <paramref name="cancellationToken"/> is canceled, <see langword="false"/> is returned.
        /// </summary>
        /// <param name="handle">The handle to wait on.</param>
        /// <param name="timeout">The amount of time to wait for a signal.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="handle"/> is <see langword="null"/></exception>
        /// <exception cref="T:System.Threading.AbandonedMutexException">The wait completed because a thread exited without releasing a mutex. This exception is not thrown on Windows 98 or Windows Millennium Edition.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WaitOne(this WaitHandle handle, int timeout, CancellationToken cancellationToken)
            => WaitOne(handle, TimeSpan.FromMilliseconds(timeout), cancellationToken);

        /// <summary>
        /// Waits for the handle to receive a signal. If the handle successfully receives a signal,
        /// <see langword="true"/> is returned. If <paramref name="timeout"/> elapses or
        /// <paramref name="cancellationToken"/> is canceled, <see langword="false"/> is returned.
        /// </summary>
        /// <param name="handle">The handle to wait on.</param>
        /// <param name="timeout">The amount of time to wait for a signal.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="handle"/> is <see langword="null"/></exception>
        /// <exception cref="T:System.Threading.AbandonedMutexException">The wait completed because a thread exited without releasing a mutex. This exception is not thrown on Windows 98 or Windows Millennium Edition.</exception>
        public static bool WaitOne(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Guard.IsNotNull(handle, nameof(handle));

            int indexOfElementThatTriggeredFirst = WaitHandle.WaitAny(new[] { handle, cancellationToken.WaitHandle }, timeout);
            return indexOfElementThatTriggeredFirst switch
            {
                WaitHandle.WaitTimeout =>
                // We timed out. No handle was triggered, so the wait failed.
                false,
                0 =>
                // Our desired handle received a signal. The wait was successful.
                true,
                _ => false
            };
        }

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will always
        ///     return <see langword="true"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle handle)
            => WaitOneAsync(handle, Timeout.Infinite, CancellationToken.None);

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <param name="timeout">The amount of time to wait before timing out.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will be
        ///     <see langword="true"/> if the <see cref="WaitHandle"/> was triggered before <paramref name="timeout"/>
        ///     elapses, <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle handle, int timeout)
            => WaitOneAsync(handle, TimeSpan.FromMilliseconds(timeout), CancellationToken.None);

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <param name="timeout">The amount of time to wait before timing out.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will be
        ///     <see langword="true"/> if the <see cref="WaitHandle"/> was triggered before <paramref name="timeout"/>
        ///     elapses, <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout)
            => WaitOneAsync(handle, timeout, CancellationToken.None);

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <param name="timeout">The amount of time to wait before timing out.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will be
        ///     <see langword="true"/> if the <see cref="WaitHandle"/> was triggered before <paramref name="timeout"/>
        ///     elapses, <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle handle, int timeout, CancellationToken cancellationToken)
            => WaitOneAsync(handle, TimeSpan.FromMilliseconds(timeout), cancellationToken);

        /// <summary>
        /// Creates a TPL Task that is marked as completed when a <see cref="WaitHandle"/> is signaled.
        /// </summary>
        /// <param name="handle">The handle whose signal triggers the task to be completed.</param>
        /// <param name="timeout">The amount of time to wait before timing out.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///     A <see cref="Task{T}"/> that is completed after the handle is signaled. The result will be
        ///     <see langword="true"/> if the <see cref="WaitHandle"/> was triggered before <paramref name="timeout"/>
        ///     elapses, <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
            => ToTask(handle, timeout, cancellationToken);
    }
}
