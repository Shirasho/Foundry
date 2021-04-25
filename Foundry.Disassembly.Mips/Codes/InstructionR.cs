namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// Represents a R-Type MIPS instruction.
    /// </summary>
    public readonly struct InstructionR
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
        /// The RD value.
        /// </summary>
        public readonly HexCode RD { get; }

        /// <summary>
        /// The FD value.
        /// </summary>
        public readonly HexCode FD { get; }

        /// <summary>
        /// The FS value.
        /// </summary>
        public readonly HexCode FS { get; }

        /// <summary>
        /// The FT value.
        /// </summary>
        public readonly HexCode FT { get; }

        /// <summary>
        /// The shamd/shift value.
        /// </summary>
        public readonly HexCode Shamd { get; }

        /// <summary>
        /// The funct value.
        /// </summary>
        public readonly HexCode Funct { get; }

        /// <summary>
        /// The function code created from <see cref="Funct"/>.
        /// </summary>
        public readonly FunctionCode FunctionCode { get; }

        /// <summary>
        /// The float function code created from <see cref="Funct"/>.
        /// </summary>
        public readonly FloatFunctionCode FloatFunctionCode { get; }

        public InstructionR(uint instruction)
        {
            RawInstruction = instruction;
            OpCode = CodeMask.GetOpCode(RawInstruction);
            RS = CodeMask.RType.GetRS(RawInstruction);
            RT = CodeMask.RType.GetRT(RawInstruction);
            RD = CodeMask.RType.GetRD(RawInstruction);
            FD = CodeMask.RType.GetFD(RawInstruction);
            FS = CodeMask.RType.GetFS(RawInstruction);
            FT = CodeMask.RType.GetFT(RawInstruction);
            Shamd = CodeMask.RType.GetShamd(RawInstruction);
            Funct = CodeMask.RType.GetFunct(RawInstruction);
            FunctionCode = new FunctionCode(Funct);
            FloatFunctionCode = new FloatFunctionCode(Funct);
        }

        public static implicit operator InstructionR(uint instruction)
        {
            return new InstructionR(instruction);
        }

        public static explicit operator InstructionR(in Instruction instruction)
        {
            return new InstructionR(instruction.RawInstruction);
        }

        public static implicit operator HexCode(InstructionR instruction)
        {
            return instruction.RawInstruction;
        }

        public readonly override string ToString()
        {
            return $"0x{RawInstruction:X8}";
        }
    }
}
