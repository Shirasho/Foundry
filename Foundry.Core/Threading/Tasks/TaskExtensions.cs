using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Threading.Tasks
{
    /// <summary>
    /// <see cref="Task"/> and <see cref="Task{TResult}"/> extensions.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Waits for the task to finish running and unwraps any exceptions that were thrown.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void GetResultAndUnwrapExceptions(this Task task)
            => GetResultAndUnwrapExceptions(task, false);

        /// <summary>
        /// Waits for the task to finish running and unwraps any exceptions that were thrown.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="continueOnCapturedContext"></param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void GetResultAndUnwrapExceptions(this Task task, bool continueOnCapturedContext)
        {
            Guard.IsNotNull(task, nameof(task));

            task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for the task to finish running and unwraps any exceptions that were thrown.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        [return: MaybeNull]
        public static T GetResultAndUnwrapExceptions<T>(this Task<T> task)
            => GetResultAndUnwrapExceptions(task, false);

        /// <summary>
        /// Waits for the task to finish running and unwraps any exceptions that were thrown.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="continueOnCapturedContext"></param>
        [DebuggerHidden]
        [DebuggerStepThrough]
        [return: MaybeNull]
        public static T GetResultAndUnwrapExceptions<T>(this Task<T> task, bool continueOnCapturedContext)
        {
            Guard.IsNotNull(task, nameof(task));

            return task.ConfigureAwait(continueOnCapturedContext).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Waits for the task to finish running and unwraps any exceptions that were thrown.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <remarks>Alias for <see cref="GetResultAndUnwrapExceptions(Task)"/>.</remarks>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void WaitAndUnwrapExceptions(this Task task)
            => WaitAndUnwrapExceptions(task, false);

        /// <summary>
        /// Waits for the task to finish running and unwraps any exceptions that were thrown.
        /// </summary>
        /// <param name="task">The task to wait for.</param>
        /// <param name="continueOnCapturedContext"></param>
        /// <remarks>Alias for <see cref="GetResultAndUnwrapExceptions(Task, bool)"/>.</remarks>
        [DebuggerHidden]
        [DebuggerStepThrough]
        public static void WaitAndUnwrapExceptions(this Task task, bool continueOnCapturedContext)
            => GetResultAndUnwrapExceptions(task, continueOnCapturedContext);

#if !NETSTANDARD2_1
        /// <summary>
        /// Sets <paramref name="taskCompletionSource"/> based on the result of <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task that drives the completion source data.</param>
        /// <param name="taskCompletionSource">The <see cref="TaskCompletionSource"/> to set.</param>
        public static async Task SetCompletionSource(this Task task, TaskCompletionSource taskCompletionSource)
        {
            Guard.IsNotNull(task, nameof(task));
            Guard.IsNotNull(taskCompletionSource, nameof(taskCompletionSource));

            await task.ConfigureAwait(false);

            if (task.IsFaulted && task.Exception is not null)
            {
                taskCompletionSource.TrySetException(task.Exception ?? new Exception("A task has faulted."));
            }
            else if (task.IsCanceled)
            {
                taskCompletionSource.TrySetCanceled();
            }
            else
            {
                taskCompletionSource.TrySetResult();
            }
        }
#endif

        /// <summary>
        /// Sets <paramref name="taskCompletionSource"/> based on the result of <paramref name="task"/>.
        /// </summary>
        /// <param name="task">The task that drives the completion source data.</param>
        /// <param name="taskCompletionSource">The <see cref="TaskCompletionSource"/> to set.</param>
        public static async Task<T> SetCompletionSource<T>(this Task<T> task, TaskCompletionSource<T> taskCompletionSource)
        {
            Guard.IsNotNull(task, nameof(task));
            Guard.IsNotNull(taskCompletionSource, nameof(taskCompletionSource));

            var result = await task.ConfigureAwait(false);

            if (task.IsFaulted && task.Exception is not null)
            {
                taskCompletionSource.TrySetException(task.Exception ?? new Exception("A task has faulted."));
            }
            else if (task.IsCanceled)
            {
                taskCompletionSource.TrySetCanceled();
            }
            else
            {
                taskCompletionSource.TrySetResult(result);
            }

            return result;
        }

        /// <summary>
        /// Runs the task as an async void while swallowing exceptions.
        /// </summary>
        /// <param name="task">The task to fire and forget.</param>
        /// <remarks>
        /// This works around the issue that async void has in that uncaught exceptions can
        /// crash the app domain since errors are caught and sent to the error handler.
        /// </remarks>
        public static void ToAsyncVoid(this Task task)
            => ToAsyncVoid(task, Noop<Exception>.Instance);

        /// <summary>
        /// Runs the task as an async void using the specified error handler.
        /// </summary>
        /// <param name="task">The task to fire and forget.</param>
        /// <param name="errorHandler">The error handler to use for handling errors.</param>
        /// <remarks>
        /// This works around the issue that async void has in that uncaught exceptions can
        /// crash the app domain since errors are caught and sent to the error handler.
        /// </remarks>
        public static async void ToAsyncVoid(this Task task, Action<Exception> errorHandler)
        {
            Guard.IsNotNull(task, nameof(task));
            Guard.IsNotNull(errorHandler, nameof(errorHandler));

            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception e)
            {
                errorHandler(e);
            }
        }

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to monitor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task that completed first.</returns>
        public static Task WhenAll(this IEnumerable<Task>? tasks, CancellationToken cancellationToken = default)
        {
            return tasks is null
                ? Task.FromResult(Task.CompletedTask)
                : Task.WhenAll(tasks).WithCancellation(cancellationToken);
        }

        /// <summary>
        /// Creates a task that will complete when all of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to monitor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task that completed first.</returns>
        public static Task WhenAll<TResult>(this IEnumerable<Task<TResult>>? tasks, CancellationToken cancellationToken = default)
        {
            return tasks is null
                ? Task.FromResult(Task.FromResult(default(TResult)!))
                : (Task)Task.WhenAll(tasks).WithCancellation(cancellationToken);
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to monitor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task that completed first.</returns>
        public static Task<Task> WhenAny(this IEnumerable<Task>? tasks, CancellationToken cancellationToken = default)
        {
            return tasks is null
                ? TaskProvider.CompletedTaskTask
                : Task.WhenAny(tasks).WithCancellation(cancellationToken);
        }

        /// <summary>
        /// Creates a task that will complete when any of the supplied tasks have completed.
        /// </summary>
        /// <param name="tasks">The tasks to monitor.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task that completed first.</returns>
        public static Task<Task<TResult>> WhenAny<TResult>(this IEnumerable<Task<TResult>>? tasks, CancellationToken cancellationToken = default)
        {
            return tasks is null
                ? TaskProvider<TResult>.CompletedTaskTask
                : Task.WhenAny(tasks).WithCancellation(cancellationToken);
        }

        /// <summary>
        /// Returns the first task to complete successfully where the result of the task
        /// fulfills the specified predicate.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="tasks">The tasks to monitor.</param>
        /// <param name="predicate">The predicate to test the result against.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task that completed first.</returns>
        /// <remarks>
        /// https://github.com/NuGet/NuGet2/blob/2.13/src/Core/Extensions/TaskExtensions.cs
        /// </remarks>
        [SuppressMessage("Performance", "HAA0301:Closure Allocation Source", Justification = "Intentional capture.")]
        [SuppressMessage("Performance", "HAA0302:Display class allocation to capture closure", Justification = "Intentional capture.")]
        public static Task<TResult> WhenAny<TResult>(this IEnumerable<Task<TResult>>? tasks, Predicate<TResult> predicate, CancellationToken cancellationToken = default)
        {
            if (tasks is null)
            {
                return TaskProvider<TResult>.CompletedTask;
            }

            var t = tasks.ToArray();
            int numTasksRemaining = t.Length;
            var tcs = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);

            foreach (var task in t)
            {
                task.ContinueWith(innerTask =>
                {
                    if (innerTask.Status == TaskStatus.RanToCompletion && (predicate is null || predicate(innerTask.Result)))
                    {
                        tcs.TrySetResult(innerTask.Result);
                    }

                    if (Interlocked.Decrement(ref numTasksRemaining) == 0)
                    {
                        tcs.TrySetResult(default!);
                    }
                }, TaskContinuationOptions.ExecuteSynchronously);
            }

            return tcs.Task.WithCancellation(cancellationToken);
        }

        /// <summary>
        /// Adds cancellation support to a task that does not accept a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="task">The task to add cancellation support for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/></exception>
        public static Task WithCancellation(this Task task, CancellationToken cancellationToken)
        {
            Guard.IsNotNull(task, nameof(task));

            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            var registration = cancellationToken.Register(s =>
            {
                var source = (TaskCompletionSource<int>)s!;
                source.TrySetCanceled();
            }, tcs);

            task.ContinueWith((t, s) =>
            {
                var tcsAndRegistration = (Tuple<TaskCompletionSource<int>, CancellationTokenRegistration>)s!;

                if (t.IsFaulted && t.Exception is not null)
                {
                    tcsAndRegistration.Item1.TrySetException(t.Exception.GetBaseException());
                }

                if (t.IsCanceled)
                {
                    tcsAndRegistration.Item1.TrySetCanceled();
                }

                if (t.IsCompleted)
                {
                    tcsAndRegistration.Item1.TrySetResult(0);
                }

                tcsAndRegistration.Item2.Dispose();
            }, new Tuple<TaskCompletionSource<int>, CancellationTokenRegistration>(tcs, registration), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return tcs.Task;
        }

        /// <summary>
        /// Adds cancellation support to a task that does not accept a <see cref="CancellationToken"/>.
        /// </summary>
        /// <param name="task">The task to add cancellation support for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ArgumentNullException"><paramref name="task"/> is <see langword="null"/></exception>
        public static Task<TResult> WithCancellation<TResult>(this Task<TResult> task, CancellationToken cancellationToken)
        {
            Guard.IsNotNull(task, nameof(task));

            var tcs = new TaskCompletionSource<TResult>();
            var registration = cancellationToken.Register(s =>
            {
                var source = (TaskCompletionSource<TResult>)s!;
                source.TrySetCanceled();
            }, tcs);

            task.ContinueWith((t, s) =>
            {
                var tcsAndRegistration = (Tuple<TaskCompletionSource<TResult>, CancellationTokenRegistration>)s!;

                if (t.IsFaulted && t.Exception is not null)
                {
                    tcsAndRegistration.Item1.TrySetException(t.Exception.GetBaseException());
                }

                if (t.IsCanceled)
                {
                    tcsAndRegistration.Item1.TrySetCanceled();
                }

                if (t.IsCompleted)
                {
                    tcsAndRegistration.Item1.TrySetResult(t.Result);
                }

                tcsAndRegistration.Item2.Dispose();
            }, Tuple.Create(tcs, registration), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return tcs.Task;
        }

        /// <summary>
        /// Correctly aggregates errors from a <see cref="Task.WhenAll(Task[])"/> call.
        /// </summary>
        /// <param name="task">The task returned from <see cref="Task.WhenAll(Task[])"/>.</param>
        /// <returns>A task that will have correct error handling. The original task may be returned.</returns>
        public static Task WithWhenAllAggregatedExceptions(this Task task)
        {
            Guard.IsNotNull(task, nameof(task));

            return task.ContinueWith(t => t.IsFaulted && t.Exception is AggregateException ae && (ae.InnerExceptions.Count > 1 || ae.InnerException is AggregateException)
                    ? Task.FromException(ae.Flatten())
                    : t, TaskContinuationOptions.ExecuteSynchronously).Unwrap();
        }
    }
}
