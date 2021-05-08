using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Linq
{
    public static partial class Linq
    {
        public static IEnumerable<IGrouping<TNewKey, TValue>> TransformKey<TOldKey, TValue, TNewKey>(this IEnumerable<IGrouping<TOldKey, TValue>> grouping, Func<TOldKey, TNewKey> reducer)
        {
            Guard.IsNotNull(grouping, nameof(grouping));
            Guard.IsNotNull(reducer, nameof(reducer));

            return _(); IEnumerable<IGrouping<TNewKey, TValue>> _()
            {
                foreach (var group in grouping)
                {
                    yield return new Grouping<TNewKey, TValue>(reducer(group.Key), group);
                }
            }
        }

        private class Grouping<TKey, TValue> : IGrouping<TKey, TValue>
        {
            public TKey Key { get; }
            private readonly IEnumerable<TValue> Value;

            public Grouping(TKey key, IEnumerable<TValue> value)
            {
                Key = key;
                Value = value;
            }

            public IEnumerator<TValue> GetEnumerator() => Value.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
