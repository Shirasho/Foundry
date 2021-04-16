using System;
using System.Diagnostics.CodeAnalysis;
using Foundry.Media.Nintendo64.Rom.Utilities;
using Microsoft.Toolkit.Diagnostics;

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
        public static RomMetadata None { get; } = new RomMetadata();

        /// <summary>
        /// The last address of the header byte that we care about, rounded to the nearest 4 bytes.
        /// </summary>
        public const int HeaderLength = 0x40;

        /// <summary>
        /// The clock rate override.
        /// </summary>
        public uint ClockrateOverride { get; }

        /// <summary>
        /// The ROM entry address.
        /// </summary>
        public uint EntryAddress { get; }

        /// <summary>
        /// The return address.
        /// </summary>
        public uint ReturnAddress { get; }

        /// <summary>
        /// The first CRC.
        /// </summary>
        public uint Crc1 { get; }

        /// <summary>
        /// The second CRC.
        /// </summary>
        public uint Crc2 { get; }

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
        public byte Version { get; }

        private RomMetadata()
        {
            Title = string.Empty;
            GameCode = string.Empty;
        }

        private RomMetadata(in Span<byte> bigEndianHeaderBytes, ERomFormat format)
        {
            Guard.HasSizeGreaterThanOrEqualTo(bigEndianHeaderBytes, 40, nameof(bigEndianHeaderBytes));

            Format = format;

            var reader = new SpanReader(bigEndianHeaderBytes);
            reader.Seek(0x04);

            ClockrateOverride = reader.ReadUInt32();
            EntryAddress = reader.ReadUInt32();

            reader.Seek(0x0C);
            ReturnAddress = reader.ReadUInt32();

            reader.Seek(0x10);
            Crc1 = reader.ReadUInt32();
            Crc2 = reader.ReadUInt32();

            reader.Seek(0x20);
            Title = reader.ReadString(0x20, 20);

            reader.Seek(0x3B);
            GameCode = reader.ReadString(3);
            DestinationCode = (ERomDestinationCode)reader.ReadByte();
            Version = reader.ReadByte();
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

            result = new RomMetadata(headerBuffer, format);
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
