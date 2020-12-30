using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class ProcessExtensions
    {
        /// <summary>
        /// Waits asynchronously for the <see cref="Process"/> to exit.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The process exit code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<int> WaitForExitAsync(this Process process, CancellationToken cancellationToken = default)
            => WaitForExitAsync(process, true, cancellationToken);

        /// <summary>
        /// Waits asynchronously for the <see cref="Process"/> to exit.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <param name="killOnCancellation">Whether to kill the process if <paramref name="cancellationToken"/> is canceled before the process exits.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The process exit code.</returns>
        public static async Task<int> WaitForExitAsync(this Process process, bool killOnCancellation, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(process, nameof(process));

            if (process.HasExited)
            {
                return 0;
            }

            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            using (cancellationToken.Register(() =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    tcs.TrySetCanceled(cancellationToken);
                    if (killOnCancellation)
                    {
                        process.Kill();
                    }
                }
                else
                {
                    // Wrap in a task to avoid deadlocks on OSX and Unix.
                    Task.Run(() =>
                    {
                        tcs.TrySetCanceled(cancellationToken);
                        if (killOnCancellation)
                        {
                            process.Kill();
                        }
                    }, CancellationToken.None);
                }
            }))
            {
                process.EnableRaisingEvents = true;

                void OnProcessOnExited(object? sender, EventArgs args)
                {
                    tcs.TrySetResult(process.ExitCode);
                    process.Exited -= OnProcessOnExited;
                }

                process.Exited += OnProcessOnExited;
                if (process.HasExited)
                {
                    process.Exited -= OnProcessOnExited;
                    return process.ExitCode;
                }
                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Waits asynchronously for the <see cref="Process"/> to exit.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <param name="timeout">The amount of time to wait for the process to exit.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The process exit code.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task<int> WaitForExitAsync(this Process process, TimeSpan timeout, CancellationToken cancellationToken = default)
            => WaitForExitAsync(process, timeout, true, cancellationToken);

        /// <summary>
        /// Waits asynchronously for the <see cref="Process"/> to exit.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <param name="timeout">The amount of time to wait for the process to exit.</param>
        /// <param name="killOnCancellation">Whether to kill the process if <paramref name="cancellationToken"/> is canceled or <paramref name="timeout"/> elapses before the process exits.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The process exit code.</returns>
        public static async Task<int> WaitForExitAsync(this Process process, TimeSpan timeout, bool killOnCancellation, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(process, nameof(process));

            if (process.HasExited)
            {
                return 0;
            }

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            process.EnableRaisingEvents = true;

            void OnProcessOnExited(object? sender, EventArgs args)
            {
                tcs.TrySetResult(process.ExitCode);
                process.Exited -= OnProcessOnExited;
            }

            process.Exited += OnProcessOnExited;

            cts.Token.Register(() =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    tcs.TrySetCanceled(cancellationToken);
                    if (killOnCancellation)
                    {
                        process.Kill();
                    }
                }
                else
                {
                    // Wrap in a task to avoid deadlocks on OSX and Unix.
                    Task.Run(() =>
                    {
                        tcs.TrySetCanceled(cancellationToken);
                        if (killOnCancellation)
                        {
                            process.Kill();
                        }
                    }, CancellationToken.None);
                }
            });
            if (process.HasExited)
            {
                process.Exited -= OnProcessOnExited;
                return process.ExitCode;
            }
            return await tcs.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Waits asynchronously for <paramref name="process"/> to start.
        /// </summary>
        /// <param name="process">The process to wait for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="InvalidOperationException">The process has exited.</exception>
        public static async Task WaitForStartAsync(this Process process, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(process, nameof(process));

            if (process.HasExited)
            {
                return;
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (process.HasExited)
                {
                    throw new InvalidOperationException("The process has exited.");
                }

                try
                {
                    _ = process.StartTime;
                    return;
                }
                catch (Exception)
                {
                    await Task.Delay(10, CancellationToken.None).ConfigureAwait(false);
                }
            }
        }
    }
}
