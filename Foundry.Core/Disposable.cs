using System;
using System.Collections.Generic;
using System.Linq;

namespace Foundry
{
    /// <summary>
    /// A utility class for creating <see cref="IDisposable"/> instances.
    /// </summary>
    public static class Disposable
    {
        /// <summary>
        /// An <see cref="IDisposable"/> that does nothing when disposed.
        /// </summary>
        public static IDisposable Empty { get; } = new EmptyDisposable();

        /// <summary>
        /// Combines disposables into a single <see cref="IDisposable"/> instance.
        /// </summary>
        /// <param name="disposable">The <see cref="IDisposable"/> instance to combine.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable Combine(IDisposable? disposable)
            => new CompositeDisposable(disposable);

        /// <summary>
        /// Combines disposables into a single <see cref="IDisposable"/> instance.
        /// </summary>
        /// <param name="disposables">The <see cref="IDisposable"/> instances to combine.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable Combine(params IDisposable?[]? disposables)
            => new CompositeDisposable(disposables);

        /// <summary>
        /// Combines disposables into a single <see cref="IDisposable"/> instance.
        /// </summary>
        /// <param name="disposables">The <see cref="IDisposable"/> instances to combine.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable Combine(IEnumerable<IDisposable?>? disposables)
            => Combine(disposables?.ToArray());

        /// <summary>
        /// Combines actions into a single <see cref="IDisposable"/> instance. Exceptions thrown during
        /// action execution are suppressed.
        /// </summary>
        /// <param name="action">The action to invoke when the <see cref="IDisposable"/> is disposed.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable CreateSafe(Action? action)
            => new DelegateDisposable(action, true);

        /// <summary>
        /// Combines actions into a single <see cref="IDisposable"/> instance. Exceptions thrown during
        /// action execution are suppressed.
        /// </summary>
        /// <param name="actions">The actions to invoke when the <see cref="IDisposable"/> is disposed.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable CreateSafe(params Action?[]? actions)
            => new DelegateDisposable(actions, true);

        /// <summary>
        /// Combines actions into a single <see cref="IDisposable"/> instance. Exceptions thrown during
        /// action execution are suppressed.
        /// </summary>
        /// <param name="actions">The actions to invoke when the <see cref="IDisposable"/> is disposed.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable CreateSafe(IEnumerable<Action?>? actions)
            => CreateSafe(actions?.ToArray());

        /// <summary>
        /// Combines actions into a single <see cref="IDisposable"/> instance.
        /// </summary>
        /// <param name="action">The action to invoke when the <see cref="IDisposable"/> is disposed.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable Create(Action? action)
            => new DelegateDisposable(action, false);

        /// <summary>
        /// Combines actions into a single <see cref="IDisposable"/> instance.
        /// </summary>
        /// <param name="actions">The actions to invoke when the <see cref="IDisposable"/> is disposed.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable Create(params Action?[]? actions)
            => new DelegateDisposable(actions, false);

        /// <summary>
        /// Combines actions into a single <see cref="IDisposable"/> instance. Exceptions thrown during
        /// action execution are suppressed.
        /// </summary>
        /// <param name="actions">The actions to invoke when the <see cref="IDisposable"/> is disposed.</param>
        /// <returns>An <see cref="IDisposable"/> that encapsulates all of the provided <see cref="IDisposable"/> instances.</returns>
        public static IDisposable Create(IEnumerable<Action?>? actions)
            => Create(actions?.ToArray());

        private sealed class DelegateDisposable : IDisposable
        {
            private readonly Action?[] Actions;
            private readonly bool SuppressErrors;

            public DelegateDisposable(Action? action, bool suppressErrors)
                : this(new[] { action }, suppressErrors)
            {

            }

            public DelegateDisposable(Action?[]? actions, bool suppressErrors)
            {
                Actions = actions ?? Array.Empty<Action?>();
                SuppressErrors = suppressErrors;
            }

            public void Dispose()
            {
                foreach (var action in Actions)
                {
                    if (action is null)
                    {
                        continue;
                    }

                    try
                    {
                        action.Invoke();
                    }
                    catch when (SuppressErrors)
                    {
                        // Suppress
                    }
                }
            }
        }

        private sealed class CompositeDisposable : IDisposable
        {
            private readonly IDisposable?[] Disposables;

            public CompositeDisposable(params IDisposable?[]? disposables)
            {
                Disposables = disposables ?? Array.Empty<IDisposable?>();
            }

            public void Dispose()
            {
                foreach (var disposable in Disposables)
                {
                    if (disposable is null)
                    {
                        continue;
                    }

                    try
                    {
                        disposable.Dispose();
                    }
                    catch
                    {
                        // Suppress
                    }
                }
            }
        }

        internal sealed class EmptyDisposable : IDisposable
        {
            /// <inheritdoc />
            public void Dispose()
            {
                // Method intentionally left empty.
            }
        }
    }
}
