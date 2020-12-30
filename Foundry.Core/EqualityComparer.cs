using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Foundry
{
    /// <summary>
    /// Determines the equality of two objects.
    /// </summary>
    /// <typeparam name="T">The type of object.</typeparam>
    /// <param name="x">The first object.</param>
    /// <param name="y">The second object.</param>
    public delegate bool Equality<in T>(T? x, T? y);

    /// <summary>
    /// Extension of <see cref="EqualityComparer{T}"/>.
    /// </summary>
    public static class EqualityComparer
    {
        /// <summary>
        /// Creates a <see cref="IEqualityComparer{T}"/> using the provided comparison.
        /// </summary>
        /// <typeparam name="T">The type to compare.</typeparam>
        /// <param name="comparison">The method to use to determine equality.</param>
        public static IEqualityComparer<T?> Create<T>(Equality<T?>? comparison)
            => Create(comparison, null);

        /// <summary>
        /// Creates a <see cref="IEqualityComparer{T}"/> using the provided comparison.
        /// </summary>
        /// <typeparam name="T">The type to compare.</typeparam>
        /// <param name="hashCode">The method to use to determine hash code.</param>
        public static IEqualityComparer<T?> Create<T>(Func<T?, int>? hashCode)
            => Create(null, hashCode);

        /// <summary>
        /// Creates a <see cref="IEqualityComparer{T}"/> using the provided comparison.
        /// </summary>
        /// <typeparam name="T">The type to compare.</typeparam>
        /// <param name="comparison">The method to use to determine equality.</param>
        /// <param name="hashCode">The method to use to determine hash code.</param>
        public static IEqualityComparer<T?> Create<T>(Equality<T?>? comparison, Func<T?, int>? hashCode)
        {
            return new CustomEqualityComparer<T?>(comparison, hashCode);
        }

        private class CustomEqualityComparer<T> : IEqualityComparer<T>
        {
            private static readonly IEqualityComparer<T?> DefaultComparer = EqualityComparer<T?>.Default;
            private static readonly Func<T, int> DefaultComparerHashCode = DefaultComparer.GetHashCode!;

            [NotNull]
            private Equality<T> Comparison { get; }

            [NotNull]
            private Func<T, int> HashCode { get; }

            public CustomEqualityComparer(Equality<T?>? comparison, Func<T, int>? hashCode)
            {
                Comparison = comparison ?? new Equality<T>(DefaultComparer.Equals);
                HashCode = hashCode ?? DefaultComparerHashCode;
            }

            /// <inheritdoc />
            public bool Equals(T? x, T? y)
            {
                return Comparison(x, y);
            }

            /// <inheritdoc />
            public int GetHashCode(T? obj)
            {
                return obj is not null ? HashCode(obj) : 0;
            }
        }
    }
}
