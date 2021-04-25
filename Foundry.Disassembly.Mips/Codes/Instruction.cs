namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// Represents an unprocessed MIPS instruction.
    /// </summary>
    public readonly struct Instruction
    {
        /// <summary>
        /// The raw MIPS instruction bits.
        /// </summary>
        public readonly HexCode RawInstruction { get; }

        /// <summary>
        /// The operation code of this instruction.
        /// </summary>
        public readonly OperationCode OpCode { get; }

        public Instruction(uint instruction)
        {
            RawInstruction = instruction;
            OpCode = new OperationCode(CodeMask.GetOpCode(RawInstruction));
        }

        public static implicit operator Instruction(uint instruction)
        {
            return new Instruction(instruction);
        }

        public static implicit operator HexCode(Instruction instruction)
        {
            return instruction.RawInstruction;
        }

        public readonly override string ToString()
        {
            return RawInstruction.ToString();
        }
    }
}
