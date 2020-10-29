using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Threading.Tasks
{
    /// <summary>
    /// <see cref="Task"/> and <see cref="Task{TResult}"/> provider.
    /// </summary>
    public static class TaskProvider
    {
        /// <summary>
        /// A completed task with a result of <see langword="true"/>.
        /// </summary>
        public static Task<bool> TrueTask { get; } = Task.FromResult(true);

        /// <summary>
        /// A completed task with a result of <see langword="false"/>.
        /// </summary>
        public static Task<bool> FalseTask { get; } = Task.FromResult(false);

        /// <summary>
        /// A task that has already completed successfully.
        /// </summary>
        public static Task CompletedTask { get; } = Task.CompletedTask;

        /// <summary>
        /// A task that returns a task that has already completed successfully.
        /// </summary>
        public static Task<Task> CompletedTaskTask { get; } = Task.FromResult(Task.CompletedTask);
    }

    /// <summary>
    /// <see cref="Task"/> and <see cref="Task{TResult}"/> provider.
    /// </summary>
    public static class TaskProvider<T>
    {
        /// <summary>
        /// A completed task with a result of the default value of <typeparamref name="T"/>.
        /// </summary>
        public static Task<T> CompletedTask { get; } = Task.FromResult<T>(default!);

        /// <summary>
        /// A completed task with a result of an empty array of type <typeparamref name="T"/>.
        /// </summary>
        public static Task<T[]> CompletedEmptyTask { get; } = Task.FromResult(System.Array.Empty<T>());

        /// <summary>
        /// A task that returns a task that has already completed successfully.
        /// </summary>
        public static Task<Task<T>> CompletedTaskTask { get; } = Task.FromResult(CompletedTask);

        /// <summary>
        /// A task that has completed with the <see cref="TaskStatus.Canceled"/> status.
        /// </summary>
        public static Task<T> CancelledTask { get; } = Task.FromCanceled<T>(new CancellationToken(true));
    }
}
