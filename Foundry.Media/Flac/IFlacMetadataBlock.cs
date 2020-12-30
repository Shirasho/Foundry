using System.IO;

namespace Foundry.Media.Flac
{
    internal interface IFlacMetadataBlock
    {
        /// <summary>
        /// The number of bytes this metadata block takes up.
        /// </summary>
        int BlockLength { get; }

        /// <summary>
        /// Writes this metadata block to the specified stream.
        /// </summary>
        /// <param name="stream">The stream to write this block to.</param>
        /// <param name="isFinalBlock">Whether this is the final metadata block.</param>
        internal void Write(Stream stream, bool isFinalBlock);
    }
}
