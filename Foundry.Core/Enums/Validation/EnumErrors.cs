using System;
using System.Runtime.CompilerServices;

namespace Foundry
{
    internal static class EnumErrors<TEnum>
        where TEnum : struct, Enum
    {
        private static readonly Type EnumType = typeof(TEnum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string InvalidEnumValue(TEnum value)
        {
            return $"{value} is not a valid value for the enum type {EnumType}.";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string InvalidFlagEnumValue(TEnum value)
        {
            return $"{value} is not a valid flag mask for the enum type {EnumType}.";
        }
    }
}
