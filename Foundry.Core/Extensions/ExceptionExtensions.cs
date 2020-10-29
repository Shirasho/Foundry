using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Foundry.Reflection;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Returns an enumerable of inner exceptions in order from outermost to innermost.
        /// The original exception will always be first in the collection.
        /// </summary>
        /// <param name="e">The parent <see cref="Exception"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <see langword="null"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<Exception> GetInnerExceptions(this Exception e)
            => GetInnerExceptions(e, true);

        /// <summary>
        /// Returns an enumerable of inner exceptions in order from outermost to innermost.
        /// The original exception will always be first in the collection.
        /// </summary>
        /// <param name="e">The parent <see cref="Exception"/>.</param>
        /// <param name="extractAggregateExceptions">Whether to extract out the <see cref="AggregateException"/>s contained in inner exceptions.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <see langword="null"/></exception>
        public static IEnumerable<Exception> GetInnerExceptions(this Exception e, bool extractAggregateExceptions)
        {
            Guard.IsNotNull(e, nameof(e));

            return _(); IEnumerable<Exception> _()
            {
                yield return e;

                var ex = e.InnerException;
                while (ex != null)
                {
                    if (extractAggregateExceptions && ex is AggregateException ae)
                    {
                        foreach (var aei in ae.InnerExceptions)
                        {
                            yield return aei;
                        }
                    }
                    else
                    {
                        yield return ex;
                    }
                    ex = ex.InnerException;
                }
            }
        }

        /// <summary>
        /// Returns the first inner exception of the specified type. If <paramref name="e"/>
        /// is of type <typeparamref name="T"/>, <paramref name="e"/> is returned.
        /// </summary>
        /// <typeparam name="T">The exception type to find.</typeparam>
        /// <param name="e">The outer exception.</param>
        /// <param name="strictType">Whether the type to find must match exactly instead of accepting a child type.</param>
        /// <exception cref="Reflection.TargetInvocationException">A static initializer is invoked and throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: MaybeNull]
        public static T GetInnerExceptionOfType<T>(this Exception? e, bool strictType = true)
            where T : Exception => GetInnerExceptionOfType(e, typeof(T), strictType) as T;

        /// <summary>
        /// Returns the first inner exception of the specified type. If <paramref name="e"/>
        /// is of type <paramref name="type"/>, <paramref name="e"/> is returned.
        /// </summary>
        /// <param name="e">The outer exception.</param>
        /// <param name="type">The exception type to find.</param>
        /// <param name="strictType">Whether the type to find must match exactly instead of accepting a child type.</param>
        /// <exception cref="ArgumentException"><paramref name="type"/> is not of type <see cref="Exception"/>.</exception>
        /// <exception cref="Reflection.TargetInvocationException">A static initializer is invoked and throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Exception? GetInnerExceptionOfType(this Exception? e, Type type, bool strictType = true)
        {
            Guard.IsNotNull(type, nameof(type));
            GuardEx.IsTypeOf<Exception>(type, nameof(type));

            if (e is null)
            {
                return null;
            }

            foreach (var ex in GetInnerExceptions(e))
            {
                if (strictType ? ex.GetType() == type : ex.GetType().IsTypeOf(type))
                {
                    return ex;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the innermost <see cref="Exception"/>.
        /// </summary>
        /// <param name="e">The parent <see cref="Exception"/>.</param>
        /// <param name="depth">The maximum depth to go when searching. A negative number will search the entire way.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <see langword="null"/></exception>
        public static Exception GetInnermostException(this Exception e, int depth = -1)
        {
            Guard.IsNotNull(e, nameof(e));

            if (depth < 0)
            {
                depth = int.MaxValue;
            }

            var inner = e;
            int searchDepth = 0;

            if (inner is AggregateException ae)
            {
                inner = ae.InnerException ?? (ae.InnerExceptions.Count > 0 ? ae.InnerExceptions[0] : ae);
            }

            while (inner.InnerException != null && searchDepth < depth)
            {
                inner = inner.InnerException;
                if (inner is AggregateException ae2)
                {
                    inner = ae2.InnerException ?? (ae2.InnerExceptions.Count > 0 ? ae2.InnerExceptions[0] : ae2);
                }

                ++searchDepth;
            }
            return inner;
        }

        /// <summary>
        /// Returns the message of the innermost exception.
        /// </summary>
        /// <param name="e">The outer exception.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetInnermostExceptionMessage(this Exception e)
            => GetInnermostException(e).Message;

        /// <summary>
        /// Returns the message of the innermost exception.
        /// </summary>
        /// <param name="e">The outer exception.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetInnermostExceptionMessage(this Exception e, int depth)
            => GetInnermostException(e, depth).Message;

        /// <summary>
        /// Returns whether this exception represents a file or device-sharing violation.
        /// </summary>
        /// <param name="e">The exception.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <see langword="null"/></exception>
        /// <exception cref="PlatformNotSupportedException">This method is called on a non-Windows platform.</exception>
        public static bool IsSharingViolation(this Exception e)
        {
            Guard.IsNotNull(e, nameof(e));

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                throw new PlatformNotSupportedException();
            }

            // ERROR_SHARING_VIOLATION
            return (uint)Marshal.GetHRForException(e) == 0x80070020;
        }

        /// <summary>
        /// Restores the context of an inner exception and throws it. If there are no
        /// inner exceptions, <paramref name="e"/> is thrown instead.
        /// </summary>
        /// <param name="e">The exception containing the inner exception throw.</param>
        /// <param name="depth">The maximum depth to go when searching. A negative number will search the entire way.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <see langword="null"/></exception>
        [DebuggerHidden]
        [DoesNotReturn]
        public static Exception RestoreAndThrow(this Exception e, int depth = 0)
        {
            Guard.IsNotNull(e, nameof(e));

            if (depth < 0)
            {
                depth = int.MaxValue;
            }

            var exceptions = GetInnerExceptions(e, false).ToArray();
            int index = Math.Min(exceptions.Length - 1, depth);

            // Does the actual throwing.
            ExceptionDispatchInfo.Capture(exceptions[index]).Throw();

#pragma warning disable CS8763
            // Damned if I do return ([DoesNotReturn]), damned if I don't (Not all code paths return a value).
            // The former is a compiler hint that can be suppressed. The latter is a compiler error that can't be.
            // Suppress the compiler hint.
            return exceptions[index];
#pragma warning restore CS8763
        }

        /// <summary>
        /// Restores the context of an inner exception and throws it. If there are no
        /// inner exceptions, <paramref name="e"/> is thrown instead.
        /// </summary>
        /// <param name="e">The exception containing the inner exception throw.</param>
        /// <param name="depth">The maximum depth to go when searching. A negative number will search the entire way.</param>
        /// <param name="flatten">Whether to flatten the <see cref="AggregateException"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="e"/> is <see langword="null"/></exception>
        [DebuggerHidden, DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DoesNotReturn]
        public static Exception RestoreAndThrow(this AggregateException e, int depth = 0, bool flatten = true)
        {
            Guard.IsNotNull(e, nameof(e));

            return RestoreAndThrow(flatten ? e.Flatten() : e, depth);
        }

        /// <summary>
        /// Restores the context of the innermost exception and throws it.
        /// </summary>
        /// <param name="e">The exception to throw.</param>
        /// <remarks>See Documentation/ExceptionHandling.txt</remarks>
        [DebuggerHidden, DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DoesNotReturn]
        public static Exception RestoreInnermostAndRethrow(this Exception e)
            => RestoreAndThrow(e, -1);

        /// <summary>
        /// Restores the context of the innermost exception and throws it.
        /// </summary>
        /// <param name="e">The exception to throw.</param>
        /// <param name="flatten">Whether to flatten the <see cref="AggregateException"/>.</param>
        /// <remarks>See Documentation/ExceptionHandling.txt</remarks>
        [DebuggerHidden, DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DoesNotReturn]
        public static Exception RestoreInnermostAndRethrow(this AggregateException e, bool flatten)
            => RestoreAndThrow(e, -1, flatten);
    }
}
