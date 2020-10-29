/* 
   Copyright (C) 1997 - 2002, Makoto Matsumoto and Takuji Nishimura,
   All rights reserved.                          

   Redistribution and use in source and binary forms, with or without
   modification, are permitted provided that the following conditions
   are met:

     1. Redistributions of source code must retain the above copyright
        notice, this list of conditions and the following disclaimer.

     2. Redistributions in binary form must reproduce the above copyright
        notice, this list of conditions and the following disclaimer in the
        documentation and/or other materials provided with the distribution.

     3. The names of its contributors may not be used to endorse or promote 
        products derived from this software without specific prior written 
        permission.

   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
   A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE COPYRIGHT OWNER OR
   CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
   EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
   PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
   PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
   LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
   NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
   SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

   The original C# porting is done by stlalv on October 8, 2010. 
   e-mail:stlalv @ nifty.com (remove space)
   Original C code is found at http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/MT2002/emt19937ar.html as mt19937ar.tgz

   This file was updated by Ethan Treff on October 27, 2020.
*/

using System;
using System.Diagnostics;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Numerics.Random
{
    /// <summary>
    /// A random number generator which generates random numbers using the Mersenne Twister algorithm.
    /// This class may not be inherited by any classes outside of this assembly.
    /// </summary>
    public sealed class MersenneTwisterRandom : IRandomNumberProvider
    {
        /// <summary>
        /// Static system RNG for default seeding only.
        /// </summary>
        private static readonly System.Random Random = new System.Random();

        private const int N = 624;
        private const int M = 397;

        /// <summary>
        /// Constant vector A.
        /// </summary>
        private const uint MATRIX_A = 0x9908b0dfu;

        /// <summary>
        /// Most significant w-r bytes.
        /// </summary>
        private const uint UPPER_MASK = 0x80000000u;

        /// <summary>
        /// Least significant r bytes.
        /// </summary>
        private const uint LOWER_MASK = 0x7fffffffu;

        /// <summary>
        /// The array for the state vector.
        /// </summary>
        private readonly ulong[] _TwisterBytes = new ulong[N];

        /// <summary>
        /// <see cref="_TwisterBytesIndex"/> == <see cref="N"/> + 1 indicates that
        /// <see cref="_TwisterBytes"/>[<see cref="N"/>] has not been initialized.
        /// </summary>
        private int _TwisterBytesIndex = N + 1;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MersenneTwisterRandom()
            : this((ulong)(Random.NextDouble() * ulong.MaxValue))
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public MersenneTwisterRandom(ulong seed)
        {
            SeedTwister(seed);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="seed">The seed.</param>
        public MersenneTwisterRandom(ulong[] seed)
        {
            Guard.IsNotNull(seed, nameof(seed));

            SeedTwister(seed);
        }

        private void SeedTwister(ulong seed)
        {
            lock (Random)
            {
                _TwisterBytes[0] = seed & 0xffffffffUL;
                for (_TwisterBytesIndex = 1; _TwisterBytesIndex < N; ++_TwisterBytesIndex)
                {
                    _TwisterBytes[_TwisterBytesIndex] = 1812433253UL * (_TwisterBytes[_TwisterBytesIndex - 1] ^ (_TwisterBytes[_TwisterBytesIndex - 1] >> 30)) + (ulong)_TwisterBytesIndex;
                    _TwisterBytes[_TwisterBytesIndex] &= 0xffffffffUL;
                }
            }
        }

        private void SeedTwister(ulong[] seed)
        {
            lock (Random)
            {
                SeedTwister(19650218UL);
                int i = 1;
                int j = 0;
                int k = N > seed.Length ? N : seed.Length;
                for (; k != 0; k--)
                {
                    _TwisterBytes[i] = (_TwisterBytes[i] ^ ((_TwisterBytes[i - 1] ^ (_TwisterBytes[i - 1] >> 30)) * 1664525UL)) + seed[j] + (ulong)j; /* non linear */
                    _TwisterBytes[i] &= 0xffffffffUL; /* for WORDSIZE > 32 machines */
                    i++;
                    j++;
                    if (i >= N)
                    {
                        _TwisterBytes[0] = _TwisterBytes[N - 1];
                        i = 1;
                    }

                    if (j >= seed.Length)
                    {
                        j = 0;
                    }
                }

                for (k = N - 1; k != 0; k--)
                {
                    _TwisterBytes[i] = (_TwisterBytes[i] ^ ((_TwisterBytes[i - 1] ^ (_TwisterBytes[i - 1] >> 30)) * 1566083941UL)) - (ulong)i; // non linear
                    _TwisterBytes[i] &= 0xffffffffUL; // for WORDSIZE > 32 machines
                    i++;
                    if (i >= N)
                    {
                        _TwisterBytes[0] = _TwisterBytes[N - 1];
                        i = 1;
                    }
                }

                _TwisterBytes[0] = 0x80000000UL; // MSB is 1; assuring non-zero initial array 
            }
        }

        /// <summary>
        /// Obtains a random uint.
        /// </summary>
        /// <returns>A random uint.</returns>
        public uint NextUInt32()
        {
            lock (Random)
            {
                ulong[] mag01 = { 0x0UL, MATRIX_A };
                ulong y;
                // mag01[x] = x * MATRIX_A  for x=0,1
                if (_TwisterBytesIndex >= N)
                {	// generate N words at one time
                    int kk;
                    if (_TwisterBytesIndex == N + 1)
                    {			// if init_genrand() has not been called,
                        SeedTwister((ulong)(Random.NextDouble() * ulong.MaxValue));	// a default initial seed is used
                    }
                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (_TwisterBytes[kk] & UPPER_MASK) | (_TwisterBytes[kk + 1] & LOWER_MASK);
                        _TwisterBytes[kk] = _TwisterBytes[kk + M] ^ (y >> 1) ^ mag01[y & 0x1UL];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (_TwisterBytes[kk] & UPPER_MASK) | (_TwisterBytes[kk + 1] & LOWER_MASK);
                        _TwisterBytes[kk] = _TwisterBytes[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1UL];
                    }
                    y = (_TwisterBytes[N - 1] & UPPER_MASK) | (_TwisterBytes[0] & LOWER_MASK);
                    _TwisterBytes[N - 1] = _TwisterBytes[M - 1] ^ (y >> 1) ^ mag01[y & 0x1UL];
                    _TwisterBytesIndex = 0;
                }
                y = _TwisterBytes[_TwisterBytesIndex++];
                // Tempering
                y ^= y >> 11;
                y ^= (y << 7) & 0x9d2c5680UL;
                y ^= (y << 15) & 0xefc60000UL;
                y ^= y >> 18;
                return (uint)y;
            }
        }

        /// <inheritdoc />
        public ulong NextUInt64()
        {
            lock (Random)
            {
                ulong[] mag01 = { 0x0UL, MATRIX_A };
                ulong y;
                // mag01[x] = x * MATRIX_A  for x=0,1
                if (_TwisterBytesIndex >= N)
                {	// generate N words at one time
                    int kk;
                    if (_TwisterBytesIndex == N + 1)
                    {			// if init_genrand() has not been called,
                        SeedTwister((ulong)(Random.NextDouble() * ulong.MaxValue));	// a default initial seed is used
                    }
                    for (kk = 0; kk < N - M; kk++)
                    {
                        y = (_TwisterBytes[kk] & UPPER_MASK) | (_TwisterBytes[kk + 1] & LOWER_MASK);
                        _TwisterBytes[kk] = _TwisterBytes[kk + M] ^ (y >> 1) ^ mag01[y & 0x1UL];
                    }
                    for (; kk < N - 1; kk++)
                    {
                        y = (_TwisterBytes[kk] & UPPER_MASK) | (_TwisterBytes[kk + 1] & LOWER_MASK);
                        _TwisterBytes[kk] = _TwisterBytes[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1UL];
                    }
                    y = (_TwisterBytes[N - 1] & UPPER_MASK) | (_TwisterBytes[0] & LOWER_MASK);
                    _TwisterBytes[N - 1] = _TwisterBytes[M - 1] ^ (y >> 1) ^ mag01[y & 0x1UL];
                    _TwisterBytesIndex = 0;
                }
                y = _TwisterBytes[_TwisterBytesIndex++];
                // Tempering
                y ^= y >> 11;
                y ^= (y << 7) & 0x9d2c5680UL;
                y ^= (y << 15) & 0xefc60000UL;
                y ^= y >> 18;
                return y;
            }
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public int NextInt32()
        {
            return NextInt32(int.MaxValue);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public int NextInt32(int maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            return (int)(NextUInt32() * ((double)maxValue / uint.MaxValue));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public int NextInt32(int minValue, int maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(minValue, 0, nameof(minValue));
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            var ordered = new OrderedElements<int>(minValue, maxValue);

            return NextInt32(ordered.Greater - ordered.Lesser) + ordered.Lesser;
        }

        /// <inheritdoc />
        [DebuggerStepThrough]
        public long NextInt64()
        {
            return NextInt64(long.MaxValue);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public long NextInt64(long maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            return (long)(NextUInt64() * ((ulong)maxValue / ulong.MaxValue));
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minValue"/> is less than 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public long NextInt64(long minValue, long maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(minValue, 0, nameof(minValue));
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            var ordered = new OrderedElements<long>(minValue, maxValue);

            return NextInt64(ordered.Greater - ordered.Lesser) + ordered.Lesser;
        }

        /// <summary>
        /// Obtains a random <see cref="double" /> between 0 inclusive and 1 exclusive.
        /// </summary>
        /// <returns>A random <see cref="double" />.</returns>
        public double NextDouble()
        {
            // Returns [0,1]. To return [0,1) add 1 to the denominator.
            return NextUInt32() * (1d / uint.MaxValue);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than 0.</exception>
        public double NextDouble(double maxValue)
        {
            Guard.IsGreaterThanOrEqualTo(maxValue, 0, nameof(maxValue));

            return NextDouble() * maxValue;
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

            return NextDouble(ordered.Greater - ordered.Lesser) + ordered.Lesser;
        }

        /// <summary>
        /// Obtains a random <see cref="bool" />.
        /// </summary>
        /// <returns>A random <see cref="bool" />.</returns>
        public bool NextBool()
        {
            return NextInt32(0, 1) == 1;
        }

        /// <summary>
        /// Returns the next <paramref name="size" /> number of bytes.
        /// </summary>
        /// <param name="size">The number of bytes to return.</param>
        /// <returns>The random bytes.</returns>
        public byte[] NextBytes(long size)
        {
            lock (Random)
            {
                byte[] data = new byte[size];
                for (long i = 0; i < data.LongLength; ++i)
                {
                    data[i] = (byte)NextUInt32();
                }
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
                for (int i = 0; i < buffer.Length; ++i)
                {
                    buffer[i] = (byte)NextUInt32();
                }
            }
        }
    }
}
