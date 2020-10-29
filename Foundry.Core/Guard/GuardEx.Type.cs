using System;
using System.Runtime.CompilerServices;
using Foundry.Reflection;

namespace Microsoft.Toolkit.Diagnostics
{
    /// <summary>
    /// Helper methods not included in <see cref="Guard"/> to verify conditions when running code.
    /// </summary>
    public static partial class GuardEx
    {
        /// <summary>
        /// Asserts that the input value is assignable to type <paramref name="typeOf"/>.
        /// </summary>
        /// <param name="value">The input value to test.</param>
        /// <param name="name">The name of the input parameter being tested.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsTypeOf<T>(Type value, string name)
        {
            if (value.IsTypeOf<T>())
            {
                return;
            }

            ThrowHelperEx.ThrowArgumentExceptionForIsTypeOf(value, typeof(T), name);
        }

        /// <summary>
        /// Asserts that the input value is assignable to type <paramref name="typeOf"/>.
        /// </summary>
        /// <param name="value">The input value to test.</param>
        /// <param name="name">The name of the input parameter being tested.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsTypeOf(Type value, Type typeOf, string name)
        {
            if (value.IsTypeOf(typeOf))
            {
                return;
            }

            ThrowHelperEx.ThrowArgumentExceptionForIsTypeOf(value, typeOf, name);
        }
    }
}
