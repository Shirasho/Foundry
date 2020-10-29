using System;
using System.Diagnostics.CodeAnalysis;

namespace Foundry
{
    /// <summary>
    /// A helper class for throwing extensions.
    /// </summary>
    public static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentNullException(string argumentName)
        {
            throw new ArgumentNullException(argumentName);
        }

        [DoesNotReturn]
        public static void ThrowEmptyCollectionError()
        {
            throw new InvalidOperationException("The collection is empty.");
        }
    }
}
