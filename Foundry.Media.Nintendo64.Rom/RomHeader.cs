using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.HighPerformance;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// ROM metadata.
    /// </summary>
    public sealed class RomHeader
    {
        /// <summary>
        /// Represents unset, missing, or invalid metadata.
        /// </summary>
        public static RomHeader None { get; } = new RomHeader();

        /// <summary>
        /// The number of bytes that make up the ROM header.
        /// </summary>
        public const int Length = 0x40;

        /// <summary>
        /// The clock rate override.
        /// </summary>
        public uint ClockrateOverride { get; }

        /// <summary>
        /// The ROM entry address.
        /// </summary>
        /// <remarks>
        /// Also known as the program counter. This value sets the boot location (RAM entry point)
        /// when performing certain kinds of resets (0x8000030C), however some CIC chips will alter
        /// this location in the same way they affect the CRC calculation.
        /// </remarks>
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

        private RomHeader()
        {
            Title = string.Empty;
            GameCode = string.Empty;
        }

        private RomHeader(in ReadOnlySpan<byte> data, ERomFormat format)
        {
            Guard.HasSizeGreaterThanOrEqualTo(data, Length, nameof(data));

            Format = format;

            ClockrateOverride = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0x04, sizeof(uint))) & 0xFFF0;       // 0x04 - 0x07            
            EntryAddress = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0x08, sizeof(uint)));                     // 0x08 - 0x0B
            ReturnAddress = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0x0C, sizeof(uint)));                    // 0x0C - 0x0F
            Crc1 = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0x10, sizeof(uint)));                             // 0x10 - 0x13
            Crc2 = BinaryPrimitives.ReadUInt32BigEndian(data.Slice(0x17, sizeof(uint)));                             // 0x14 - 0x17
            // (Official N64 does not look beyond 0x18)
            Title = ReadString(data, 0x20, 20);                                                                      // 0x20 - 0x33
            GameCode = ReadString(data, 0x3B, 3);                                                                    // 0x3B - 0x3D
            DestinationCode = (ERomDestinationCode)data[0x3E];                                                       // 0x3E
            Version = data[0x3F];                                                                                    // 0x3F
        }

        private RomHeader(ERomFormat format, uint clockRateOverride, uint entryAddress, uint returnAddress, uint crc1, uint crc2, string title, string gameCode, ERomDestinationCode destCode, byte version)
        {
            Format = format;
            ClockrateOverride = clockRateOverride;
            EntryAddress = entryAddress;
            ReturnAddress = returnAddress;
            Crc1 = crc1;
            Crc2 = crc2;
            Title = title;
            GameCode = gameCode;
            DestinationCode = destCode;
            Version = version;
        }

        /// <summary>
        /// Returns this <see cref="RomHeader"/> instance with an adjusted <see cref="EntryAddress"/>.
        /// </summary>
        /// <param name="offset">The offset of the entry address compared to the original <see cref="EntryAddress"/>.</param>
        /// <remarks>
        /// Some ROMS have a different entry address than what is specified in the original <see cref="EntryAddress"/>.
        /// </remarks>
        public RomHeader WithEntryOffset(int offset)
        {
            return new RomHeader(Format, ClockrateOverride, (uint)(EntryAddress + offset), ReturnAddress, Crc1, Crc2, Title, GameCode, DestinationCode, Version);
        }

        /// <summary>
        /// Tries to set the CRC values in the header data pointed to by <paramref name="data"/>.
        /// </summary>
        /// <param name="data">The header data.</param>
        /// <param name="crc1">The first CRC value.</param>
        /// <param name="crc2">The second CRC value.</param>
        /// <returns><see langword="true"/> if the CRC values were updated successfully, <see langword="false"/> otherwise.</returns>
        public static bool TrySetCrc(in Span<byte> data, uint crc1, uint crc2)
        {
            if (data.Length < Length)
            {
                return false;
            }

            data.Slice(0x10, 4).Cast<byte, uint>()[0] = crc1;
            data.Slice(0x14, 4).Cast<byte, uint>()[0] = crc2;

            return true;
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

        internal static RomHeader Create(ReadOnlySpan<byte> data)
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
        public static bool TryCreate(ReadOnlySpan<byte> data, [NotNullWhen(true)] out RomHeader? result)
        {
            if (data.Length < Length)
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

            byte[]? bigEndianCopy = null;
            try
            {
                if (format != ERomFormat.BigEndian)
                {
                    // We want to work with big endian.
                    bigEndianCopy = ArrayPool<byte>.Shared.Rent(data.Length);
                    RomConverter.ConvertTo(ERomFormat.BigEndian, data, bigEndianCopy);
                }

                result = new RomHeader(bigEndianCopy is not null ? new ReadOnlySpan<byte>(bigEndianCopy) : data, format);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
            finally
            {
                if (bigEndianCopy is not null)
                {
                    ArrayPool<byte>.Shared.Return(bigEndianCopy);
                }
            }
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

        private static string ReadString(in ReadOnlySpan<byte> bytes, int address, int length)
        {
            Span<byte> trimChars = stackalloc byte[] { 0x0, 0x32 };
            Span<byte> buffer = stackalloc byte[length];

            bytes.Slice(address, length).CopyTo(buffer);
            buffer = buffer.Trim(trimChars);
            buffer.Replace(0x0, 0x20);

            return Encoding.ASCII.GetString(buffer);
        }

        public override string ToString()
        {
            return $"{Title} ({GameCode} {DestinationCode})";
        }
    }
}
