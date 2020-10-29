using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Numerics.Random
{
    /// <summary>
    /// A random number generator that uses <see cref="RNGCryptoServiceProvider"/> as the
    /// underlying generator. This random number generator is cryptically secure. This class
    /// is thread-safe.
    /// </summary>
    /// <remarks>
    /// This class is slow. Do not use this in performance-critical applications.
    /// In performance-critical applications, use <see cref="MersenneTwisterRandom"/> instead.
    /// </remarks>
    public sealed class CryptoRandom : IRandomNumberProvider
    {
        private static RNGCryptoServiceProvider Random { get; } = new RNGCryptoServiceProvider();

        /// <inheritdoc />
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public unsafe ulong NextUInt64()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            NextBytes(buffer);

            return MemoryMarshal.Read<ulong>(buffer);
        }

        /// <inheritdoc />
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public unsafe int NextInt32()
        {
            Span<byte> buffer = stackalloc byte[sizeof(int)];
            NextBytes(buffer);

            return MemoryMarshal.Read<int>(buffer);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public int NextInt32(int maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            int result;
            do
            {
                result = NextInt32();
            } while (result > maxValue);

            return result;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException"><paramref name="minValue"/> is less than 0.</exception>
        /// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public int NextInt32(int minValue, int maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(minValue, 0, nameof(minValue));
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            var ordered = new OrderedElements<int>(minValue, maxValue);

            int result;
            do
            {
                result = NextInt32();
            } while (result < ordered.Lesser || result > ordered.Greater);

            return result;
        }

        /// <inheritdoc />
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public long NextInt64()
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            NextBytes(buffer);

            return MemoryMarshal.Read<long>(buffer);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public long NextInt64(long maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            long result;
            do
            {
                result = NextInt64();
            } while (result > maxValue);

            return result;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException"><paramref name="minValue"/> is less than 0.</exception>
        /// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public long NextInt64(long minValue, long maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(minValue, 0, nameof(minValue));
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            var ordered = new OrderedElements<long>(minValue, maxValue);

            long result;
            do
            {
                result = NextInt64();
            } while (result < ordered.Lesser || result > ordered.Greater);

            return result;
        }

        /// <inheritdoc />
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public double NextDouble()
        {
            Span<byte> buffer = stackalloc byte[sizeof(double)];
            NextBytes(buffer);

            return MemoryMarshal.Read<double>(buffer);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public double NextDouble(double maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            double result;
            do
            {
                result = NextDouble();
            } while (result > maxValue);

            return result;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentException"><paramref name="minValue"/> is less than 0.</exception>
        /// <exception cref="ArgumentException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public double NextDouble(double minValue, double maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(minValue, 0, nameof(minValue));
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            var ordered = new OrderedElements<double>(minValue, maxValue);

            double result;
            do
            {
                result = NextDouble();
            } while (result < ordered.Lesser || result > ordered.Greater);

            return result;
        }

        /// <inheritdoc />
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public bool NextBool()
        {
            Span<byte> buffer = stackalloc byte[sizeof(bool)];
            NextBytes(buffer);

            // Marshaling as a bool doesn't work as one would hope here.
            // We need to check for even/odd here instead.
            return (buffer[0] & 1) != 1;
        }

        /// <inheritdoc />
        /// <exception cref="CryptographicException">The cryptographic service provider (CSP) cannot be acquired. </exception>
        public byte[] NextBytes(long size)
        {
            lock (Random)
            {
                byte[] data = new byte[size];
                Random.GetBytes(data);
                return data;
            }
        }

        /// <summary>
        /// Fills <paramref name="buffer" /> with random bytes.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        public void NextBytes(Span<byte> buffer)
        {
            lock (Random)
            {
#if NETSTANDARD2_0
                for (int i = 0; i < buffer.Length; ++i)
                {
                    buffer[i] = (byte)NextInt32();
                }
#else
                Random.GetBytes(buffer);
#endif
            }
        }
    }
}
