namespace Foundry.Media.Flac
{
    internal enum EFlacMetadataBlockType : byte
    {
        /// <summary>
        /// Information about the FLAC stream.
        /// </summary>
        StreamInfo = 0,

        /// <summary>
        /// A block used solely for padding.
        /// </summary>
        Padding = 1,

        /// <summary>
        /// A block with application specific information.
        /// </summary>
        Application = 2,

        /// <summary>
        /// A block containing SeekTable information.
        /// </summary>
        SeekTable = 3,

        /// <summary>
        /// A block containing comments (artist, field, ...).
        /// </summary>
        Comment = 4,

        /// <summary>
        /// A block containing cue sheet information.
        /// </summary>
        CueSheet = 5,

        /// <summary>
        /// A block containing the picture bytes.
        /// </summary>
        Picture = 6,

        /// <summary>
        /// An unset, unknown, or invalid block type.
        /// </summary>
        None = 7
    }
}
