using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class IComparableExtensions
    {
        /// <summary>
        /// Clamps <paramref name="element"/> to the specified values.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="element">The element to clamp.</param>
        /// <param name="lowerBound">The first boundary.</param>
        /// <param name="upperBound">The second boundary.</param>
        /// <returns><paramref name="element"/> if it is between <paramref name="lowerBound"/> and <paramref name="upperBound"/>,
        /// <paramref name="lowerBound"/> if <paramref name="element"/> is less than <paramref name="lowerBound"/>, or <paramref name="upperBound"/>
        /// if <paramref name="element"/> is greater than <paramref name="upperBound"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        /// <exception cref="InvalidOperationException"></exception>
        [return: NotNull]
        public static T Clamp<T>([DisallowNull] this T element, [DisallowNull] T lowerBound, [DisallowNull] T upperBound)
            where T : IComparable<T>
        {
            if (element is null)
            {
                // No Guard - missing class constraint
                ThrowHelper.ThrowArgumentNullException(nameof(element));
            }
            if (lowerBound is null)
            {
                // No Guard - missing class constraint
                ThrowHelper.ThrowArgumentNullException(nameof(lowerBound));
            }
            if (upperBound is null)
            {
                // No Guard - missing class constraint
                ThrowHelper.ThrowArgumentNullException(nameof(upperBound));
            }

            if (lowerBound.CompareTo(upperBound) > 0)
            {
                ThrowArgumentOrderException();
            }

            return element.CompareTo(lowerBound) < 0
                ? lowerBound
                : element.CompareTo(upperBound) > 0
                    ? upperBound
                    : element;
        }

        /// <summary>
        /// Returns whether <paramref name="element"/> is between <paramref name="lowerBound"/> and <paramref name="upperBound"/>.
        /// </summary>
        /// <typeparam name="T">The type of the values.</typeparam>
        /// <param name="element">The element to check.</param>
        /// <param name="lowerBound">The first boundary.</param>
        /// <param name="upperBound">The second boundary.</param>
        /// <param name="lowerInclusivity">The lower inclusivity.</param>
        /// <param name="upperInclusivity">The upper inclusivity.</param>
        /// <returns><see langword="true"/> if <paramref name="element"/> is between <paramref name="lowerBound"/> and <paramref name="upperBound"/>,
        /// <see langword="false"/> otherwise.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="lowerBound"/> equals <paramref name="upperBound"/> when <paramref name="lowerInclusivity"/> does not equal <paramref name="upperInclusivity"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="element"/> is <see langword="null"/></exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static bool IsBetween<T>([DisallowNull] this T element, [DisallowNull] T lowerBound, [DisallowNull] T upperBound,
                                        EInclusivity lowerInclusivity = EInclusivity.Inclusive, EInclusivity upperInclusivity = EInclusivity.Inclusive)
            where T : IComparable<T>
        {
            if (element is null)
            {
                // No Guard - missing class constraint
                ThrowHelper.ThrowArgumentNullException(nameof(element));
            }
            if (lowerBound is null)
            {
                // No Guard - missing class constraint
                ThrowHelper.ThrowArgumentNullException(nameof(lowerBound));
            }
            if (upperBound is null)
            {
                // No Guard - missing class constraint
                ThrowHelper.ThrowArgumentNullException(nameof(upperBound));
            }

            if (lowerBound.CompareTo(upperBound) > 0)
            {
                ThrowArgumentOrderException();
            }

            GuardEx.IsValid(lowerInclusivity, nameof(lowerInclusivity));
            GuardEx.IsValid(upperInclusivity, nameof(upperInclusivity));

            switch (lowerInclusivity)
            {
                case EInclusivity.Exclusive when upperInclusivity == EInclusivity.Exclusive:
                    return element.CompareTo(lowerBound) > 0 && element.CompareTo(upperBound) < 0;
                case EInclusivity.Inclusive when upperInclusivity == EInclusivity.Exclusive:
                    if (lowerBound.CompareTo(upperBound) == 0)
                    {
                        throw new InvalidOperationException($"{nameof(lowerBound)} cannot equal {nameof(upperBound)} when {nameof(lowerInclusivity)} does not equal {nameof(upperInclusivity)}.");
                    }
                    return element.CompareTo(lowerBound) >= 0 && element.CompareTo(upperBound) < 0;
                case EInclusivity.Exclusive when upperInclusivity == EInclusivity.Inclusive:
                    if (lowerBound.CompareTo(upperBound) == 0)
                    {
                        throw new InvalidOperationException($"{nameof(lowerBound)} cannot equal {nameof(upperBound)} when {nameof(lowerInclusivity)} does not equal {nameof(upperInclusivity)}.");
                    }
                    return element.CompareTo(lowerBound) > 0 && element.CompareTo(upperBound) <= 0;
                case EInclusivity.Inclusive when upperInclusivity == EInclusivity.Inclusive:
                default:
                    return element.CompareTo(lowerBound) >= 0 && element.CompareTo(upperBound) <= 0;
            }
        }

        /// <summary>
        /// Obtains the maximum element among the provided elements.
        /// </summary>
        public static T? Max<T>(this T? first, T? other)
            where T : IComparable
        {
            if (other is null)
            {
                return first;
            }

            if (first is null)
            {
                return other;
            }

            return first.CompareTo(other) > 0
                ? first
                : other;
        }

        /// <summary>
        /// Obtains the maximum element among the provided elements.
        /// </summary>
        public static T? Max<T>(this T? first, params T?[]? others)
            where T : IComparable
        {
            if (others is null)
            {
                return first;
            }

            var max = first;
            foreach (var element in others)
            {
                if (max is null && element is not null)
                {
                    max = element;
                    continue;
                }

                if (element is null)
                {
                    continue;
                }

                if (element.CompareTo(max) > 0)
                {
                    max = element;
                }
            }

            return max;
        }

        /// <summary>
        /// Obtains the minimum element among the provided elements.
        /// </summary>
        public static T? Min<T>(this T? first, T? other)
            where T : IComparable
        {
            if (other is null)
            {
                return first;
            }

            if (first is null)
            {
                return other;
            }

            return first.CompareTo(other) < 0
                ? first
                : other;
        }

        /// <summary>
        /// Obtains the minimum element among the provided elements.
        /// </summary>
        public static T? Min<T>(this T? first, params T?[]? others)
            where T : IComparable
        {
            if (others is null)
            {
                return first;
            }

            var min = first;
            foreach (var element in others)
            {
                if (min is null && element is not null)
                {
                    min = element;
                    continue;
                }

                if (element is null)
                {
                    continue;
                }

                if (element.CompareTo(min) < 0)
                {
                    min = element;
                }
            }

            return min;
        }

        [DoesNotReturn]
        private static void ThrowArgumentOrderException()
        {
            throw new ArgumentException("The lower bound must be less than or equal to the upper bound.");
        }
    }
}
