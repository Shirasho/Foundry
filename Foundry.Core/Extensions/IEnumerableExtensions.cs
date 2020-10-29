using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static partial class CollectionExtensions
    {
        /// <summary>
        /// Performs an operation on each element of the source enumerable in parallel.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="degreeOfParallelism">The number of elements to operate on at once.</param>
        /// <param name="action">The action to perform.</param>
        public static Task ForEachParallelAsync<T>(this IEnumerable<T> collection, int degreeOfParallelism, Func<T, CancellationToken, Task> action, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(collection, nameof(collection));
            Guard.IsNotNull(action, nameof(action));

            return Task.WhenAll(Partitioner
                .Create(collection)
                .GetPartitions(degreeOfParallelism)
                .AsParallel()
                .Select(new Func<IEnumerator<T>, Task>(AwaitPartition)));

            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext() && !cancellationToken.IsCancellationRequested)
                    {
                        await action(partition.Current, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}
