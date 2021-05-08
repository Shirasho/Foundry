using System;
using System.Buffers;
using System.Collections.Generic;
using Foundry.Media.Nintendo64.Rom.Utilities;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.HighPerformance;

namespace Foundry.Media.Nintendo64.OotDisassembler.Tools
{
    /// <summary>
    /// A utility for decompressing an OoT ROM.
    /// </summary>
    public sealed class Decompressor
    {
        private const int UINTSIZE = 0x01000000;
        private const int COMPSIZE = 0x02000000;
        private const int DCMPSIZE = 0x04000000;

        /// <summary>
        /// Decompresses a ROM.
        /// </summary>
        /// <param name="data">The data to decompress.</param>
        /// <param name="tableMarkerSequence">The uint marker bytes that indicate the start of the table.</param>
        public IMemoryOwner<byte> Decompress(in ReadOnlyMemory<byte> data, in ReadOnlyMemory<uint> tableMarkerSequence)
        {
            Guard.HasSizeEqualTo(data, COMPSIZE, nameof(data));

            if (data.IsEmpty)
            {
                return MemoryPool<byte>.Shared.Rent(0);
            }

            var result = MemoryPool<byte>.Shared.Rent(DCMPSIZE);
            try
            {
                int tableOffset = GetTableOffset(data, tableMarkerSequence);
                var tableInfo = GetTableEntry(data, tableOffset, 2);

                data.CopyTo(result.Memory);

                // Set everything after the table in the result to 0.
                var deadSpace = result.Memory.Span.Slice((int)tableInfo.EndVirtualAddress);
                for (int i = 0; i < deadSpace.Length; ++i)
                {
                    deadSpace[i] = 0;
                }

                var destTable = result.Memory.Slice(tableOffset).Cast<byte, uint>();

                foreach (var entry in GetTableEntries(data, tableOffset))
                {
                    if (entry.StartPhysicalAddress >= DCMPSIZE || entry.EndPhysicalAddress >= DCMPSIZE)
                    {
                        continue;
                    }

                    if (!entry.IsCompressed)
                    {
                        var source = data.Slice((int)entry.StartPhysicalAddress, entry.Size).Span;
                        var dest = result.Memory.Span.Slice((int)entry.StartVirtualAddress, entry.Size);
                        source.CopyTo(dest);
                    }
                    else
                    {
                        // We do not know how many bytes this will take up, so we cannot clamp the src and dest lengths.
                        var source = data.Slice((int)entry.StartPhysicalAddress).Span;
                        var dest = result.Memory.Span.Slice((int)entry.StartVirtualAddress);
                        Decompress(source, dest, entry.Size);
                    }

                    var replacementEntry = entry.SetDecompressed();
                    SetTableEntry(destTable, replacementEntry);
                }
            }
            catch
            {
                result.Dispose();
                throw;
            }

            Crc.FixCrc(result.Memory.Span);

            return result;
        }

        /// <summary>
        /// Returns the offset of the table.
        /// </summary>
        /// <param name="data">The ROM data.</param>
        /// <param name="tableMarkerSequence">The uint marker bytes that indicate the start of the table.</param>
        public int GetTableOffset(in ReadOnlyMemory<byte> data, in ReadOnlyMemory<uint> tableMarkerSequence)
        {
            Guard.IsNotEmpty(tableMarkerSequence, nameof(tableMarkerSequence));

            // Operations must keep in mind that this cast is on big-endian byte order.
            // Any number values must have their endian-ness reversed if necessary.
            // TableMarkerSequence contains literal uints parsed in the endian-ness of
            // the architecture, so there is no need to manipulate the byte order.
            var data32Bit = data.Cast<byte, uint>();
            if (data32Bit.Length < 0x1000 + tableMarkerSequence.Length)
            {
                return -1;
            }

            try
            {
                // Start at the end of the makerom
                for (int i = 0x1000; i + 4 < UINTSIZE * 4; i += 4)
                {
                    bool matches = true;
                    for (int s = 0; s < tableMarkerSequence.Length; ++s)
                    {
                        uint marker = data32Bit.Span[i + s];
                        matches &= marker == tableMarkerSequence.Span[s];
                    }

                    if (matches)
                    {
                        return i * 4;
                    }
                }
            }
            catch
            {
                // Suppress
            }

            return -1;
        }

        /// <summary>
        /// Returns the table entries for the ROM data.
        /// </summary>
        /// <param name="data">The ROM data.</param>
        /// <param name="tableMarkerSequence">The uint marker bytes that indicate the start of the table.</param>
        public IEnumerable<TableEntry> GetTableEntries(in ReadOnlyMemory<byte> data, in ReadOnlyMemory<uint> tableMarkerSequence)
        {
            Guard.IsNotEmpty(tableMarkerSequence, nameof(tableMarkerSequence));
            Guard.HasSizeGreaterThanOrEqualTo(data, 0x1000 + tableMarkerSequence.Length, nameof(data));

            int tableOffset = GetTableOffset(data, tableMarkerSequence);
            return GetTableEntries(data, tableOffset);
        }

        private IEnumerable<TableEntry> GetTableEntries(ReadOnlyMemory<byte> data, int tableOffset)
        {
            if (tableOffset < 0)
            {
                throw new InvalidOperationException("Unable to find ROM table.");
            }

            // Operations must keep in mind that this cast is on big-endian byte order.
            // Any number values must have their endian-ness reversed if necessary.
            var table = data.Slice(tableOffset).Cast<byte, uint>();
            var tableInfo = GetTableEntry(table, 2);
            int tableSize = tableInfo.Size;
            int entryCount = tableSize / 16;

            for (int i = 3; i < entryCount; ++i)
            {
                yield return GetTableEntry(table, i);
            }
        }

        private TableEntry GetTableEntry(in ReadOnlyMemory<byte> data, int tableOffset, int index)
        {
            return GetTableEntry(data.Slice(tableOffset).Cast<byte, uint>(), index);
        }

        private TableEntry GetTableEntry(in ReadOnlyMemory<uint> table, int index)
        {
            return new TableEntry(
                index,
                ByteManipulation.SwapEndianIfNecessary(table.Span[index * 4]),
                ByteManipulation.SwapEndianIfNecessary(table.Span[index * 4 + 1]),
                ByteManipulation.SwapEndianIfNecessary(table.Span[index * 4 + 2]),
                ByteManipulation.SwapEndianIfNecessary(table.Span[index * 4 + 3])
            );
        }

        private void SetTableEntry(in Memory<uint> table, TableEntry entry)
        {
            table.Span[entry.EntryIndex * 4] = ByteManipulation.SwapEndianIfNecessary(entry.StartVirtualAddress);
            table.Span[entry.EntryIndex * 4 + 1] = ByteManipulation.SwapEndianIfNecessary(entry.EndVirtualAddress);
            table.Span[entry.EntryIndex * 4 + 2] = ByteManipulation.SwapEndianIfNecessary(entry.StartPhysicalAddress);
            table.Span[entry.EntryIndex * 4 + 3] = ByteManipulation.SwapEndianIfNecessary(entry.EndPhysicalAddress);
        }

        private static void Decompress(in ReadOnlySpan<byte> src, in Span<byte> decomp, int decompSize)
        {
            int srcPlace = 0;
            int dstPlace = 0;
            uint bitCount = 0;
            byte codeByte = 0;

            var source = src.Slice(0x10);
            while (dstPlace < decompSize)
            {
                // If there are no more bits to test, get a new byte.
                if (bitCount == 0)
                {
                    codeByte = source[srcPlace++];
                    bitCount = 8;
                }

                // If bit 7 is a 1, just copy 1 byte from source to dest.
                if ((codeByte & 0x80) != 0)
                {
                    decomp[dstPlace++] = source[srcPlace++];
                }
                else
                {
                    // Get 2 bytes from source.
                    byte byte1 = source[srcPlace++];
                    byte byte2 = source[srcPlace++];

                    // Calculate distance to move in dest and number of bytes to copy.
                    int dist = ((byte1 & 0xF) << 8) | byte2;
                    int copyPlace = dstPlace - (dist + 1);
                    int numBytes = byte1 >> 4;

                    if (numBytes == 0)
                    {
                        // Do more calculations on the number of bytes to copy.
                        numBytes = source[srcPlace++] + 0x12;
                    }
                    else
                    {
                        numBytes += 2;
                    }

                    for (int i = 0; i < numBytes; ++i)
                    {
                        decomp[dstPlace++] = decomp[copyPlace++];
                    }
                }

                // Set up for next cycle.
                codeByte <<= 1;
                --bitCount;
            }
        }

        /// <summary>
        /// An entry from the ROM table.
        /// </summary>
        public class TableEntry
        {
            /// <summary>
            /// The offset of this entry within the table it belongs to.
            /// </summary>
            public int EntryIndex { get; }

            /// <summary>
            /// The virtual address of the referenced entry.
            /// </summary>
            public uint StartVirtualAddress { get; }

            /// <summary>
            /// The end virtual address of the referenced entry.
            /// </summary>
            public uint EndVirtualAddress { get; }

            /// <summary>
            /// The physical address of the referenced entry.
            /// </summary>
            public uint StartPhysicalAddress { get; }

            /// <summary>
            /// The end physical address of the referenced entry.
            /// </summary>
            public uint EndPhysicalAddress { get; }

            /// <summary>
            /// Whether this table entry is compressed and requires decompression.
            /// </summary>
            public bool IsCompressed => EndPhysicalAddress != 0x00000000;

            /// <summary>
            /// The size of this table entry.
            /// </summary>
            public int Size => (int)(EndVirtualAddress - StartVirtualAddress);

            internal TableEntry(int index, uint sv, uint ev, uint sp, uint ep)
            {
                EntryIndex = index;
                StartVirtualAddress = sv;
                EndVirtualAddress = ev;
                StartPhysicalAddress = sp;
                EndPhysicalAddress = ep;
            }

            /// <summary>
            /// Returns an entry that is the same as <paramref name="other"/>
            /// but has its contents marked as not compressed.
            /// </summary>
            internal TableEntry SetDecompressed()
            {
                return new TableEntry(EntryIndex, StartVirtualAddress, EndVirtualAddress, StartVirtualAddress, 0x00000000);
            }

            public override string ToString()
            {
                return $"StartVirtual={StartVirtualAddress} EndVirtual={EndVirtualAddress} StartPhysical={StartPhysicalAddress} EndPhysical={EndPhysicalAddress}";
            }
        }
    }
}
