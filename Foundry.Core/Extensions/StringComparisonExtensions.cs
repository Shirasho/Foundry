using System;

namespace Foundry
{
    public static class StringComparisonExtensions
    {
        /// <summary>
        /// Returns the equivalent <see cref="StringComparer"/> for a <see cref="StringComparison"/> value.
        /// </summary>
        /// <param name="stringComparison">The value to get the <see cref="StringComparer"/> for.</param>
        /// <exception cref="ArgumentException"><paramref name="stringComparison"/> is not valid.</exception>
        public static StringComparer ToStringComparer(this StringComparison stringComparison)
        {
            return stringComparison switch
            {
                StringComparison.CurrentCulture => StringComparer.CurrentCulture,
                StringComparison.CurrentCultureIgnoreCase => StringComparer.CurrentCultureIgnoreCase,
                StringComparison.InvariantCulture => StringComparer.InvariantCulture,
                StringComparison.InvariantCultureIgnoreCase => StringComparer.InvariantCultureIgnoreCase,
                StringComparison.Ordinal => StringComparer.Ordinal,
                StringComparison.OrdinalIgnoreCase => StringComparer.OrdinalIgnoreCase,
                _ => throw new ArgumentException("Invalid comparison type.", nameof(stringComparison))
            };
        }

        /// <summary>
        /// Returns the equivalent <see cref="StringComparison"/> for a <see cref="StringComparer"/> value.
        /// </summary>
        /// <param name="stringComparer">The value to get the <see cref="StringComparison"/> for.</param>
        /// <exception cref="NotSupportedException"><paramref name="stringComparer"/> does not have an equivalent <see cref="StringComparison"/>.</exception>
        public static StringComparison ToStringComparison(this StringComparer stringComparer)
        {
            if (ReferenceEquals(stringComparer, StringComparer.CurrentCulture))
            {
                return StringComparison.CurrentCulture;
            }
            if (ReferenceEquals(stringComparer, StringComparer.CurrentCultureIgnoreCase))
            {
                return StringComparison.CurrentCultureIgnoreCase;
            }
            if (ReferenceEquals(stringComparer, StringComparer.InvariantCulture))
            {
                return StringComparison.InvariantCulture;
            }
            if (ReferenceEquals(stringComparer, StringComparer.InvariantCultureIgnoreCase))
            {
                return StringComparison.InvariantCultureIgnoreCase;
            }
            if (ReferenceEquals(stringComparer, StringComparer.Ordinal))
            {
                return StringComparison.Ordinal;
            }
            if (ReferenceEquals(stringComparer, StringComparer.OrdinalIgnoreCase))
            {
                return StringComparison.OrdinalIgnoreCase;
            }

            throw new NotSupportedException("Unsupported comparer type.");
        }
    }
}
