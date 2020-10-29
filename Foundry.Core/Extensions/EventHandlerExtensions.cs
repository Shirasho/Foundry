using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class EventHandlerExtensions
    {
        /// <summary>
        /// Invokes <paramref name="handler"/> in such a way that the handler will not be
        /// nullified between access and invocation.
        /// </summary>
        /// <param name="handler">The event to invoke.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeInvoke(this EventHandler? handler)
            => SafeInvoke(handler, null, EventArgs.Empty);

        /// <summary>
        /// Invokes <paramref name="handler"/> in such a way that the handler will not be
        /// nullified between access and invocation.
        /// </summary>
        /// <param name="handler">The event to invoke.</param>
        /// <param name="sender">The sender.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeInvoke(this EventHandler? handler, object? sender)
            => SafeInvoke(handler, sender, EventArgs.Empty);

        /// <summary>
        /// Invokes <paramref name="handler"/> in such a way that the handler will not be
        /// nullified between access and invocation.
        /// </summary>
        /// <param name="handler">The event to invoke.</param>
        /// <param name="e">The event args.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeInvoke(this EventHandler? handler, EventArgs e)
            => SafeInvoke(handler, null, e);

        /// <summary>
        /// Invokes <paramref name="handler"/> in such a way that the handler will not be
        /// nullified between access and invocation.
        /// </summary>
        /// <param name="handler">The event to invoke.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeInvoke(this EventHandler? handler, object? sender, EventArgs e)
        {
            Guard.IsNotNull(e, nameof(e));

            var copy = handler;
            copy?.Invoke(sender, e);
        }

        /// <summary>
        /// Invokes <paramref name="handler"/> in such a way that the handler will not be
        /// nullified between access and invocation.
        /// </summary>
        /// <param name="handler">The event to invoke.</param>
        /// <param name="e">The event args.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeInvoke<T>(this EventHandler<T>? handler, [DisallowNull] T e)
            where T : EventArgs => SafeInvoke(handler, null, e);

        /// <summary>
        /// Invokes <paramref name="handler"/> in such a way that the handler will not be
        /// nullified between access and invocation.
        /// </summary>
        /// <param name="handler">The event to invoke.</param>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SafeInvoke<T>(this EventHandler<T>? handler, object? sender, [DisallowNull] T e)
            where T : EventArgs
        {
            Guard.IsNotNull(e, nameof(e));

            var copy = handler;
            copy?.Invoke(sender, e);
        }
    }
}
