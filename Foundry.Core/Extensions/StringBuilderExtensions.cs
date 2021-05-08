using System;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Appends a message to the string builder with the specified indent depth.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/>.</param>
        /// <param name="indent">The indent depth.</param>
        /// <param name="value">The value to append.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sb"/> is <see langword="null"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static StringBuilder AppendLine(this StringBuilder sb, int indent, string value)
            => AppendLine(sb, 0, indent, value);

        /// <summary>
        /// Appends a message to the string builder with the specified indent depth.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/>.</param>
        /// <param name="baseIndent">The base indent for every line.</param>
        /// <param name="indent">The indent depth.</param>
        /// <param name="value">The value to append.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sb"/> is <see langword="null"/></exception>
        public static StringBuilder AppendLine(this StringBuilder sb, int baseIndent, int indent, string value)
        {
            Guard.IsNotNull(sb, nameof(sb));
            Guard.IsNotNull(value, nameof(value));
            Guard.IsGreaterThanOrEqualTo(baseIndent, 0, nameof(baseIndent));
            Guard.IsGreaterThanOrEqualTo(indent, 0, nameof(indent));

            return sb.Append(new string(' ', baseIndent + indent * 2)).Append(value).AppendLine();
        }

        /// <summary>
        /// Returns whether the string builder ends with the specified characters.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/>.</param>
        /// <param name="value">The value to check for.</param>
        public static bool EndsWith(this StringBuilder sb, ReadOnlySpan<char> value)
        {
            if (sb.Length < value.Length)
            {
                return false;
            }

            string end = sb.ToString(sb.Length - value.Length, value.Length);
            return value.SequenceEqual(end);
        }

        /// <summary>
        /// Removes the first character(s) from the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/>.</param>
        /// <param name="count">The number of characters to remove from the front.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sb"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0</exception>
        public static void RemoveFirst(this StringBuilder sb, int count = 1)
        {
            Guard.IsNotNull(sb, nameof(sb));
            Guard.IsGreaterThanOrEqualTo(count, 0, nameof(count));

            while (count > 0)
            {
                sb.Remove(0, 1);
                --count;
            }
        }

        /// <summary>
        /// Removes the last character(s) from the specified <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="sb">The <see cref="StringBuilder"/>.</param>
        /// <param name="count">The number of characters to remove from the end.</param>
        /// <exception cref="ArgumentNullException"><paramref name="sb"/> is <see langword="null"/></exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than 0</exception>
        public static void RemoveLast(this StringBuilder sb, int count = 1)
        {
            Guard.IsNotNull(sb, nameof(sb));
            Guard.IsGreaterThanOrEqualTo(count, 0, nameof(count));

            while (count > 0)
            {
                sb.Remove(sb.Length - 1, 1);
                --count;
            }
        }
    }
}
