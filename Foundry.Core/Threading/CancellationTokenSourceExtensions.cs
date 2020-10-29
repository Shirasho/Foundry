using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Threading
{
    /// <summary>
    /// <see cref="CancellationTokenSource"/> extensions.
    /// </summary>
    public static class CancellationTokenSourceExtensions
    {
        /// <summary>
        /// Ensure that the <see cref="CancellationTokenSource"/> has not been
        /// canceled. An <see cref="OperationCanceledException"/> will be thrown if it
        /// has.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <see langword="null"/></exception>
        /// <exception cref="ObjectDisposedException">The token source has been disposed.</exception>
        /// <exception cref="OperationCanceledException">The token has had cancellation requested.</exception>
        public static void ThrowIfCancellationRequested(this CancellationTokenSource source)
        {
            Guard.IsNotNull(source, nameof(source));

            source.Token.ThrowIfCancellationRequested();
        }

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationTokenSource"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="cancellationTokenSource"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static bool IsCancelledBefore(this CancellationTokenSource cancellationTokenSource, int duration)
            => IsCancelledBefore(cancellationTokenSource, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationTokenSource"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="cancellationTokenSource"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static Task<bool> IsCancelledBeforeAsync(this CancellationTokenSource cancellationTokenSource, int duration)
            => IsCancelledBeforeAsync(cancellationTokenSource, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationTokenSource"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="cancellationTokenSource"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static bool IsCancelledBefore(this CancellationTokenSource cancellationTokenSource, TimeSpan duration)
            => (cancellationTokenSource?.Token ?? throw new ArgumentNullException(nameof(cancellationTokenSource))).IsCancelledBefore(duration);

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationTokenSource"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if <paramref name="cancellationTokenSource"/> was canceled before the elapsed <paramref name="duration"/>, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        /// <exception cref="T:System.Runtime.InteropServices.SEHException">An unknown error occurred while accessing the <see cref="WaitHandle"/>.</exception>
        public static Task<bool> IsCancelledBeforeAsync(this CancellationTokenSource cancellationTokenSource, TimeSpan duration)
        {
            Guard.IsNotNull(cancellationTokenSource, nameof(cancellationTokenSource));

            return cancellationTokenSource.Token.IsCancelledBeforeAsync(duration);
        }

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationTokenSource"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="cancellationTokenSource"/> was canceled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        public static bool IsNotCancelledAfter(this CancellationTokenSource cancellationTokenSource, int duration)
            => IsNotCancelledAfter(cancellationTokenSource, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationTokenSource"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token source.</param>
        /// <param name="duration">The number of milliseconds to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="cancellationTokenSource"/> was canceled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        public static Task<bool> IsNotCancelledAfterAsync(this CancellationTokenSource cancellationTokenSource, int duration)
            => IsNotCancelledAfterAsync(cancellationTokenSource, TimeSpan.FromMilliseconds(duration));

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationTokenSource"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token.</param>
        /// <param name="duration">The time to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="cancellationTokenSource"/> was canceled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        public static bool IsNotCancelledAfter(this CancellationTokenSource cancellationTokenSource, TimeSpan duration)
            => (cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource))).Token.IsNotCancelledAfter(duration);

        /// <summary>
        /// Blocks until <paramref name="duration"/> elapses or <paramref name="cancellationTokenSource"/> is canceled,
        /// whichever comes first.
        /// </summary>
        /// <param name="cancellationTokenSource">The cancellation token.</param>
        /// <param name="duration">The time to sleep.</param>
        /// <returns><see langword="true"/> if the thread slept for <paramref name="duration"/>, <see langword="false"/> if <paramref name="cancellationTokenSource"/> was canceled.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cancellationTokenSource"/> is <see langword="null"/>.</exception>
        public static Task<bool> IsNotCancelledAfterAsync(this CancellationTokenSource cancellationTokenSource, TimeSpan duration)
        {
            Guard.IsNotNull(cancellationTokenSource, nameof(cancellationTokenSource));

            return cancellationTokenSource.Token.IsNotCancelledAfterAsync(duration);
        }
    }
}
