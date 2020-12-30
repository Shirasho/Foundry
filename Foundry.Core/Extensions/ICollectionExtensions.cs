using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Foundry.Numerics.Random;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    /// <summary>
    /// Collection extensions.
    /// </summary>
    public static partial class CollectionExtensions
    {
        /// <summary>
        /// A random number generator to use for extension methods.
        /// </summary>
        private static readonly IRandomNumberProvider RandomNumberGenerator = new MersenneTwisterRandom();

        /// <summary>
        /// Adds a collection of elements to the collection.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="elements">The elements to add to the collection.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T>? elements)
        {
            Guard.IsNotNull(collection, nameof(collection));

            if (elements is null)
            {
                return;
            }

            foreach (var element in elements)
            {
                collection.Add(element);
            }
        }

        /// <summary>
        /// Adds a collection of elements to the collection where the element matches the specified predicate.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="elements">The elements to add to the collection.</param>
        /// <param name="predicate">The predicate that must be passed for the item to be added to the collection. If null, all elements will be added.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T>? elements, Predicate<T> predicate)
        {
            Guard.IsNotNull(collection, nameof(collection));
            Guard.IsNotNull(predicate, nameof(predicate));

            if (elements is null)
            {
                return;
            }

            foreach (var element in elements)
            {
                if (predicate(element))
                {
                    collection.Add(element);
                }
            }
        }

        /// <summary>
        /// Adds the specified elements to the collection as long as the collection does not already contain the elements.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="values">The values to add.</param>
        /// <returns>The number of elements added.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddRangeUnique<T>(this ICollection<T> collection, IEnumerable<T>? values)
        {
            return AddRangeUnique(collection, values?.ToArray());
        }

        /// <summary>
        /// Adds the specified elements to the collection as long as the collection does not already contain the elements.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="values">The values to add.</param>
        /// <returns>The number of elements added.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddRangeUnique<T>(this ICollection<T> collection, params T[]? values)
        {
            return AddRangeUnique(collection, values, (Func<T, T, bool>?)null);
        }

        /// <summary>
        /// Adds the specified elements to the collection as long as the collection does not already contain the elements.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="values">The values to add.</param>
        /// <param name="equalityComparer">The <see cref="IEqualityComparer{T}"/> to use for object comparison. If null, the default <see cref="IEqualityComparer{T}"/> will be used.</param>
        /// <returns><see langword="true" /> if the element was added, <see langword="false" /> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/> -or- <paramref name="equalityComparer"/> is <see langword="null" /></exception>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AddRangeUnique<T>(this ICollection<T> collection, IEnumerable<T>? values, IEqualityComparer<T>? equalityComparer)
        {
            Guard.IsNotNull(collection, nameof(collection));

            if (values is null)
            {
                return 0;
            }

            equalityComparer ??= EqualityComparer<T>.Default;

            int result = 0;

            foreach (var value in values)
            {
                bool exists = false;
                foreach (var element in collection)
                {
                    if (equalityComparer.Equals(element, value))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    collection.Add(value);
                    ++result;
                }
            }

            return result;
        }

        /// <summary>
        /// Adds the specified elements to the collection as long as the collection does not already contain the elements.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="values">The values to add.</param>
        /// <param name="equalityComparer">The equality comparer function to use for object comparison. If null, the default <see cref="IEqualityComparer{T}"/> is used.</param>
        /// <returns><see langword="true" /> if the element was added, <see langword="false" /> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static int AddRangeUnique<T>(this ICollection<T> collection, IEnumerable<T>? values, Func<T, T, bool>? equalityComparer)
        {
            Guard.IsNotNull(collection, nameof(collection));

            if (values is null)
            {
                return 0;
            }

            equalityComparer ??= EqualityComparer<T>.Default.Equals;

            int result = 0;

            foreach (var value in values)
            {
                bool exists = false;
                foreach (var element in collection)
                {
                    if (equalityComparer(element, value))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    collection.Add(value);
                    ++result;
                }
            }

            return result;
        }

        /// <summary>
        /// Adds an item to the collection, ensuring the result is sorted using the specified
        /// comparer. If the comparer is <see langword="null" />, a default comparer is used.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="item">The item to add to the collection.</param>
        /// <param name="comparer">The comparer to use.</param>
        /// <returns>The index at which the element was added.</returns>
        /// <remarks>
        /// This method will only generate a single event in the case of an <see cref="ObservableCollection{T}"/>.
        /// This method also assumes the collection is already sorted.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        public static int AddSorted<T>(this IList<T> collection, T? item, IComparer<T>? comparer = null)
        {
            Guard.IsNotNull(collection, nameof(collection));

            comparer ??= Comparer<T>.Default;
            int i = 0;
            while (i < collection.Count && comparer.Compare(collection[i], item!) < 0)
            {
                ++i;
            }

            collection.Insert(i, item!);

            return i;
        }

        /// <summary>
        /// Adds the specified element to the collection as long as the collection does not already contain the element.
        /// </summary>
        /// <typeparam name="T">The type of the element to add.</typeparam>
        /// <param name="collection">The collection to add to.</param>
        /// <param name="value">The value to add.</param>
        /// <param name="equalityComparer">The <see cref="IEqualityComparer{T}"/> to use for object comparison.</param>
        /// <returns><see langword="true" /> if the element was added, <see langword="false" /> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.</exception>
        public static bool AddUnique<T>(this ICollection<T> collection, T? value, IEqualityComparer<T>? equalityComparer = null)
        {
            Guard.IsNotNull(collection, nameof(collection));

            if (!collection.Contains(value!, equalityComparer ?? EqualityComparer<T>.Default))
            {
                collection.Add(value!);
                return true;
            }

            return false;
        }
    }
}
