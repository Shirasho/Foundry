using System;
using System.Diagnostics.CodeAnalysis;

namespace Foundry
{
    /// <summary>
    /// A helper class for throwing extensions.
    /// </summary>
    public static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentNullException(string argumentName)
        {
            throw new ArgumentNullException(argumentName);
        }

        [DoesNotReturn]
        public static void ThrowEmptyCollectionError()
        {
            throw new InvalidOperationException("The collection is empty.");
        }

        [DoesNotReturn]
        public static void ThrowInvalidEnumValue<TEnum>(TEnum value)
            where TEnum : struct, Enum => ThrowInvalidEnumValue(value, typeof(TEnum));

        [DoesNotReturn]
        internal static void ThrowInvalidEnumValue<TEnum>(TEnum value, Type enumType)
            where TEnum : struct, Enum
        {
            throw new ArgumentOutOfRangeException($"{value} is not a valid {enumType.Name} enum value.");
        }

        [DoesNotReturn]
        public static void ThrowInvalidEnumValue<TEnum>(string value)
            where TEnum : struct, Enum => ThrowInvalidEnumValue(value, typeof(TEnum));

        [DoesNotReturn]
        internal static void ThrowInvalidEnumValue(string value, Type enumType)
        {
            throw new FormatException($"The value {value} is not a member of type {enumType} or is outside the underlying type's range.");
        }
    }
}
