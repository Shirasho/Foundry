namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// How to print out codes.
    /// </summary>
    public enum ECodeStringFormat
    {
        /// <summary>
        /// Print the decimal representation.
        /// </summary>
        Decimal,

        /// <summary>
        /// Print the hex representation.
        /// </summary>
        Hex,

        /// <summary>
        /// Print the instruction name. If the code is not valid or has
        /// no instruction <see cref="Hex"/> will be used.
        /// </summary>
        Instruction
    }
}
