using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Toolkit.Diagnostics
{
    internal static class ThrowHelperEx
    {
        /// <summary>
        /// Returns a formatted representation of the input value.
        /// </summary>
        /// <param name="obj">The input <see cref="object"/> to format.</param>
        /// <returns>A formatted representation of <paramref name="obj"/> to display in error messages.</returns>
        private static string ToAssertString(this object? obj)
        {
            return obj switch
            {
                string _ => $"\"{obj}\"",
                null => "null",
                _ => $"<{obj}>"
            };
        }

        /// <summary>
        /// Returns a formatted representation of the input value.
        /// </summary>
        /// <param name="obj">The input <see cref="object"/> to format.</param>
        /// <returns>A formatted representation of <paramref name="obj"/> to display in error messages.</returns>
        private static string ToAssertString<T>(this T? obj)
            where T : struct
        {
            return ToAssertString(obj.HasValue ? (object)obj.Value : null);
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> when <see cref="GuardEx.IsValid{T}(T?, string)"/> (where <typeparamref name="T"/> is
        /// <see langword="struct"/> + <see cref="Enum"/>) fails.
        /// </summary>
        /// <typeparam name="T">The type of the input value.</typeparam>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void ThrowArgumentExceptionForIsValidForEnum<T>(T? value, string name)
            where T : struct, Enum
        {
            ThrowArgumentException(name, $"Parameter {name.ToAssertString()} ({typeof(T).ToTypeString()}) must be a valid enum value or flag combination, was {value.ToAssertString()}.");
        }

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> when <see cref="GuardEx.IsTypeOf(Type, Type, string)"/> fails.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [DoesNotReturn]
        public static void ThrowArgumentExceptionForIsTypeOf(Type value, Type genericTypeDefinition, string name)
        {
            ThrowArgumentException(name, $"Parameter {name.ToAssertString()} ({value.ToTypeString()}) must be of type {genericTypeDefinition.ToTypeString()}.");
        }

        /// <summary>
        /// Throws a new <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="name">The argument name.</param>
        /// <param name="message">The message to include in the exception.</param>
        /// <exception cref="ArgumentException">Thrown with the specified parameters.</exception>
        [DoesNotReturn]
        public static void ThrowArgumentException(string name, string message)
        {
            throw new ArgumentException(message, name);
        }
    }
}
