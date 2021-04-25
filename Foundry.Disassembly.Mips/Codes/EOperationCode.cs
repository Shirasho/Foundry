using System;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Disassembly.Mips.Codes
{
    public readonly struct OperationCode : IEquatable<OperationCode>, IEquatable<uint>
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public enum Code : uint
        {
            Spec_RType = 0,
            Spec_Branch = 1,
            [Instruction("j")] J = 2,
            [Instruction("jal")] Jal = 3,
            [Instruction("beq")] Beq = 4,
            [Instruction("bne")] Bne = 5,
            [Instruction("blez")] Blez = 6,
            [Instruction("bgtz")] Bgtz = 7,
            [Instruction("addi")] Addi = 8,
            [Instruction("addiu")] Addiu = 9,
            [Instruction("slti")] Slti = 10,
            [Instruction("sltiu")] Sltiu = 11,
            [Instruction("andi")] Andi = 12,
            [Instruction("ori")] Ori = 13,
            [Instruction("xori")] Xori = 14,
            [Instruction("lui")] Lui = 15,
            Spec_Coprocessor0 = 16,
            Spec_Coprocessor1 = 17,
            Spec_Coprocessor2 = 18,
            [Instruction("beql")] Beql = 20,
            [Instruction("bnel")] Bnel = 21,
            [Instruction("blezl")] Blezl = 22,
            [Instruction("bgtzl")] Bgtzl = 23,
            [Instruction("daddi")] Daddi = 24,
            [Instruction("daddiu")] Daddiu = 25,
            [Instruction("lb")] Lb = 32,
            [Instruction("lh")] Lh = 33,
            [Instruction("lwl")] Lwl = 34,
            [Instruction("lw")] Lw = 35,
            [Instruction("lbu")] Lbu = 36,
            [Instruction("lhu")] Lhu = 37,
            [Instruction("lwr")] Lwr = 38,
            [Instruction("sb")] Sb = 40,
            [Instruction("sh")] Sh = 41,
            [Instruction("swl")] Swl = 42,
            [Instruction("sw")] Sw = 43,
            [Instruction("swr")] Swr = 46,
            [Instruction("cache")] Cache = 47,
            [Instruction("lwc1")] Lwc1 = 49,
            [Instruction("ld")] Ld = 55,
            [Instruction("pref")] Pref = 51,
            [Instruction("ll")] Ll = 48,
            [Instruction("lwc2")] Lwc2 = 50,
            [Instruction("ldc1")] Ldc1 = 53,
            [Instruction("ldc2")] Ldc2 = 54,
            [Instruction("swc1")] Swc1 = 57,
            [Instruction("sdc1")] Sdc1 = 61,
            [Instruction("sd")] Sd = 63,
            [Instruction("sc")] Sc = 56,
            [Instruction("swc2")] Swc2 = 58,
            [Instruction("sdc2")] Sdc2 = 62
        }

        /// <summary>
        /// The operation code.
        /// </summary>
        public readonly Code OpCode { get; }

        /// <summary>
        /// The instruction for this operation code.
        /// </summary>
        public readonly string Instruction => OpCode.HasAttribute<Code, InstructionAttribute>(out var attr) ? attr.Instruction : string.Empty;

        /// <summary>
        /// Whether this operation code instance represents a valid operation.
        /// </summary>
        public readonly bool IsValid => OpCode.IsDefined();

        public OperationCode(uint code)
            : this((Code)code)
        {

        }

        public OperationCode(Code code)
        {
            OpCode = code;
        }

        public static implicit operator Code(in OperationCode code)
        {
            return code.OpCode;
        }

        public static implicit operator OperationCode(Code code)
        {
            return new OperationCode(code);
        }

        public readonly bool Equals(OperationCode other)
            => OpCode == other.OpCode;

        public readonly bool Equals(uint other)
            => (uint)OpCode == other;

        public readonly override bool Equals(object? obj)
        {
            return obj is OperationCode rc && Equals(rc);
        }

        public readonly override int GetHashCode()
        {
            return OpCode.GetHashCode();
        }

        public static bool operator ==(in OperationCode left, in OperationCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in OperationCode left, in OperationCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(in OperationCode left, Code right)
        {
            return left.OpCode == right;
        }

        public static bool operator !=(in OperationCode left, Code right)
        {
            return !(left == right);
        }

        public static bool operator ==(in OperationCode left, uint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in OperationCode left, uint right)
        {
            return !(left == right);
        }

        public readonly override string ToString()
            => ToString(ECodeStringFormat.Instruction);

        public readonly string ToString(ECodeStringFormat format)
        {
            return format switch
            {
                ECodeStringFormat.Decimal => OpCode.ToString("D"),
                ECodeStringFormat.Hex => $"0x{(uint)OpCode:X4}",
                ECodeStringFormat.Instruction when format.IsDefined() => OpCode.ToString(),
                _ => $"0x{(uint)OpCode:X4}"
            };
        }
    }
}
