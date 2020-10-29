using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Threading
{
    /// <summary>
    /// <see cref="CancellationToken"/> extensions.
    /// </summary>
    public static class CancellationTokenExtensions
    {
        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationToken"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="cancellationToken"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static bool IsCancelledBefore(this CancellationToken cancellationToken, int duration)
            => IsCancelledBefore(cancellationToken, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationToken"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="cancellationToken"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static Task<bool> IsCancelledBeforeAsync(this CancellationToken cancellationToken, int duration)
            => IsCancelledBeforeAsync(cancellationToken, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationToken"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="cancellationToken"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static bool IsCancelledBefore(this CancellationToken cancellationToken, TimeSpan duration)
        {
            return !IsNotCancelledAfter(cancellationToken, duration);
        }

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationToken"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="cancellationToken"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static async Task<bool> IsCancelledBeforeAsync(this CancellationToken cancellationToken, TimeSpan duration)
        {
            return !await IsNotCancelledAfterAsync(cancellationToken, duration).ConfigureAwait(false);
        }

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationToken"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="cancellationToken"/> was canceled.</returns>
        public static bool IsNotCancelledAfter(this CancellationToken cancellationToken, int duration)
            => IsNotCancelledAfter(cancellationToken, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationToken"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="cancellationToken"/> was canceled.</returns>

        public static Task<bool> IsNotCancelledAfterAsync(this CancellationToken cancellationToken, int duration)
            => IsNotCancelledAfterAsync(cancellationToken, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationToken"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="duration">The time to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="cancellationToken"/> was canceled.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static bool IsNotCancelledAfter(this CancellationToken cancellationToken, TimeSpan duration)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                return !cancellationToken.WaitHandle.WaitOne(duration);
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
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationToken"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="duration">The time to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="cancellationToken"/> was canceled.</returns>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static async Task<bool> IsNotCancelledAfterAsync(this CancellationToken cancellationToken, TimeSpan duration)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                return !await cancellationToken.WaitHandle.WaitOneAsync(duration, CancellationToken.None).ConfigureAwait(false);
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
    }
}
