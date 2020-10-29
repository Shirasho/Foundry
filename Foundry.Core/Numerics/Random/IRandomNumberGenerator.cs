using System;

namespace Foundry.Numerics.Random
{
    /// <summary>
    /// An interface for a random number generator.
    /// </summary>
    public interface IRandomNumberProvider
    {
        /// <summary>
        /// Obtains a random <see cref="ulong"/> between 0 inclusive and <see cref="ulong.MaxValue"/> inclusive.
        /// </summary>
        /// <returns>A random <see cref="ulong"/>.</returns>
        ulong NextUInt64();

        /// <summary>
        /// Obtains a random <see cref="int"/> between 0 inclusive and <see cref="int.MaxValue"/> inclusive.
        /// </summary>
        /// <returns>A random <see cref="int"/>.</returns>
        int NextInt32();

        /// <summary>
        /// Obtains a random <see cref="int"/> between 0 inclusive and <paramref name="maxValue"/> inclusive.
        /// </summary>
        /// <param name="maxValue">The maximum value of the random number.</param>
        /// <returns>A random integer.</returns>
        int NextInt32(int maxValue);

        /// <summary>
        /// Obtains a random <see cref="int"/> between <paramref name="minValue"/> inclusive and <paramref name="maxValue"/> inclusive.
        /// </summary>
        /// <param name="minValue">The minimum value of the random number.</param>
        /// <param name="maxValue">The maximum value of the random number.</param>
        /// <returns>A random integer.</returns>
        int NextInt32(int minValue, int maxValue);

        /// <summary>
        /// Obtains a random <see cref="long"/> between 0 inclusive and <see cref="long.MaxValue"/> inclusive.
        /// </summary>
        /// <returns>A random <see cref="long"/>.</returns>
        long NextInt64();

        /// <summary>
        /// Obtains a random <see cref="long"/> between 0 inclusive and <paramref name="maxValue"/> inclusive.
        /// </summary>
        /// <param name="maxValue">The maximum value of the random number.</param>
        /// <returns>A random <see cref="long"/>.</returns>
        long NextInt64(long maxValue);

        /// <summary>
        /// Obtains a random <see cref="long"/> between <paramref name="minValue"/> inclusive and <paramref name="maxValue"/> inclusive.
        /// </summary>
        /// <param name="minValue">The minimum value of the random number.</param>
        /// <param name="maxValue">The maximum value of the random number.</param>
        /// <returns>A random <see cref="long"/>.</returns>
        long NextInt64(long minValue, long maxValue);

        /// <summary>
        /// Obtains a random <see cref="double"/> between 0 inclusive and 1 exclusive.
        /// </summary>
        /// <returns>A random <see cref="double"/>.</returns>
        double NextDouble();

        /// <summary>
        /// Obtains a random <see cref="double"/> between 0 inclusive and <paramref name="maxValue"/> exclusive.
        /// </summary>
        /// <returns>A random <see cref="double"/>.</returns>
        double NextDouble(double maxValue);

        /// <summary>
        /// Obtains a random <see cref="double"/> between <paramref name="minValue"/> inclusive and <paramref name="maxValue"/> inclusive.
        /// </summary>
        /// <param name="minValue">The minimum value of the random number.</param>
        /// <param name="maxValue">The maximum value of the random number.</param>
        /// <returns>A random <see cref="double"/>.</returns>
        double NextDouble(double minValue, double maxValue);

        /// <summary>
        /// Obtains a random <see cref="bool"/>.
        /// </summary>
        /// <returns>A random <see cref="bool"/>.</returns>
        bool NextBool();

        /// <summary>
        /// Returns the next <paramref name="size"/> number of bytes.
        /// </summary>
        /// <param name="size">The number of bytes to return.</param>
        /// <returns>The random bytes.</returns>
        byte[] NextBytes(long size);

        /// <summary>
        /// Fills <paramref name="buffer"/> with random bytes.
        /// </summary>
        /// <param name="buffer">The buffer to fill.</param>
        void NextBytes(Span<byte> buffer);
    }
}
