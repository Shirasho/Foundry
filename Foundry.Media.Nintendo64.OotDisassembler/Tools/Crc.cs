using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDisassembler.Tools
{
    /// <summary>
    /// A utility for calculating and fixing OoT ROM CRC data.
    /// </summary>
    public static class Crc
    {
        private const int ChecksumStart = 0x00001000;
        private const int ChecksumLength = 0x00100000;
        private static readonly IReadOnlyList<uint> CrcTable = new uint[256];

        static Crc()
        {
            uint poly = 0xEDB88320;
            uint[] crcTable = new uint[256];
            for (int i = 0; i < CrcTable.Count; ++i)
            {
                uint crc = (uint)i;
                for (int j = 8; j > 0; --j)
                {
                    if ((crc & 1) != 0)
                    {
                        crc = (crc >> 1) ^ poly;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
                crcTable[i] = crc;
            }
            CrcTable = crcTable;
        }

        /// <summary>
        /// Returns whether the ROM data has the correct CRC values.
        /// </summary>
        /// <param name="data">The ROM data to check the CRC of.</param>
        /// <returns><see langword="true"/> if the data has the correct CRC values, <see langword="false"/> if it does not,
        /// or <see langword="null"/> if the CRC could not be calculated.</returns>
        public static bool? HasCorrectCrc(in ReadOnlySpan<byte> data)
            => HasCorrectCrc(data, out _);

        /// <summary>
        /// Returns whether the ROM data has the correct CRC values.
        /// </summary>
        /// <param name="data">The ROM data to check the CRC of.</param>
        /// <param name="crc">The calculated CRC values.</param>
        /// <returns><see langword="true"/> if the data has the correct CRC values, <see langword="false"/> if it does not,
        /// or <see langword="null"/> if the CRC could not be calculated.</returns>
        public static bool? HasCorrectCrc(in ReadOnlySpan<byte> data, out (uint Crc1, uint Crc2) crc)
        {
            //TODO: This is OOT specific.
            var buffer = data.Slice(ChecksumStart, ChecksumLength);
            int bootCode = GetCIC(buffer);
            uint seed = bootCode switch
            {
                6101 or 6102 => 0xF8CA4DDC,
                6103 => 0xA3886759,
                6105 => 0xDF26F436,
                6106 => 0x1FEA617A,
                _ => 1
            };

            if (seed == 1)
            {
                crc = (0, 0);
                return null;
            }

            uint t1 = seed;
            uint t2 = seed;
            uint t3 = seed;
            uint t4 = seed;
            uint t5 = seed;
            uint t6 = seed;

            int i = ChecksumStart;
            while (i < ChecksumStart + ChecksumLength)
            {
                uint d = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(i, 4));
                if (t6 + d < t6)
                {
                    ++t4;
                }
                t6 += d;
                t3 ^= d;
                uint r = ROL(d, (int)(d & 0x1F));
                t5 += r;
                if (t2 > d)
                {
                    t2 ^= r;
                }
                else
                {
                    t2 ^= t6 ^ d;
                }

                if (bootCode == 6105)
                {
                    t1 += BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(RomHeader.Length + 0x0710 + (i & 0xFF))) ^ d;
                }
                else
                {
                    t1 += t5 ^ d;
                }

                i += 4;
            }

            uint crc1;
            uint crc2;
            if (bootCode == 6103)
            {
                crc1 = (t6 ^ t4) + t3;
                crc2 = (t5 ^ t2) + t1;
            }
            else if (bootCode == 6106)
            {
                crc1 = (t6 * t4) + t3;
                crc2 = (t5 * t2) + t1;
            }
            else
            {
                crc1 = t6 ^ t4 ^ t3;
                crc2 = t5 ^ t2 ^ t1;
            }

            // The endianness of these need to be reversed.
            crc = (BinaryPrimitives.ReverseEndianness(crc1), BinaryPrimitives.ReverseEndianness(crc2));
            return RomHeader.TryCreate(data, out var header) &&
                   header.Crc1 == crc1 &&
                   header.Crc2 == crc2;
        }

        /// <summary>
        /// Fixes the two CRC values for a data region.
        /// </summary>
        /// <param name="data">The ROM data to check the CRC of. If the data has invalid CRC, it will be updated.</param>
        /// <returns><see langword="true"/> if the CRC required fixing, <see langword="false"/> otherwise.</returns>
        public static bool FixCrc(in Span<byte> data)
        {
            if (HasCorrectCrc(data, out var crc) != false)
            {
                return false;
            }

            return RomHeader.TrySetCrc(data, crc.Crc1, crc.Crc2);
        }

        internal static int GetCIC(in ReadOnlySpan<byte> data)
        {
            //TODO: Is this OOT specific?
            uint crc = GetCrc32(data.Slice(RomHeader.Length, 0x1000 - RomHeader.Length));
            return crc switch
            {
                0x6170A4A1 => 6101,
                0x90BB6CB5 => 6102,
                0x0B050EE0 => 6103,
                0x98BC2C86 => 6105,
                0xACC8580A => 6106,
                _ => 0
            };
        }

        private static uint GetCrc32(in ReadOnlySpan<byte> data)
        {
            uint crc = uint.MaxValue;
            for (int i = 0; i < data.Length; ++i)
            {
                crc = (crc >> 8) ^ CrcTable[(int)((crc ^ data[i]) & 0xFF)];
            }
            return ~crc;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ROL(uint i, int b)
        {
            return (i << b) | (i >> (32 - b));
        }
    }
}
