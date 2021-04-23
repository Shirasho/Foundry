using System;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    /// <summary>
    /// Represents a segment of data.
    /// </summary>
    public class DataSegment
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

        public DataSegment(string name, in ReadOnlyMemory<byte> data, uint virtualAddress)
        {
            Guard.IsNotNullOrWhitespace(name, nameof(name));
            Guard.IsNotEmpty(data, nameof(data));

            Name = name;
            Data = data;
            Length = Data.Length;
            VirtualAddress = virtualAddress;
        }
    }
}
