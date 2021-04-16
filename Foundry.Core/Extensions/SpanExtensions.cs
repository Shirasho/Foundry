using System;

namespace Foundry
{
    public static partial class MemoryExtensions
    {
        /// <summary>
        /// Replaces all instances of <paramref name="oldValue"/> in <paramref name="span"/>
        /// with <paramref name="newValue"/>.
        /// </summary>
        /// <param name="span">The span to search.</param>
        /// <param name="oldValue">The value to replace.</param>
        /// <param name="newValue">The new value.</param>
        public static void Replace(this in Span<byte> span, byte oldValue, byte newValue)
        {
            for (int i = 0; i < (uint)span.Length; ++i)
            {
                if (span[i] == oldValue)
                {
                    span[i] = newValue;
                }
            }
        }
    }
}
