using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Foundry
{
    public static class IntPtrExtensions
    {
        /// <summary>
        /// Returns whether the contents being pointed to by the two <see cref="IntPtr"/>s
        /// contain the exact same bits. This method is time-safe and keeps the full contents
        /// of the pointers in unmanaged memory (only individual bytes are pulled into managed
        /// memory at any given time).
        /// </summary>
        /// <param name="element">The first element.</param>
        /// <param name="other">The second element.</param>
        /// <exception cref="AccessViolationException">Base address plus offset byte produces a null or invalid address.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SafeEquals(this IntPtr element, IntPtr other)
        {
            return SafeEquals(element, other, true);
        }

        /// <summary>
        /// Returns whether the contents being pointed to by the two <see cref="IntPtr"/>s
        /// contain the exact same bits. This method is time-safe.
        /// </summary>
        /// <param name="element">The first element.</param>
        /// <param name="other">The second element.</param>
        /// <param name="returnValueIfBothZero">What to return if both <paramref name="element"/> and <paramref name="other"/> are <see cref="IntPtr.Zero"/>.</param>
        /// <exception cref="AccessViolationException">Base address plus offset byte produces a null or invalid address.</exception>
        public static bool SafeEquals(this IntPtr element, IntPtr other, bool returnValueIfBothZero)
        {
            if (element == IntPtr.Zero && other == IntPtr.Zero)
            {
                return returnValueIfBothZero;
            }

            int length1 = Marshal.ReadInt32(element, -4);
            int length2 = Marshal.ReadInt32(other, -4);

            if (length1 != length2)
            {
                return false;
            }

            int areEqual = 0;
            for (int i = 0; i < length1; ++i)
            {
                byte b1 = Marshal.ReadByte(element, i);
                byte b2 = Marshal.ReadByte(other, i);

                areEqual |= b1 ^ b2;
            }

            return areEqual == 0;
        }
    }
}
