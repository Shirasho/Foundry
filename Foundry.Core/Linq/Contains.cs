using System;
using System.Collections.Generic;

namespace Foundry.Linq
{
    public static partial class Linq
    {
        /// <summary>
        /// Returns whether <paramref name="collection"/> contains <paramref name="element"/>
        /// using the <paramref name="comparisonType"/> rules.
        /// </summary>
        /// <param name="collection">The collection reference.</param>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <param name="comparisonType">The search comparison rules.</param>
        public static bool Contains(this IEnumerable<string?>? collection, string? element, StringComparison comparisonType)
        {
            if (collection is null)
            {
                return false;
            }

            foreach (string? e in collection)
            {
                if (string.Equals(e, element, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
