using System.Runtime.CompilerServices;

namespace Foundry
{
    public static partial class NumericExtensions
    {
        /// <summary>
        /// Converts this <see cref="bool"/> value to an <see cref="int"/>
        /// without branching instructions or casts.
        /// </summary>
        /// <param name="b">The value to convert.</param>
        /// <returns>1 if the value is <see langword="true"/>, 0 otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ToInt(this bool b)
        {
            return Unsafe.As<bool, byte>(ref b);
        }
    }
}
