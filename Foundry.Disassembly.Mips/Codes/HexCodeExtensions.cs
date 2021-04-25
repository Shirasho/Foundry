namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// <see cref="HexCode"/> extensions.
    /// </summary>
    public static class HexCodeExtensions
    {
        /// <summary>
        /// Returns this instance as a <see cref="RegisterCode"/>.
        /// </summary>
        /// <remarks>
        /// This property does not guarantee that this instance is supposed to
        /// represent a register value.
        /// </remarks>
        public static RegisterCode AsRegisterCode(this in HexCode code) => new RegisterCode(code.Value);

        /// <summary>
        /// Gets the register name for this <see cref="HexCode"/> value.
        /// If this value does not represent a valid register <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <remarks>
        /// This property does not guarantee that this instance is supposed to
        /// represent a register value.
        /// </remarks>
        public static string RegisterName(this in HexCode code) => AsRegisterCode(code).RegisterName;

        /// <summary>
        /// Gets the register number for this <see cref="HexCode"/> value.
        /// If this value does not represent a valid register <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <remarks>
        /// This property does not guarantee that this instance is supposed to
        /// represent a register value.
        /// </remarks>
        public static string RegisterNumber(this in HexCode code) => AsRegisterCode(code).RegisterNumber;

        /// <summary>
        /// Gets the float register number for this <see cref="HexCode"/> value.
        /// </summary>
        /// <remarks>
        /// This method does not check whether this <see cref="HexCode"/> value
        /// represents a valid float register.
        /// </remarks>
        public static string FloatRegisterName(this in HexCode code)
        {
            if (code == 31)
            {
                return "$31";
            }

            return $"$f{code.Value}";
        }

        /// <summary>
        /// Formats an address to include an offset.
        /// </summary>
        public static string FormatOffset(this in HexCode code, SignedHexCode offset)
        {
            if (offset == 0)
            {
                return code.ToString();
            }

            return $"{code} + {offset}";
        }

        /// <summary>
        /// Formats an address to include an offset.
        /// </summary>
        public static string FormatOffset(this in HexCode code, in HexCode offset)
        {
            if (offset == 0)
            {
                return code.ToString();
            }

            return $"{code} + {offset}";
        }
    }
}
