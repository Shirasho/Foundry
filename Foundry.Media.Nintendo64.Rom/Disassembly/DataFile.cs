using System;
using System.Buffers.Binary;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// Represents a segment of data.
    /// </summary>
    public class DataFile
    {
        /// <summary>
        /// The name of the data segment.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The memory region of the data segment.
        /// </summary>
        public ReadOnlyMemory<byte> Data { get; }

        /// <summary>
        /// The length of the data segment.
        /// </summary>
        /// <remarks>
        /// This is equal to the length of <see cref="Data"/>.
        /// </remarks>
        public int Length { get; }

        /// <summary>
        /// The virtual address of the data segment.
        /// </summary>
        public uint VirtualAddress { get; }

        public DataFile(string name, in ReadOnlyMemory<byte> data, uint virtualAddress)
        {
            Guard.IsNotNullOrWhiteSpace(name, nameof(name));
            Guard.IsNotEmpty(data, nameof(data));

            Name = name;
            Data = data;
            Length = Data.Length;
            VirtualAddress = virtualAddress;
        }

        internal uint Read(uint address)
        {
            if (address * 4 + 4 > Length)
            {
                return 0;
            }

            return BinaryPrimitives.ReadUInt32BigEndian(Data.Span.Slice((int)(address * 4), 4));
        }
    }
}
