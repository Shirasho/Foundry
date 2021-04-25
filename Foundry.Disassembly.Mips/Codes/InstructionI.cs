namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// Represents an I-Type MIPS instruction.
    /// </summary>
    public readonly struct InstructionI
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
        /// The RS value.
        /// </summary>
        public readonly HexCode RS { get; }

        /// <summary>
        /// The RT value.
        /// </summary>
        public readonly HexCode RT { get; }

        /// <summary>
        /// The IMM value.
        /// </summary>
        public readonly HexCode IMM { get; }

        public readonly SignedHexCode IMMSigned { get; }

        public InstructionI(uint instruction)
        {
            RawInstruction = instruction;
            OpCode = CodeMask.GetOpCode(RawInstruction);
            RS = CodeMask.IType.GetRS(RawInstruction);
            RT = CodeMask.IType.GetRT(RawInstruction);
            IMM = CodeMask.IType.GetIMM(RawInstruction);
            IMMSigned = CodeMask.IType.GetIMMSigned(RawInstruction);

        }

        public static implicit operator InstructionI(uint instruction)
        {
            return new InstructionI(instruction);
        }

        public static explicit operator InstructionI(in Instruction instruction)
        {
            return new InstructionI(instruction.RawInstruction);
        }

        public static implicit operator HexCode(InstructionI instruction)
        {
            return instruction.RawInstruction;
        }

        public readonly override string ToString()
        {
            return $"0x{RawInstruction:X8}";
        }
    }
}
