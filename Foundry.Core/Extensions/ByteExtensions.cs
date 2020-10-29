using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static partial class CollectionExtensions
    {
        /// <summary>
        /// Attempts to extract the encoding of byte memory.
        /// </summary>
        /// <param name="bytes">The bytes to sample.</param>
        /// <param name="precision">The maximum number of bytes to sample. Higher values are slower but increase result accuracy.</param>
        /// <remarks> https://stackoverflow.com/a/12853721 </remarks>
        public static Encoding GetEncoding(this ReadOnlySpan<byte> bytes, int precision = 1000)
        {
            // If we have nothing to sample, return the user's default code page.
            if (bytes.Length == 0)
            {
                return Encoding.Default;
            }

            // Check for BOM
            if (bytes.Length >= 4 && bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0xFE && bytes[3] == 0xFF) { return Encoding.GetEncoding("utf-32BE"); }
            if (bytes.Length >= 4 && bytes[0] == 0xFF && bytes[1] == 0xFE && bytes[2] == 0x00 && bytes[3] == 0x00) { return Encoding.UTF32; }
            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF) { return Encoding.BigEndianUnicode; }
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE) { return Encoding.Unicode; }
            if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF) { return Encoding.UTF8; }
            if (bytes.Length >= 3 && bytes[0] == 0x2b && bytes[1] == 0x2f && bytes[2] == 0x76) { return Encoding.UTF7; }

            // No BOM - sample the bytes.
            if (precision <= 0 || precision > bytes.Length)
            {
                precision = bytes.Length;
            }

            // Some bytes are encoding in UTF8 with no BOM or signature.
            int i = 0;
            bool isUtf8 = false;
            while (i < precision - 4)
            {
                if (bytes[i] <= 0x7F) { i++; continue; }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
                if (bytes[i] >= 0xC2 && bytes[i] <= 0xDF && bytes[i + 1] >= 0x80 && bytes[i + 1] < 0xC0) { i += 2; isUtf8 = true; continue; }
                if (bytes[i] >= 0xE0 && bytes[i] <= 0xF0 && bytes[i + 1] >= 0x80 && bytes[i + 1] < 0xC0 && bytes[i + 2] >= 0x80 && bytes[i + 2] < 0xC0) { i += 3; isUtf8 = true; continue; }
                if (bytes[i] >= 0xF0 && bytes[i] <= 0xF4 && bytes[i + 1] >= 0x80 && bytes[i + 1] < 0xC0 && bytes[i + 2] >= 0x80 && bytes[i + 2] < 0xC0 && bytes[i + 3] >= 0x80 && bytes[i + 3] < 0xC0) { i += 4; isUtf8 = true; continue; }
                isUtf8 = false; break;
            }
            if (isUtf8)
            {
                return Encoding.UTF8;
            }

            // Heuristic attempt to detect UTF16 without a BOM. We look for 0s in odd or even byte places,
            // and if a certain threshold is reached, the code is probably UTF16.
            const double threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
            int count = 0;
            for (int n = 0; n < precision; n += 2)
            {
                if (bytes[n] == 0)
                {
                    ++count;
                }
            }

            if ((double)count / precision > threshold) { return Encoding.BigEndianUnicode; }
            count = 0;
            for (int n = 1; n < precision; n += 2)
            {
                if (bytes[n] == 0)
                {
                    ++count;
                }
            }

            if ((double)count / precision > threshold) { return Encoding.Unicode; }

            // As a last ditch effort, try to find "charset=xyz" or "encoding=xyz"
            for (int n = 0; n < precision - 9; ++n)
            {
                if (((bytes[n + 0] == 'c' || bytes[n + 0] == 'C') && (bytes[n + 1] == 'h' || bytes[n + 1] == 'H') && (bytes[n + 2] == 'a' || bytes[n + 2] == 'A') && (bytes[n + 3] == 'r' || bytes[n + 3] == 'R') && (bytes[n + 4] == 's' || bytes[n + 4] == 'S') && (bytes[n + 5] == 'e' || bytes[n + 5] == 'E') && (bytes[n + 6] == 't' || bytes[n + 6] == 'T') && bytes[n + 7] == '=') ||
                    ((bytes[n + 0] == 'e' || bytes[n + 0] == 'E') && (bytes[n + 1] == 'n' || bytes[n + 1] == 'N') && (bytes[n + 2] == 'c' || bytes[n + 2] == 'C') && (bytes[n + 3] == 'o' || bytes[n + 3] == 'O') && (bytes[n + 4] == 'd' || bytes[n + 4] == 'D') && (bytes[n + 5] == 'i' || bytes[n + 5] == 'I') && (bytes[n + 6] == 'n' || bytes[n + 6] == 'N') && (bytes[n + 7] == 'g' || bytes[n + 7] == 'G') && bytes[n + 8] == '='))
                {
                    if (bytes[n + 0] == 'c' || bytes[n + 0] == 'C')
                    {
                        n += 8;
                    }
                    else
                    {
                        n += 9;
                    }

                    if (bytes[n] == '"' || bytes[n] == '\'')
                    {
                        n++;
                    }

                    int oldN = n;
                    while (n < precision && (bytes[n] == '_' || bytes[n] == '-' || (bytes[n] >= '0' && bytes[n] <= '9') || (bytes[n] >= 'a' && bytes[n] <= 'z') || (bytes[n] >= 'A' && bytes[n] <= 'Z')))
                    { n++; }
                    Span<byte> nb = stackalloc byte[n - oldN];
                    bytes[oldN..n].CopyTo(nb);
                    try
                    {
                        string internalEnc = Encoding.ASCII.GetString(nb);
                        return Encoding.GetEncoding(internalEnc);
                    }
                    catch { break; }    // If C# doesn't recognize the name of the encoding, break.
                }
            }

            // If all else fails, try the user's code page.
            return Encoding.Default;
        }

        /// <summary>
        /// Converts this byte array into a <see cref="string"/>
        /// using the specified encoding. If <paramref name="encoding"/> is
        /// <see langword="null" />, an attempt will be made to detect the encoding
        /// of the bytes.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArrayTypeMismatchException"></exception>
        /// <exception cref="RankException"></exception>
        /// <exception cref="OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue"></see> elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("bytes")]
        public static string? GetString(this byte[]? bytes, Encoding? encoding = null)
        {
            if (bytes is null)
            {
                return null;
            }

            return bytes.Length > 0
                ? GetString(new ReadOnlySpan<byte>(bytes), encoding)
                : string.Empty;
        }

        /// <summary>
        /// Converts this byte array into a <see cref="string"/>
        /// using the specified encoding. If <paramref name="encoding"/> is
        /// <see langword="null" />, an attempt will be made to detect the encoding
        /// of the bytes.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <param name="encoding">The encoding to use.</param>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="ArrayTypeMismatchException"></exception>
        /// <exception cref="RankException"></exception>
        /// <exception cref="OverflowException">The array is multidimensional and contains more than <see cref="F:System.Int32.MaxValue"></see> elements.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetString(this ReadOnlySpan<byte> bytes, Encoding? encoding = null)
        {
            if (bytes.Length == 0)
            {
                return string.Empty;
            }

            return (encoding ?? GetEncoding(bytes)).GetString(bytes);
        }

        /// <summary>
        /// Increments the last element in the byte array. If the increment would cause the value to
        /// exceed <see cref="byte.MaxValue"/>, the element before it is incremented instead and the
        /// element is reset to 0.
        /// </summary>
        /// <param name="a">The <see cref="byte"/> array to increment.</param>
        /// <returns>The incremented array.</returns>
        /// <remarks>byte[] = { 3, 4, 255, 255} ==> { 3, 5, 0, 0 }</remarks>
        /// <exception cref="ArgumentNullException"><paramref name="a"/> is <see langword="null"/>.</exception>
        /// <exception cref="OverflowException">Every element in the array is equal to <see cref="byte.MaxValue"/>.</exception>
        public static byte[] Increment(this byte[] a)
        {
            Guard.IsNotNull(a, nameof(a));

            int incrementIndex = Array.FindLastIndex(a, b => b < byte.MaxValue);
            if (incrementIndex < 0)
            {
                throw new OverflowException($"Cannot increment byte array. Every element is equal to {byte.MaxValue}.");
            }

            return a.Take(incrementIndex)
                    .Concat(new[] { (byte)(a[incrementIndex] + 1) })
                    .Concat(new byte[a.Length - incrementIndex - 1])
                    .ToArray();
        }

        /// <summary>
        /// Returns whether <paramref name="a"/> is greater than or equal to <paramref name="b"/>,
        /// treating the entire <see cref="byte"/> array as a single "number".
        /// </summary>
        /// <param name="a">The first <see cref="byte"/> array.</param>
        /// <param name="b">The second <see cref="byte"/> array.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="a"/> is greater than or equal to <paramref name="b"/>,
        ///     <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="a"/> is <see langword="null"/> -or-
        ///     <paramref name="b"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException"></exception>
        public static bool IsGreaterThanOrEqualTo(this byte[] a, byte[] b)
            => IsGreaterThanOrEqualTo(a, b, true);

        /// <summary>
        /// Returns whether <paramref name="a"/> is greater than <paramref name="b"/>,
        /// treating the entire <see cref="byte"/> array as a single "number".
        /// </summary>
        /// <param name="a">The first <see cref="byte"/> array.</param>
        /// <param name="b">The second <see cref="byte"/> array.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="a"/> is greater than <paramref name="b"/>,
        ///     <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="a"/> is <see langword="null"/> -or-
        ///     <paramref name="b"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException"></exception>
        public static bool IsGreaterThan(this byte[] a, byte[] b)
            => IsGreaterThanOrEqualTo(a, b, false);

        /// <summary>
        /// Returns whether <paramref name="a"/> is less than or equal to <paramref name="b"/>,
        /// treating the entire <see cref="byte"/> array as a single "number".
        /// </summary>
        /// <param name="a">The first <see cref="byte"/> array.</param>
        /// <param name="b">The second <see cref="byte"/> array.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="a"/> is less than or equal to <paramref name="b"/>,
        ///     <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="a"/> is <see langword="null"/> -or-
        ///     <paramref name="b"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException"></exception>
        public static bool IsLessThanOrEqualTo(this byte[] a, byte[] b)
            => IsLessThanOrEqualTo(a, b, true);

        /// <summary>
        /// Returns whether <paramref name="a"/> is less than <paramref name="b"/>,
        /// treating the entire <see cref="byte"/> array as a single "number".
        /// </summary>
        /// <param name="a">The first <see cref="byte"/> array.</param>
        /// <param name="b">The second <see cref="byte"/> array.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="a"/> is less than <paramref name="b"/>,
        ///     <see langword="false"/> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="a"/> is <see langword="null"/> -or-
        ///     <paramref name="b"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException"></exception>
        public static bool IsLessThan(this byte[] a, byte[] b)
            => IsLessThanOrEqualTo(a, b, false);

        /// <exception cref="ArgumentNullException">
        ///     <paramref name="a"/> is <see langword="null"/> -or-
        ///     <paramref name="b"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException"></exception>
        private static bool IsLessThanOrEqualTo(this byte[] a, byte[] b, bool defaultResult)
        {
            Guard.IsNotNull(a, nameof(a));
            Guard.IsNotNull(b, nameof(b));

            if (a.Length != b.Length)
            {
                byte[] longer = a.Length > b.Length ? a : b;
                int diff = Math.Abs(a.Length - b.Length);

                if (longer == b)
                {
                    while (diff > 0)
                    {
                        if (longer[diff - 1] != 0)
                        {
                            return true;
                        }
                        --diff;
                    }
                }
                else
                {
                    return false;
                }
            }

            int l = Math.Min(a.Length, b.Length);
            for (int i = 0; i < l; ++i)
            {
                if (a[i] != b[i])
                {
                    return a[i] < b[i];
                }
            }

            return defaultResult;
        }

        /// <exception cref="ArgumentNullException">
        ///     <paramref name="a"/> is <see langword="null"/> -or-
        ///     <paramref name="b"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="OverflowException"></exception>
        private static bool IsGreaterThanOrEqualTo(this byte[] a, byte[] b, bool defaultResult)
        {
            Guard.IsNotNull(a, nameof(a));
            Guard.IsNotNull(b, nameof(b));

            if (a.Length != b.Length)
            {
                byte[] longer = a.Length > b.Length ? a : b;
                int diff = Math.Abs(a.Length - b.Length);

                if (longer == a)
                {
                    while (diff > 0)
                    {
                        if (longer[diff - 1] != 0)
                        {
                            return true;
                        }
                        --diff;
                    }
                }
                else
                {
                    return false;
                }
            }

            int l = Math.Min(a.Length, b.Length);
            for (int i = 0; i < l; ++i)
            {
                if (a[i] != b[i])
                {
                    return a[i] > b[i];
                }
            }

            return defaultResult;
        }
    }
}
