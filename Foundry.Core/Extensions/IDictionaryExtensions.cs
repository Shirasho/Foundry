using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static partial class CollectionExtensions
    {
        /// <summary>
        /// Adds a value to the dictionary. If the key already exists, the key's value is
        /// updated instead.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/></exception>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, [DisallowNull] TKey key, [AllowNull] TValue value)
        {
            Guard.IsNotNull(dictionary, nameof(dictionary));
            if (key is null)
            {
                // No Guard - missing class constraint
                ThrowHelper.ThrowArgumentNullException(nameof(key));
            }

            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, value!);
            }
            else
            {
                dictionary[key] = value!;
            }
        }

        /// <summary>
        /// Returns this <see cref="IDictionary{TKey, TValue}"/> wrapped in a <see cref="IReadOnlyDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="dictionary">The dictionary to wrap.</param>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is <see langword="null"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>([NotNull] this IDictionary<TKey, TValue> dictionary)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Returns a collection of <see cref="KeyValuePair{TKey, TValue}"/> in a <see cref="IReadOnlyDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="elements">The values to wrap.</param>
        /// <exception cref="ArgumentNullException"><paramref name="elements"/> is <see langword="null"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> elements)
        {
            return new ReadOnlyDictionary<TKey, TValue>(new Dictionary<TKey, TValue>(elements));
        }

        /// <summary>
        /// Returns the first element value in <paramref name="dictionary"/> if it is not empty,
        /// otherwise the default value for <typeparamref name="TValue"/> is returned.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="dictionary">The dictionary.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: MaybeNull]
        public static TValue FirstOrDefaultValue<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary)
        {
            return dictionary?.Count > 0
                ? dictionary.First().Value
                : default;
        }

        /// <summary>
        /// Returns a collection of <see cref="KeyValuePair{TKey, TValue}"/> in a <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <typeparam name="TKey">The key type.</typeparam>
        /// <typeparam name="TValue">The value type.</typeparam>
        /// <param name="elements">The values to wrap.</param>
        /// <exception cref="ArgumentNullException"><paramref name="elements"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> elements)
        {
            return new Dictionary<TKey, TValue>(elements);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the
        /// key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.</param>
        /// <param name="keyComparer"></param>
        /// <returns>
        /// <see langword="true"/> if the object that implements <see cref="IDictionary{TKey, TValue}"/> contains
        /// an element with the specified key; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method will first try to call <see cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
        /// for an O(1) operation. If that call fails, this method becomes O(n).
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> or <paramref name="key"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, [DisallowNull] TKey key, [MaybeNull] out TValue value, IEqualityComparer<TKey>? keyComparer)
        {
            Guard.IsNotNull(dictionary, nameof(dictionary));

            // Return O(1) if possible.
            if (dictionary.TryGetValue(key, out value))
            {
                return true;
            }

            // Slower implementation separated out so we can inline above fast path.
            return TryGetValueCore(dictionary, key, out value, keyComparer);
        }

        private static bool TryGetValueCore<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, [DisallowNull] TKey key, [MaybeNull] out TValue value, IEqualityComparer<TKey>? keyComparer)
        {
            keyComparer ??= EqualityComparer<TKey>.Default;

            foreach (var element in dictionary)
            {
                if (keyComparer.Equals(key, element.Key))
                {
                    value = element.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value associated with the specified key. <paramref name="dictionary"/> may be <see langword="null"/>.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key">The key whose value to get.</param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified key, if the
        /// key is found; otherwise, the default value for the type of the value parameter.
        /// This parameter is passed uninitialized.</param>
        /// <returns>
        /// <see langword="true"/> if the object that implements <see cref="IDictionary{TKey, TValue}"/> contains
        /// an element with the specified key; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method will first try to call <see cref="IDictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
        /// for an O(1) operation. If that call fails, this method becomes O(n).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetValueSafe<TKey, TValue>(this IDictionary<TKey, TValue>? dictionary, [DisallowNull] TKey key, [MaybeNull] out TValue value)
        {
            if (dictionary is null)
            {
                value = default;
                return false;
            }

            return dictionary.TryGetValue(key, out value);
        }
    }
}
