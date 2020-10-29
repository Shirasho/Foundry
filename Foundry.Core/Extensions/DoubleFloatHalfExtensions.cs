using System;

namespace Foundry
{
    public static partial class NumericExtensions
    {
#if !NETSTANDARD2_1
        /// <summary>
        /// Returns whether this <see cref="Half"/> is considered equal to the provided value.
        /// </summary>
        /// <param name="h">The first <see cref="Half"/>.</param>
        /// <param name="other">The second <see cref="Half"/>.</param>
        /// <param name="epsilon">How far apart the <see cref="Half"/> values can be and still be considered equal. This accounts for floating point precision errors.</param>
        public static bool Equals(this Half h, Half other, Half epsilon = Half.Epsilon * 10)
        {
            return Math.Abs(h - other) < epsilon;
        }
#endif

        /// <summary>
        /// Returns whether this <see cref="float"/> is considered equal to the provided value.
        /// </summary>
        /// <param name="f">The first <see cref="float"/>.</param>
        /// <param name="other">The second <see cref="float"/>.</param>
        /// <param name="epsilon">How far apart the <see cref="float"/> values can be and still be considered equal. This accounts for floating point precision errors.</param>
        public static bool Equals(this float f, float other, float epsilon = float.Epsilon * 10)
        {
            return Math.Abs(f - other) < epsilon;
        }

        /// <summary>
        /// Returns whether this <see cref="double"/> is considered equal to the provided value.
        /// </summary>
        /// <param name="d">The first <see cref="double"/>.</param>
        /// <param name="other">The second <see cref="double"/>.</param>
        /// <param name="epsilon">How far apart the <see cref="double"/> values can be and still be considered equal. This accounts for floating point precision errors.</param>
        public static bool Equals(this double d, double other, double epsilon = double.Epsilon * 10)
        {
            return Math.Abs(d - other) < epsilon;
        }
    }
}
