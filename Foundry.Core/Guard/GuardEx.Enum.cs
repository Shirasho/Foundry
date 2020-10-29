using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Foundry;

namespace Microsoft.Toolkit.Diagnostics
{
    /// <summary>
    /// Helper methods not included in <see cref="Guard"/> to verify conditions when running code.
    /// </summary>
    [DebuggerStepThrough]
    public static partial class GuardEx
    {
        /// <summary>
        /// Asserts that the input value is valid for an enum of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The input value to test.</param>
        /// <param name="name">The name of the input parameter being tested.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsValid<T>(T value, string name)
            where T : struct, Enum
        {
            if (Enums<T>.Info.IsFlagEnum && Enums<T>.Info.IsValidFlagCombination(value))
            {
                return;
            }

            if (Enums<T>.Info.IsDefined(value))
            {
                return;
            }

            ThrowHelperEx.ThrowArgumentExceptionForIsValidForEnum<T>(value, name);
        }

        /// <summary>
        /// Asserts that the input value is valid for an enum of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The input value to test.</param>
        /// <param name="name">The name of the input parameter being tested.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void IsValid<T>(T? value, string name)
            where T : struct, Enum
        {
            if (!value.HasValue)
            {
                return;
            }

            if (Enums<T>.Info.IsFlagEnum && Enums<T>.Info.IsValidFlagCombination(value.Value))
            {
                return;
            }

            if (Enums<T>.Info.IsDefined(value.Value))
            {
                return;
            }

            ThrowHelperEx.ThrowArgumentExceptionForIsValidForEnum(value, name);
        }
    }
}
