namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// Represents a J-Type MIPS instruction.
    /// </summary>
    public readonly struct InstructionJ
    {
        /// <summary>
        /// The raw MIPS instruction bits.
        /// </summary>
        public readonly HexCode RawInstruction { get; }

        /// <summary>
        /// The OpCode value.
        /// </summary>
        public readonly HexCode OpCode { get; }

        /// <summary>
        /// The addr value.
        /// </summary>
        public readonly HexCode Addr { get; }

        public InstructionJ(uint instruction)
        {
            RawInstruction = instruction;
            OpCode = CodeMask.GetOpCode(RawInstruction);
            Addr = CodeMask.JType.GetAddr(RawInstruction);
        }

        public static implicit operator InstructionJ(uint instruction)
        {
            return new InstructionJ(instruction);
        }

        public static explicit operator InstructionJ(in Instruction instruction)
        {
            return new InstructionJ(instruction.RawInstruction);
        }

        public static implicit operator HexCode(InstructionJ instruction)
        {
            return instruction.RawInstruction;
        }

        public readonly override string ToString()
        {
            return $"0x{RawInstruction:X8}";
        }
    }
}
