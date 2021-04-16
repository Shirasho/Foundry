using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// ROM metadata.
    /// </summary>
    public sealed class RomMetadata
    {
        /// <summary>
        /// Represents unset, missing, or invalid metadata.
        /// </summary>
        public static RomMetadata None => new RomMetadata(ERomFormat.Unknown, ERomDestinationCode.Undocumented, string.Empty, string.Empty, 0);

        /// <summary>
        /// The last address of the header byte that we care about, rounded to the nearest 4 bytes.
        /// </summary>
        internal const int HeaderLength = 0x40;

        /// <summary>
        /// The format of the ROM data.
        /// </summary>
        public ERomFormat Format { get; }

        /// <summary>
        /// The destination code of the ROM.
        /// </summary>
        public ERomDestinationCode DestinationCode { get; }

        /// <summary>
        /// The title of the ROM.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// The game code for this ROM that is provided by Nintendo.
        /// </summary>
        public string GameCode { get; }

        /// <summary>
        /// The mask ROM version.
        /// </summary>
        public int MaskRomVersion { get; }

        private RomMetadata(ERomFormat format, ERomDestinationCode destinationCode, string title, string gameCode, int maskRomVersion)
        {
            Format = format;
            DestinationCode = destinationCode;
            Title = title;
            GameCode = gameCode;
            MaskRomVersion = maskRomVersion;
        }

        /// <summary>
        /// Returns the file extension of the format stored in <see cref="Format"/>.
        /// </summary>
        public string GetFormatExtension()
            => GetFormatExtension(Format);

        /// <summary>
        /// Returns the file extension of the format specified in <paramref name="format"/>.
        /// If the format is unknown, <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <param name="format">The format to get the extension of.</param>
        public static string GetFormatExtension(ERomFormat format)
        {
            return format switch
            {
                ERomFormat.BigEndian => ".z64",
                ERomFormat.ByteSwapped => ".v64",
                ERomFormat.LittleEndian => ".n64",
                _ => string.Empty
            };
        }

        internal static RomMetadata Create(ReadOnlySpan<byte> data)
        {
            if (!TryCreate(data, out var result))
            {
                throw new ArgumentException("Data is not a valid ROM.");
            }

            return result;
        }

        /// <summary>
        /// Tries to extract the ROM metadata from the provided data.
        /// </summary>
        /// <param name="data">The data containing the ROM metadata.</param>
        /// <param name="result">If this method returns <see langword="true"/>, contains the parsed ROM metadata.</param>
        /// <returns><see langword="true"/> if the metadata was extracted successfully, <see langword="false"/> otherwise.</returns>
        public static bool TryCreate(ReadOnlySpan<byte> data, [NotNullWhen(true)] out RomMetadata? result)
        {
            if (data.Length < HeaderLength)
            {
                result = default;
                return false;
            }

            var format = GetFormat(data);
            if (format == ERomFormat.Invalid)
            {
                result = default;
                return false;
            }

            // This can be an unnecessary "allocation" in certain cases,
            // but the size is small enough that we will do so in order
            // to reduce complexity.
            Span<byte> headerBuffer = stackalloc byte[HeaderLength];
            data.Slice(0, HeaderLength).CopyTo(headerBuffer);

            if (format != ERomFormat.BigEndian)
            {
                // We want to work with big endian.
                RomConverter.ConvertTo(ERomFormat.BigEndian, headerBuffer);
            }

            var destinationCode = (ERomDestinationCode)headerBuffer[0x3E];
            int maskRomVersion = headerBuffer[0x3F];

            Span<byte> trimChars = stackalloc byte[] { 0x0, 0x32 };
            string title = Encoding.ASCII.GetString(headerBuffer.Slice(0x20, 20).Trim(trimChars));
            string gameCode = Encoding.ASCII.GetString(data.Slice(0x3B, 3).Trim(trimChars));

            result = new RomMetadata(format, destinationCode, title, gameCode, maskRomVersion);
            return true;
        }

        /// <summary>
        /// Returns the format of the specified ROM data.
        /// </summary>
        /// <param name="data">The first four (or more) bytes of the ROM data.</param>
        public static ERomFormat GetFormat(ReadOnlySpan<byte> data)
        {
            if (data.Length < 4)
            {
                return ERomFormat.Invalid;
            }

            Span<byte> beMarker = stackalloc byte[] { 0x80, 0x37, 0x12, 0x40 };
            Span<byte> bsMarker = stackalloc byte[] { 0x37, 0x80, 0x40, 0x12 };
            Span<byte> leMarker = stackalloc byte[] { 0x40, 0x12, 0x37, 0x80 };

            if (data.StartsWith(beMarker))
            {
                return ERomFormat.BigEndian;
            }

            if (data.StartsWith(bsMarker))
            {
                return ERomFormat.ByteSwapped;
            }

            if (data.StartsWith(leMarker))
            {
                return ERomFormat.LittleEndian;
            }

            return ERomFormat.Invalid;
        }

        public override string ToString()
        {
            return $"{Title} ({GameCode} {DestinationCode})";
        }
    }
}
