using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Foundry.Serialization.Json
{
    /// <summary>
    /// <see cref="Utf8JsonReader"/> extensions.
    /// </summary>
    public static class Utf8JsonReaderExtensions
    {
        /// <summary>
        /// In the worst case, an ASCII character represented as a single utf-8 byte could expand 6x when escaped.
        /// For example: '+' becomes '\u0043'
        /// Escaping surrogate pairs (represented by 3 or 4 utf-8 bytes) would expand to 12 bytes (which is still <= 6x).
        /// The same factor applies to utf-16 characters.
        /// </summary>
        public const int MaxExpansionFactorWhileEscaping = 6;

        /// <summary>
        /// default (i.e. 'D'), 8 + 4 + 4 + 4 + 12 + 4 for the hyphens (e.g. 094ffa0a-0442-494d-b452-04003fa755cc)
        /// </summary>
        public const int MaximumFormatGuidLength = 36;

        public const int MaximumEscapedGuidLength = MaxExpansionFactorWhileEscaping * MaximumFormatGuidLength;

        /// <summary>
        /// Parses the current JSON token value from the source as a <see cref="Guid"/>.
        /// Returns the value if the entire UTF-8 encoded token value can be successfully parsed to a <see cref="Guid"/>
        /// value.
        /// Throws exceptions otherwise.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if trying to get the value of a JSON token that is not a <see cref="JsonTokenType.String"/>.
        /// </exception>
        /// <exception cref="FormatException">
        /// Thrown if the JSON token value is of an unsupported format for a Guid.
        /// </exception>
        /// <remarks>
        /// This differs from <see cref="Utf8JsonReader.GetGuid"/> in that the format is
        /// not expected to be of type 'D'.
        /// </remarks>
        public static Guid GetGuidFlexible(this ref Utf8JsonReader reader)
        {
            if (!TryGetGuidFlexible(ref reader, out var value))
            {
                ThrowUnsupportedGuidFormatException();
            }

            return value;
        }

        /// <summary>
        /// Parses the current JSON token value from the source as a <see cref="Guid"/>.
        /// Returns <see langword="true"/> if the entire UTF-8 encoded token value can be successfully
        /// parsed to a <see cref="Guid"/> value. Only supports <see cref="Guid"/> values with hyphens
        /// and without any surrounding decorations.
        /// Returns <see langword="false"/> otherwise.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if trying to get the value of a JSON token that is not a <see cref="JsonTokenType.String"/>.
        /// </exception>
        /// <remarks>
        /// This differs from <see cref="Utf8JsonReader.TryGetGuid(out Guid)"/> in that the format is
        /// not expected to be of type 'D'.
        /// </remarks>
        public static bool TryGetGuidFlexible(this ref Utf8JsonReader reader, out Guid value)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new InvalidOperationException("The JSON token is expected to be of type 'string'.");
            }

            ReadOnlySpan<byte> span = stackalloc byte[0];
            if (reader.HasValueSequence)
            {
                long sequenceLength = reader.ValueSequence.Length;
                if (sequenceLength > MaximumEscapedGuidLength)
                {
                    value = default;
                    return false;
                }

                Span<byte> stackSpan = stackalloc byte[(int)sequenceLength];

                reader.ValueSequence.CopyTo(stackSpan);
                span = stackSpan;
            }
            else
            {
                if (reader.ValueSpan.Length > MaximumEscapedGuidLength)
                {
                    value = default;
                    return false;
                }

                span = reader.ValueSpan;
            }

            //            if (reader.CurrentState._stringHasEscaping)
            //            {
            //                int idx = span.IndexOf((byte)'\\');

            //                Span<byte> utf8Unescaped = stackalloc byte[span.Length];
            //                JsonReaderHelper.Unescape(span, utf8Unescaped, idx, out int written);

            //#pragma warning disable PC001 // API not supported on all platforms
            //                utf8Unescaped = utf8Unescaped.Slice(0, written);
            //#pragma warning restore PC001 // API not supported on all platforms

            //                span = utf8Unescaped;
            //            }

            if (span.Length <= MaximumFormatGuidLength &&
                TryParseGuid(span, out var tmp))
            {
                value = tmp;
                return true;
            }

            value = default;
            return false;
        }

        private static bool TryParseGuid(ReadOnlySpan<byte> source, out Guid value)
        {
            return Utf8Parser.TryParse(source, out value, out _, 'D') ||
                   Utf8Parser.TryParse(source, out value, out _, 'B') ||
                   Utf8Parser.TryParse(source, out value, out _, 'P') ||
                   Utf8Parser.TryParse(source, out value, out _, 'N');
        }

        [DoesNotReturn]
        private static void ThrowUnsupportedGuidFormatException()
        {
            throw new FormatException("The JSON value is not in a supported Guid format.");
        }
    }
}
