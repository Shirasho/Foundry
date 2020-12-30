using System;
using System.IO;
using Foundry.IO;

namespace Foundry.Media.Flac
{
    /// <summary>
    /// Metadata that describes a FLAC stream.
    /// </summary>
    public class FlacMetadata
    {
        /// <summary>
        /// The byte offset of the first audio block in the stream.
        /// </summary>
        public int DataOffset { get; }

        /// <summary>
        /// Information about the FLAC stream.
        /// </summary>
        public FlacStreamInfo StreamInfo { get; private set; }

        private FlacMetadata(int dataOffset)
        {
            DataOffset = dataOffset;

            StreamInfo = null!;
        }

        internal static FlacMetadata Create(int specificationByteLength, Stream stream)
        {
            /**
             * https://xiph.org/flac/format.html#metadata_block
             * Headers are comprised of 4 bytes:
             * 
             * Bit 1: Is Final Metadata Block
             * Bit 2-8: Block Type
             * Bit 9-32: Metadata length
             */

            
            var reader = new BufferedBinaryReader(stream);
            bool isFinalBlock;
            Span<byte> blockLengthBytes = stackalloc byte[3];
            do
            {
                byte blockInfo = reader.ReadByte();
                isFinalBlock = (blockInfo & 1) == 1;

                BufferedBinaryReader.ReadBytes(reader, blockLengthBytes);
                int blockLength = (blockLengthBytes[2]) | (blockLengthBytes[1] << 8) | (blockLengthBytes[0] << 16);

                switch ((EFlacMetadataBlockType)(blockInfo & 0x7F))
                {
                    case EFlacMetadataBlockType.StreamInfo:
                        StreamInfo = new FlacStreamInfo(blockLength);

                }
            }
            while (!isFinalBlock);
        }
    }
}
