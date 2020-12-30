using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Linq
{
    public static partial class Linq
    {
        /// <summary>
        /// Orders the collection using the specified <see cref="IComparer{T}"/>.
        /// </summary>
        /// <param name="collection">The collection reference.</param>
        /// <param name="comparer">The <see cref="IComparer{T}"/> to use. If left null, the default <see cref="IComparer{T}"/> is used.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/></exception>
        public static IEnumerable<T?> OrderBy<T>(this IEnumerable<T?> collection, IComparer<T?>? comparer)
        {
            Guard.IsNotNull(collection, nameof(collection));

            return collection.OrderBy(FuncProvider<T?>.ReturnSelf, new ComparerBridge<T?>(comparer));
        }
    }
}
