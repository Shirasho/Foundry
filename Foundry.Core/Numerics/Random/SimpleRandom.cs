using System;
using System.Runtime.InteropServices;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Numerics.Random
{
    /// <summary>
    /// A simple <see cref="System.Random"/> that uses <see cref="IRandomNumberProvider"/> as its generator.
    /// This class is not thread-safe.
    /// </summary>
    public sealed class SimpleRandom : IRandomNumberProvider
    {
        private readonly System.Random Random;

        public SimpleRandom()
        {
            Random = new System.Random();
        }

        /// <param name="seed"></param>
        public SimpleRandom(int seed)
        {
            Random = new System.Random(seed);
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">This function is not supported for this <see cref="IRandomNumberProvider"/> implementation.</exception>
        public ulong NextUInt64()
        {
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            NextBytes(buffer);

            return MemoryMarshal.Read<ulong>(buffer);
        }

        /// <inheritdoc />
        public int NextInt32()
        {
            return Random.Next();
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue" /> is less than 0. </exception>
        public int NextInt32(int maxValue)
        {
            // System.Random is exclusive upper.
            int maxActual = maxValue == int.MaxValue ? int.MaxValue : maxValue + 1;
            return Random.Next(maxActual);
        }

        /// <inheritdoc />
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public int NextInt32(int minValue, int maxValue)
        {
            var ordered = new OrderedElements<int>(minValue, maxValue);

            // System.Random is exclusive upper.
            int maxActual = ordered.Greater == int.MaxValue ? int.MaxValue : ordered.Greater + 1;

            return Random.Next(ordered.Lesser, maxActual);
        }

        /// <inheritdoc />
        public long NextInt64()
        {
            Span<byte> buffer = stackalloc byte[sizeof(long)];
            NextBytes(buffer);

            return MemoryMarshal.Read<long>(buffer);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public long NextInt64(long maxValue)
        {
            return (long)(NextDouble(maxValue) * long.MaxValue);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public long NextInt64(long minValue, long maxValue)
        {
            return (long)NextDouble(minValue, maxValue);
        }

        /// <inheritdoc />
        public double NextDouble()
        {
            return Random.NextDouble();
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public double NextDouble(double maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            return Random.NextDouble() * maxValue;
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public double NextDouble(double minValue, double maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(minValue, 0, nameof(minValue));
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            var ordered = new OrderedElements<double>(minValue, maxValue);
            return Random.NextDouble() * (ordered.Greater - ordered.Lesser) + ordered.Lesser;
        }

        /// <inheritdoc />
        public bool NextBool()
        {
            Span<byte> buffer = stackalloc byte[sizeof(bool)];
            NextBytes(buffer);

            // Marshaling as a bool doesn't work as one would hope here.
            // We need to check for even/odd here instead.
            return (buffer[0] & 1) != 1;
        }

        /// <inheritdoc />
        public byte[] NextBytes(long size)
        {
            lock (Random)
            {
                byte[] data = new byte[size];
                Random.NextBytes(data);
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
                Random.NextBytes(buffer);
            }
        }
    }
}
