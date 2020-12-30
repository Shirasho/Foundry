using System;

namespace Foundry
{
    public static partial class NumericExtensions
    {
        /// <summary>
        /// Writes as many big-endian bytes as possible to the specified buffer.
        /// </summary>
        /// <param name="value">The value to get the bytes of.</param>
        /// <param name="buffer">The buffer to write the bytes to.</param>
        public static void GetBytes(this int value, Span<byte> buffer)
            => GetBytes(value, buffer, out _);

        /// <summary>
        /// Writes as many big-endian bytes as possible to the specified buffer.
        /// </summary>
        /// <param name="value">The value to get the bytes of.</param>
        /// <param name="buffer">The buffer to write the bytes to.</param>
        /// <param name="numBytes">The number of bytes to extract from <paramref name="value"/>.</param>
        public static void GetBytes(this int value, Span<byte> buffer, int numBytes)
            => GetBytes(value, buffer, numBytes, out _);

        /// <summary>
        /// Writes as many big-endian bytes as possible to the specified buffer.
        /// </summary>
        /// <param name="value">The value to get the bytes of.</param>
        /// <param name="buffer">The buffer to write the bytes to.</param>
        /// <param name="bytesWritten">The number of bytes written to the buffer.</param>
        public static void GetBytes(this int value, Span<byte> buffer, out int bytesWritten)
            => GetBytes(value, buffer, sizeof(int), out bytesWritten);

        /// <summary>
        /// Writes as many big-endian bytes as possible to the specified buffer.
        /// </summary>
        /// <param name="value">The value to get the bytes of.</param>
        /// <param name="buffer">The buffer to write the bytes to.</param>
        /// <param name="numBytes">The number of bytes to extract from <paramref name="value"/>.</param>
        /// <param name="bytesWritten">The number of bytes written to the buffer.</param>
        public static void GetBytes(this int value, Span<byte> buffer, int numBytes, out int bytesWritten)
        {
            int maxBytes = Math.Max(0, numBytes.Min(sizeof(int), buffer.Length));
            for (int i = maxBytes - 1; i >= 0; --i)
            {
                buffer[i] = (byte)(value & 0xFF);
                value >>= 8;
            }

            bytesWritten = maxBytes;
        }
    }
}
