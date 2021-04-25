using System;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Disassembly.Mips.Codes
{
    public readonly struct FunctionCode : IEquatable<FunctionCode>, IEquatable<uint>
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public enum Code : uint
        {
            [Instruction("sll")] Sll = 0,
            [Instruction("srl")] Srl = 2,
            [Instruction("sra")] Sra = 3,
            [Instruction("sllv")] Sllv = 4,
            [Instruction("srlv")] Srlv = 6,
            [Instruction("srav")] Srav = 7,
            [Instruction("jr")] Jr = 8,
            [Instruction("jalr")] Jalr = 9,
            [Instruction("movz")] Movz = 10,
            [Instruction("movn")] Movn = 11,
            [Instruction("syscall")] Syscall = 12,
            [Instruction("break")] Break = 13,
            [Instruction("sync")] Sync = 15,
            [Instruction("mfhi")] Mfhi = 16,
            [Instruction("mthi")] Mthi = 17,
            [Instruction("mflo")] Mflo = 18,
            [Instruction("mtlo")] Mtlo = 19,
            [Instruction("dsllv")] Dsllv = 20,
            [Instruction("dsrlv")] Dsrlv = 22,
            [Instruction("dsrav")] Dsrav = 23,
            [Instruction("mult")] Mult = 24,
            [Instruction("multu")] Multu = 25,
            [Instruction("div")] Div = 26,
            [Instruction("divu")] Divu = 27,
            [Instruction("dmult")] Dmult = 28,
            [Instruction("dmultu")] Dmultu = 29,
            [Instruction("ddiv")] Ddiv = 30,
            [Instruction("ddivu")] Ddivu = 31,
            [Instruction("add")] Add = 32,
            [Instruction("addu")] Addu = 33,
            [Instruction("sub")] Sub = 34,
            [Instruction("subu")] Subu = 35,
            [Instruction("and")] And = 36,
            [Instruction("or")] Or = 37,
            [Instruction("xor")] Xor = 38,
            [Instruction("nor")] Nor = 39,
            [Instruction("slt")] Slt = 42,
            [Instruction("sltu")] Sltu = 43,
            [Instruction("dadd")] Dadd = 44,
            [Instruction("daddu")] Daddu = 45,
            [Instruction("tge")] Tge = 48,
            [Instruction("tgeu")] Tgeu = 49,
            [Instruction("tlt")] Tlt = 50,
            [Instruction("teq")] Teq = 52,
            [Instruction("tne")] Tne = 54,
            [Instruction("tltu")] Tltu = 51,
            [Instruction("dsll")] Dsll = 56,
            [Instruction("dsra")] Dsra = 59,
            [Instruction("dsll32")] Dsll32 = 60,
            [Instruction("dsra32")] Dsra32 = 63
        }

        /// <summary>
        /// The function code.
        /// </summary>
        public readonly Code FuncCode { get; }

        /// <summary>
        /// The instruction for this function code.
        /// </summary>
        public readonly string Instruction => FuncCode.HasAttribute<Code, InstructionAttribute>(out var attr) ? attr.Instruction : string.Empty;

        /// <summary>
        /// Whether this function code instance represents a valid function code.
        /// </summary>
        public readonly bool IsValid => FuncCode.IsDefined();

        public FunctionCode(uint code)
            : this((Code)code)
        {

        }

        public FunctionCode(Code code)
        {
            FuncCode = code;
        }

        public static implicit operator Code(in FunctionCode code)
        {
            return code.FuncCode;
        }

        public static implicit operator FunctionCode(Code code)
        {
            return new FunctionCode(code);
        }

        public readonly bool Equals(FunctionCode other)
            => FuncCode == other.FuncCode;

        public readonly bool Equals(uint other)
            => (uint)FuncCode == other;

        public readonly override bool Equals(object? obj)
        {
            return obj is FunctionCode rc && Equals(rc);
        }

        public readonly override int GetHashCode()
        {
            return FuncCode.GetHashCode();
        }

        public static bool operator ==(in FunctionCode left, in FunctionCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in FunctionCode left, in FunctionCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(in FunctionCode left, Code right)
        {
            return left.FuncCode == right;
        }

        public static bool operator !=(in FunctionCode left, Code right)
        {
            return !(left == right);
        }

        public static bool operator ==(in FunctionCode left, uint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in FunctionCode left, uint right)
        {
            return !(left == right);
        }

        public readonly override string ToString()
            => ToString(ECodeStringFormat.Instruction);

        public readonly string ToString(ECodeStringFormat format)
        {
            return format switch
            {
                ECodeStringFormat.Decimal => FuncCode.ToString("D"),
                ECodeStringFormat.Hex => $"0x{(uint)FuncCode:X4}",
                ECodeStringFormat.Instruction when format.IsDefined() => FuncCode.ToString(),
                _ => $"0x{(uint)FuncCode:X4}"
            };
        }
    }
}
