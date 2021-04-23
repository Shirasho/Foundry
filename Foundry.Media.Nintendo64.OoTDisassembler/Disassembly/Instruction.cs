using System;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    /// <summary>
    /// Represents a MIPS RawInstruction.
    /// </summary>
    public readonly struct Instruction
    {
        /// <summary>
        /// The 32-bit raw RawInstruction value.
        /// </summary>
        public readonly uint RawInstruction { get; }

        /// <summary>
        /// The operation code.
        /// </summary>
        public readonly uint OperationCode => (RawInstruction & 0b11111100000000000000000000000000) >> 26;

        /// <summary>
        /// The function code.
        /// </summary>
        public readonly FunctionCode FunctionCode => FunctionCode.GetValue(RawInstruction & 0b00000000000000000000000000111111);

        /// <summary>
        /// The RS bytes.
        /// </summary>
        public readonly Register RS => (RawInstruction & 0b00000011111000000000000000000000) >> 21;

        /// <summary>
        /// The RT bytes.
        /// </summary>
        public readonly Register RT => (RawInstruction & 0b00000000000111110000000000000000) >> 16;

        /// <summary>
        /// The RD bytes.
        /// </summary>
        public readonly Register RD => (RawInstruction & 0b00000000000000001111100000000000) >> 11;

        /// <summary>
        /// The FT bytes.
        /// </summary>
        public readonly uint FT => (RawInstruction & 0b00000000000111110000000000000000) >> 16;

        /// <summary>
        /// The FS bytes.
        /// </summary>
        public readonly uint FS => (RawInstruction & 0b00000000000000001111100000000000) >> 11;

        /// <summary>
        /// The FD bytes.
        /// </summary>
        public readonly uint FD => (RawInstruction & 0b00000000000000000000011111000000) >> 6;

        /// <summary>
        /// The SHAMT/Shift bytes.
        /// </summary>
        public readonly uint Shift => (RawInstruction & 0b00000000000000000000011111000000) >> 6;

        /// <summary>
        /// The IMM/Immediate bytes.
        /// </summary>
        public readonly uint Immediate => RawInstruction & 0b00000000000000001111111111111111;

        /// <summary>
        /// The IMM/Immediate bytes as a signed value.
        /// </summary>
        public readonly long ImmediateSigned => ((Immediate & (1 << 15)) != 0)
                ? (long)Math.Pow(-2, 15) + (Immediate & 0b00000000000000000111111111111111)
                : Immediate;

        /// <summary>
        /// Whether this RawInstruction contains a load operation code.
        /// </summary>
        public readonly bool IsLoadOperationCode => OperationCode > 31;

        public Instruction(uint instruction)
        {
            RawInstruction = instruction;
        }

        public static implicit operator uint(Instruction RawInstruction)
        {
            return RawInstruction.RawInstruction;
        }

        public static implicit operator Instruction(uint RawInstruction)
        {
            return new Instruction(RawInstruction);
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public readonly bool Equals(uint other)
            => RawInstruction.Equals(other);

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///   <see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public readonly bool Equals(Instruction other)
            => RawInstruction.Equals(other.RawInstruction);

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public readonly override int GetHashCode()
        {
            return RawInstruction.GetHashCode();
        }

        public readonly override string ToString()
        {
            return $"Ox{RawInstruction:X}";
        }
    }
}
