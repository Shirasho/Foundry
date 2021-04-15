using System;
using System.Text;

namespace Foundry.Media.Nintendo64.RomData
{
    public readonly struct RomMetadata
    {
        /// <summary>
        /// The last address of the header byte that we care about.
        /// </summary>
        internal const int HeaderLength = 0x3F;

        /// <summary>
        /// The format of the ROM data.
        /// </summary>
        public readonly ERomFormat Format { get; }

        /// <summary>
        /// The destination code of the ROM.
        /// </summary>
        public readonly ERomDestinationCode DestinationCode { get; }

        /// <summary>
        /// The title of the ROM.
        /// </summary>
        public readonly string Title { get; }

        /// <summary>
        /// The game code for this ROM that is provided by Nintendo.
        /// </summary>
        public string GameCode { get; }

        /// <summary>
        /// The mask ROM version.
        /// </summary>
        public readonly int MaskRomVersion { get; }

        private RomMetadata(ERomFormat format, ERomDestinationCode destinationCode, string title, string gameCode, int maskRomVersion)
        {
            Format = format;
            DestinationCode = destinationCode;
            Title = title;
            GameCode = gameCode;
            MaskRomVersion = maskRomVersion;
        }

        internal string GetFormatExtension()
        {
            return Format switch
            {
                ERomFormat.BigEndian => ".z64",
                ERomFormat.ByteSwapped => ".v64",
                ERomFormat.LittleEndian => ".z64",
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
        public static bool TryCreate(ReadOnlySpan<byte> data, out RomMetadata result)
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

            var destinationCode = (ERomDestinationCode)data[0x3E];
            int maskRomVersion = data[0x3F];

            Span<byte> trimChars = stackalloc byte[] { 0, 32 };

            var romTitleSpan = data.Slice(0x20, 20);
            Span<byte> romTitleSpanFixed = stackalloc byte[romTitleSpan.Length];
            RomConverter.ConvertTo(ERomFormat.BigEndian, romTitleSpan, romTitleSpanFixed);
            string title = Encoding.ASCII.GetString(romTitleSpanFixed);

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

            Span<byte> beMarker = stackalloc byte[] { 80, 37, 12, 40 };
            Span<byte> bsMarker = stackalloc byte[] { 37, 80, 40, 12 };
            Span<byte> leMarker = stackalloc byte[] { 40, 12, 37, 80 };

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

        public readonly override string ToString()
        {
            return $"{Title} ({GameCode} {DestinationCode})";
        }
    }
}
