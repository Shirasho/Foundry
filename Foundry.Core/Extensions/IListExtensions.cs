using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static partial class CollectionExtensions
    {
        /// <summary>
        /// Returns this collection wrapped as a <see cref="IReadOnlyCollection{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        public static IReadOnlyCollection<T> AsReadOnly<T>(this IList<T> collection)
        {
            Guard.IsNotNull(collection, nameof(collection));

            return new ReadOnlyCollection<T>(collection);
        }

        /// <summary>
        /// Returns this collection wrapped as a <see cref="IReadOnlyList{T}"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        public static IReadOnlyList<T> AsReadOnlyList<T>(this IList<T> collection)
        {
            Guard.IsNotNull(collection, nameof(collection));

            return new ReadOnlyCollection<T>(collection);
        }

        /// <summary>
        /// Returns the index of the first occurrence of specified string element using the specified search rules.
        /// </summary>
        /// <param name="collection">The collection to search.</param>
        /// <param name="element">The element to find the index of.</param>
        /// <param name="comparisonType">The comparison rules to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        public static int IndexOf(this IList<string?> collection, string? element, StringComparison comparisonType = StringComparison.Ordinal)
        {
            Guard.IsNotNull(collection, nameof(collection));

            for (int i = 0; i < collection.Count; ++i)
            {
                if (string.Equals(collection[i], element, comparisonType))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Shuffles a collection using a Fisher Yates shuffle.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to shuffle.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException"><paramref name="collection"/> is read-only.</exception>
        public static void Shuffle<T>(this IList<T> collection)
        {
            Guard.IsNotNull(collection, nameof(collection));

            if (collection.IsReadOnly)
            {
                throw new NotSupportedException($"{nameof(collection)} is read-only.");
            }

            int count = collection.Count;
            for (int i = count - 1; i > 0; --i)
            {
                // We use i instead of i-1 since our RNG interface dictates an inclusive upper.
                int randomIndex = RandomNumberGenerator.NextInt32(i);
                var temp = collection[randomIndex];
                collection[randomIndex] = collection[i];
                collection[i] = temp;
            }
        }

        /// <summary>
        /// Inline sorts the <see cref="IList{T}"/> using the specified comparer. If the comparer
        /// is <see langword="null" />, a default comparer is used.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="collection">The collection to sort.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        public static void Sort<T>(this IList<T> collection, IComparer<T>? comparer = null)
        {
            Guard.IsNotNull(collection, nameof(collection));

            if (collection is List<T> listT)
            {
                listT.Sort(comparer);
            }
            else if (collection is IList list)
            {
#pragma warning disable DE0006 // API is deprecated
                ArrayList.Adapter(list).Sort(new ComparerBridge<T>(comparer));
#pragma warning restore DE0006 // API is deprecated
            }
            else
            {
                var copy = new List<T>(collection.Count);
                for (int i = 0; i < collection.Count; ++i)
                {
                    copy.Add(collection[i]);
                }
                copy.Sort(comparer);
                for (int i = 0; i < copy.Count; ++i)
                {
                    collection[i] = copy[i];
                }
            }
        }
    }
}
