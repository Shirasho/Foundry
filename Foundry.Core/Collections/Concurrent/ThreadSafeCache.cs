using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Collections.Concurrent
{
    /// <summary>
    /// Represents a thread-safe cache.
    /// </summary>
    /// <typeparam name="TKey">The key type of the cache.</typeparam>
    /// <typeparam name="TValue">The value type of the cache.</typeparam>
    public sealed class ThreadSafeCache<TKey, TValue>
        where TKey : notnull
    {
        private readonly ConcurrentDictionary<TKey, TValue> Cache;
        private readonly Func<TKey, TValue> ValueFactory;

        public ThreadSafeCache(Func<TKey, TValue> valueFactory)
        {
            Guard.IsNotNull(valueFactory, nameof(valueFactory));

            Cache = new ConcurrentDictionary<TKey, TValue>();
            ValueFactory = valueFactory;
        }

        public ThreadSafeCache(Func<TKey, TValue> valueFactory, IEqualityComparer<TKey>? keyComparer)
        {
            Guard.IsNotNull(valueFactory, nameof(valueFactory));

            Cache = new ConcurrentDictionary<TKey, TValue>(keyComparer);
            ValueFactory = valueFactory;
        }

        /// <summary>
        /// Gets the value from the cache under the key defined in <paramref name="key"/>.
        /// If the key does not exist in the cache, it is created in a thread-safe manner
        /// using the value factory specified in the constructor of this class.
        /// </summary>
        /// <param name="key">The key to get the value of.</param>
        [return: MaybeNull]
        public TValue Get([DisallowNull] TKey key)
        {
            return Cache.GetOrAdd(key, ValueFactory);
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key from the <see cref="ThreadSafeCache{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">
        /// When this method returns, contains the object from the <see cref="ThreadSafeCache{TKey, TValue}"/>
        /// that has the specified key, or the default value of the type if the operation
        /// failed.</param>
        public bool TryGetValue([DisallowNull] TKey key, [MaybeNull] out TValue value)
        {
            return Cache.TryGetValue(key, out value);
        }
    }
}
