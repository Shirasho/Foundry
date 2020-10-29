using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Linq
{
    public static partial class Linq
    {
        /// <summary>
        /// Obtains a random element from the collection.
        /// </summary>
        /// <typeparam name="T">The type of object this collection contains.</typeparam>
        /// <param name="collection">The collection reference.</param>
        /// <returns>A random element from the collection.</returns>
        /// <exception cref="OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue" /> elements.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        /// <exception cref="InvalidOperationException">The collection is empty.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        [return: MaybeNull]
        public static T Random<T>(this IEnumerable<T> collection)
        {
            Guard.IsNotNull(collection, nameof(collection));

            if (collection is IList<T> list)
            {
                if (list.Count == 0)
                {
                    ThrowHelper.ThrowEmptyCollectionError();
                }
                return list[LinqRandom.NextInt32(0, list.Count - 1)];
            }

            var c = collection.ToArray();
            if (c.Length == 0)
            {
                ThrowHelper.ThrowEmptyCollectionError();
            }

            return c[LinqRandom.NextInt32(0, c.Length - 1)];
        }
    }
}
