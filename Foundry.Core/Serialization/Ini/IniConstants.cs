namespace Foundry.Serialization.Ini
{
    /// <summary>
    /// INI constant values.
    /// </summary>
    public static class IniConstants
    {
        /// <summary>
        /// The default char value that denotes a comment line.
        /// </summary>
        public const char DefaultCommentMarker = ';';

        /// <summary>
        /// The byte value of the default char value that denotes a comment line.
        /// </summary>
        public const byte DefaultCommentMarkerByte = (byte)DefaultCommentMarker;

        /// <summary>
        /// The default string value that denotes a comment line.
        /// </summary>
        public const string DefaultCommentMarkerString = ";";

        /// <summary>
        /// The byte value of the quotation char.
        /// </summary>
        public const byte ValueQuoteByte = (byte)'"';

        /// <summary>
        /// The byte value of the special character escape char (backslash).
        /// </summary>
        public const byte EscapeByte = (byte)'\\';

        /// <summary>
        /// The byte value of the space char.
        /// </summary>
        public const byte SpaceByte = (byte)' ';

        /// <summary>
        /// The byte value of the carriage return char.
        /// </summary>
        public const byte ReturnByte = (byte)'\r';

        /// <summary>
        /// The byte value of the newline char.
        /// </summary>
        public const byte NewlineByte = (byte)'\n';

        /// <summary>
        /// The byte value of the tab char.
        /// </summary>
        public const byte TabByte = (byte)'\t';

        /// <summary>
        /// The byte value of the section start marker char ('[').
        /// </summary>
        public const byte SectionStartMarkerByte = (byte)'[';

        /// <summary>
        /// The byte value of the section start marker char (']').
        /// </summary>
        public const byte SectionEndMarkerByte = (byte)']';

        /// <summary>
        /// The byte value that includes all control values.
        /// </summary>
        /// <remarks>
        /// This value is a <see cref="uint"/> to prevent casting on a hot path.
        /// </remarks>
        public const uint ControlCharLessThan = 32;

        /// <summary>
        /// The byte value that includes all control values and the whitespace value.
        /// </summary>
        /// <remarks>
        /// This value is a <see cref="uint"/> to prevent casting on a hot path.
        /// </remarks>
        public const uint WhiteSpaceLessThan = 33;
    }
}
