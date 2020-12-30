using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Linq
{
    public static partial class Linq
    {
        /// <summary>
        /// Batches all elements in a sequence into separate arrays of up to the specified size.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="count">The maximum size of each batch.</param>
        /// <param name="selector">Transforms the source into a result object.</param>
        /// <returns>A list containing each batch.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Count is less than or equal to 0.</exception>
        public static IEnumerable<TResult?> Batch<TSource, TResult>(this IEnumerable<TSource?> collection, int count, Func<IEnumerable<TSource?>, TResult?> selector)
            => Batch(collection, count, selector, true);

        /// <summary>
        /// Batches all elements in a sequence into separate arrays of up to the specified size.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="count">The maximum size of each batch.</param>
        /// <param name="selector">Transforms the source into a result object.</param>
        /// <param name="returnRemainder">Whether to return the remainder of the elements that don't fully fill a batch.</param>
        /// <returns>A list containing each batch.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Count is less than or equal to 0.</exception>
        /// <exception cref="T:System.Exception">A delegate callback throws an exception.</exception>
        public static IEnumerable<TResult?> Batch<TSource, TResult>(this IEnumerable<TSource?> collection, int count, Func<IEnumerable<TSource?>, TResult?> selector, bool returnRemainder)
        {
            Guard.IsNotNull(collection, nameof(collection));
            Guard.IsNotNull(selector, nameof(selector));
            Guard.IsGreaterThan(count, 0, nameof(count));

            return _(); IEnumerable<TResult?> _()
            {
                TSource?[]? bucket = null;
                int bucketCount = 0;

                foreach (var item in collection)
                {
                    if (bucket is null)
                    {
                        bucket = new TSource?[count];
                    }

                    bucket[bucketCount++] = item;

                    // The bucket is fully buffered before it's yielded
                    if (bucketCount != count)
                    {
                        continue;
                    }

                    yield return selector(bucket);

                    bucket = null;
                    bucketCount = 0;
                }

                // Return the last bucket with all remaining elements
                if (returnRemainder && bucket is not null && count > 0)
                {
                    Array.Resize(ref bucket, bucketCount);
                    yield return selector(bucket);
                }
            }
        }

        /// <summary>
        /// Batches all elements in a sequence into separate arrays of up to the specified size.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="count">The maximum size of each batch.</param>
        /// <returns>A list containing each batch.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Count is less than or equal to 0.</exception>
        public static IEnumerable<IEnumerable<T?>> Batch<T>(this IEnumerable<T?> collection, int count)
            => Batch(collection, count, true);

        /// <summary>
        /// Batches all elements in a sequence into separate arrays of up to the specified size.
        /// </summary>
        /// <typeparam name="T">The type of the element.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="count">The maximum size of each batch.</param>
        /// <param name="returnRemainder">Whether to return the remainder of the elements that don't fully fill a batch.</param>
        /// <returns>A list containing each batch.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Count is less than or equal to 0.</exception>
        public static IEnumerable<IEnumerable<T?>> Batch<T>(this IEnumerable<T?> collection, int count, bool returnRemainder)
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
            => Batch(collection, count, FuncProvider<IEnumerable<T?>>.ReturnSelf, returnRemainder);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
    }
}
