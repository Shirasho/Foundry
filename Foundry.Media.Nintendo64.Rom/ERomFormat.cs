namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// The format of a ROM.
    /// </summary>
    public enum ERomFormat : byte
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown,

        /// <summary>
        /// .n64
        /// </summary>
        LittleEndian,

        /// <summary>
        /// .v64
        /// </summary>
        ByteSwapped,

        /// <summary>
        /// .z64
        /// </summary>
        BigEndian,

        /// <summary>
        /// Invalid
        /// </summary>
        Invalid,
    }
}
