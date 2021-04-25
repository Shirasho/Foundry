using System;
using System.Buffers.Binary;
using System.Text;

namespace Foundry.Disassembly.Mips.Utilities
{
    /// <summary>
    /// Reads the contents of a <see cref="Memory{T}"/>.
    /// </summary>
    internal struct MemoryReader
    {
        private readonly ReadOnlyMemory<byte> Buffer;

        public MemoryReader(in ReadOnlyMemory<byte> buffer)
        {
            Buffer = buffer;
        }

        public string ReadString(uint address, int length)
            => ReadString(address, length, Encoding.ASCII);

        public string ReadString(uint address, int length, Encoding encoding)
        {
            Span<byte> trimChars = stackalloc byte[] { 0x0, 0x32 };
            Span<byte> buffer = stackalloc byte[length];

            Buffer.Slice((int)address, length).Span.CopyTo(buffer);
            buffer = buffer.Trim(trimChars);
            buffer.Replace(0x0, 0x20);

            return encoding.GetString(buffer);
        }

        public uint ReadUInt32(uint address)
        {
            return BinaryPrimitives.ReadUInt32BigEndian(Buffer.Slice((int)address, sizeof(uint)).Span);
        }
    }
}
