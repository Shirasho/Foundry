namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// <see cref="uint"/> extensions.
    /// </summary>
    public static class UInt32Extensions
    {
        /// <summary>
        /// Gets the register name for this <see cref="uint"/> value.
        /// If this value does not represent a valid register <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <remarks>
        /// This property does not guarantee that this instance is supposed to
        /// represent a register value.
        /// </remarks>
        public static string RegisterName(this in uint code)
            => Register.GetRegister(code).Name;

        /// <summary>
        /// Gets the register number for this <see cref="uint"/> value.
        /// If this value does not represent a valid register <see cref="string.Empty"/> is returned.
        /// </summary>
        /// <remarks>
        /// This property does not guarantee that this instance is supposed to
        /// represent a register value.
        /// </remarks>
        public static string RegisterNumber(this in uint code)
            => Register.GetRegister(code).Number;

        /// <summary>
        /// Gets the float register number for this <see cref="uint"/> value.
        /// </summary>
        /// <remarks>
        /// This method does not check whether this <see cref="uint"/> value
        /// represents a valid float register.
        /// </remarks>
        public static string FloatRegisterName(this in uint code)
        {
            // 31 represents a special register.
            if (code == 31)
            {
                return "$31";
            }

            return $"$f{code}";
        }

        /// <summary>
        /// Formats an address to include an offset.
        /// </summary>
        public static string FormatOffset(this in uint code, in uint offset)
        {
            if (offset == 0)
            {
                return code.ToString();
            }

            return $"0x{code:X8} + 0x{offset:X8}";
        }
    }
}
