using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// Helper methods for manipulating bytes.
    /// </summary>
    public static class ByteManipulation
    {
        /// <summary>
        /// Byte swaps the contents of <paramref name="src"/>.
        /// </summary>
        /// <param name="data">The data to swap the bytes of.</param>
        public static void SwapBytes(in Span<byte> data)
        {
            int shorts = data.Length / 2;
            for (int i = 0; i < shorts; ++i)
            {
                Swap(data, 2 * i, 2 * i + 1);
            }
        }

        /// <summary>
        /// Byte swaps the contents of <paramref name="src"/> and stores the
        /// result in <paramref name="dest"/>.
        /// </summary>
        /// <param name="src">The source bytes to swap.</param>
        /// <param name="dest">The destination to store the result of the operation.</param>
        public static void SwapBytes(in ReadOnlySpan<byte> src, in Span<byte> dest)
        {
            int shorts = src.Length / 2;
            for (int i = 0; i < shorts; ++i)
            {
                dest[2 * i] = src[2 * i + 1];
                dest[2 * i + 1] = src[2 * i];
            }
        }

        /// <summary>
        /// Swaps the endian of the contents of <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The data to swap the endian of.</param>
        /// <exception cref="ArgumentException">The length of <paramref name="data"/> is not divisible by 4.</exception>
        public static void SwapEndian(in Span<byte> data)
        {
            Guard.IsTrue(data.Length % 4 == 0, nameof(data), "Data length must be divisible by 4.");

            var span = MemoryMarshal.Cast<byte, uint>(data);
            for (int i = 0; i < span.Length; ++i)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        /// <summary>
        /// Swaps the endian of the contents of <paramref name="src"/> and stores
        /// the result in <paramref name="dest"/>.
        /// </summary>
        /// <param name="src">The source bytes to swap the endian of.</param>
        /// <param name="dest">The destination to store the result of the operation.</param>
        /// <exception cref="ArgumentException">
        ///     The length of <paramref name="src"/> is not divisible by 4 -or-
        ///     The length of <paramref name="dest"/> is less than the length of <paramref name="src"/>.
        /// </exception>
        public static void SwapEndian(in ReadOnlySpan<byte> src, in Span<byte> dest)
        {
            Guard.IsTrue(src.Length % 4 == 0, nameof(src), "Source length must be divisible by 4.");
            Guard.HasSizeGreaterThanOrEqualTo(dest, src.Length, nameof(dest));

            var srcSpan = MemoryMarshal.Cast<byte, uint>(src);
            var destSpan = MemoryMarshal.Cast<byte, uint>(dest);
            for (int i = 0; i < srcSpan.Length; ++i)
            {
                destSpan[i] = BinaryPrimitives.ReverseEndianness(srcSpan[i]);
            }
        }

        /// <summary>
        /// Swaps the data in two indices without creating a temporary
        /// variable.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="indexA">The first index to swap.</param>
        /// <param name="indexB">The second index to swap.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap(in Span<byte> data, int indexA, int indexB)
        {
            // We do not do any index checking since it provides no benefit over
            // the index checking the span already does.

            data[indexA] ^= data[indexB];
            data[indexB] ^= data[indexA];
            data[indexA] ^= data[indexB];
        }
    }
}
