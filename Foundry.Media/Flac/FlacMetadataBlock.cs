using System;
using System.IO;

namespace Foundry.Media.Flac
{
    public abstract class FlacMetadataBlock : IFlacMetadataBlock
    {
        /// <summary>
        /// The block length.
        /// </summary>
        public abstract int BlockLength { get; }

        /// <summary>
        /// The block type.
        /// </summary>
        private protected abstract EFlacMetadataBlockType Type { get; }

        private protected FlacMetadataBlock()
        {

        }

        void IFlacMetadataBlock.Write(Stream stream, bool isFinalBlock)
        {
            Write(stream, isFinalBlock);
        }

        protected virtual void Write(Stream stream, bool isFinalBlock)
        {
            byte isFinalAndTypeByte = isFinalBlock ? 0x80 : 0;
            isFinalAndTypeByte += (byte)((byte)Type & 0x7F);

            Span<byte> headerBytes = stackalloc byte[4];
            headerBytes[0] = isFinalAndTypeByte;

            // BlockLength is only 24 bytes, so we want to get the last 3 bytes of the int.
            BlockLength.GetBytes(headerBytes[1..]);

            stream.Write(headerBytes);
        }
    }
}
