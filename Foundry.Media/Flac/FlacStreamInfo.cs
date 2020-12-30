namespace Foundry.Media.Flac
{
    /// <summary>
    /// Information about the FLAC stream.
    /// </summary>
    public class FlacStreamInfo : FlacMetadataBlock
    {
        /// <summary>
        /// The block length.
        /// </summary>
        public override int BlockLength { get; } = 34;

        private protected override EFlacMetadataBlockType Type => EFlacMetadataBlockType.StreamInfo;

        internal FlacStreamInfo()
        {

        }
    }
}
