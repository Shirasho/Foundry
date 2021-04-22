using System;
using System.Text;

namespace Foundry.Media.Nintendo64.Rom.Utilities
{
    /// <summary>
    /// Reads the contents of a <see cref="Memory{T}"/>.
    /// </summary>
    public struct MemoryReader
    {
        private readonly ReadOnlyMemory<byte> Buffer;
        private int Cursor;

        public MemoryReader(in ReadOnlyMemory<byte> buffer)
        {
            Buffer = buffer;
            Cursor = 0;
        }

        public void Seek(int offset)
        {
            Cursor = offset;
        }

        public string ReadString(int length)
        {
            string result = ReadString(Cursor, length);
            Cursor += length;
            return result;
        }

        public string ReadString(int offset, int length)
        {
            Span<byte> trimChars = stackalloc byte[] { 0x0, 0x32 };
            Span<byte> buffer = stackalloc byte[length];

            Buffer.Slice(offset, length).Span.CopyTo(buffer);
            buffer = buffer.Trim(trimChars);
            buffer.Replace(0x0, 0x20);

            return Encoding.ASCII.GetString(buffer);
        }

        public string ReadString(int length, Encoding encoding)
        {
            string result = ReadString(Cursor, length, encoding);
            Cursor += length;
            return result;
        }

        public string ReadString(int offset, int length, Encoding encoding)
        {
            Span<byte> trimChars = stackalloc byte[] { 0x0, 0x32 };
            Span<byte> buffer = stackalloc byte[length];

            Buffer.Slice(offset, length).Span.CopyTo(buffer);
            buffer = buffer.Trim(trimChars);
            buffer.Replace(0x0, 0x20);

            return encoding.GetString(buffer);
        }

        public uint ReadUInt32()
        {
            uint result = ReadUInt32(Cursor);
            Cursor += sizeof(int);
            return result;
        }

        public uint ReadUInt32(int offset)
        {
            return BitConverter.ToUInt32(Buffer.Slice(offset, sizeof(uint)).Span);
        }

        public bool TryReadUInt32(int offset, out uint result)
        {
            if (offset + 4 > Buffer.Length)
            {
                result = 0;
                return false;
            }

            result = ReadUInt32(offset);
            return true;
        }

        public ulong ReadUInt64()
        {
            ulong result = ReadUInt64(Cursor);
            Cursor += sizeof(long);
            return result;
        }

        public ulong ReadUInt64(int offset)
        {
            return BitConverter.ToUInt64(Buffer.Slice(offset, sizeof(ulong)).Span);
        }

        public int ReadInt32()
        {
            int result = ReadInt32(Cursor);
            Cursor += sizeof(int);
            return result;
        }

        public int ReadInt32(int offset)
        {
            return BitConverter.ToInt32(Buffer.Slice(offset, sizeof(int)).Span);
        }

        public byte ReadByte()
        {
            byte result = ReadByte(Cursor);
            Cursor += sizeof(byte);
            return result;
        }

        public byte ReadByte(int offset)
        {
            return Buffer.Span[offset];
        }
    }
}
