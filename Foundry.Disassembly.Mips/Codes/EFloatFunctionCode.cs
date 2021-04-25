using System;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Disassembly.Mips.Codes
{
    public readonly struct FloatFunctionCode : IEquatable<FloatFunctionCode>, IEquatable<uint>
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public enum Code : uint
        {
            [Instruction("add")] Add = 0,
            [Instruction("sub")] Sub = 1,
            [Instruction("mul")] Mul = 2,
            [Instruction("div")] Div = 3,
            [Instruction("sqrt")] Sqrt = 4,
            [Instruction("abs")] Abs = 5,
            [Instruction("mov")] Mov = 6,
            [Instruction("neg")] Neg = 7,
            [Instruction("round.l")] Roundl = 8,
            [Instruction("trunc.l")] Truncl = 9,
            [Instruction("ceil.l")] Ceill = 10,
            [Instruction("floor.l")] Floorl = 11,
            [Instruction("round.w")] Roundw = 12,
            [Instruction("trunc.w")] Truncw = 13,
            [Instruction("ceil.w")] Ceilw = 14,
            [Instruction("floor.w")] Floorw = 15,
            [Instruction("movz")] Movz = 18,
            [Instruction("movn")] Movn = 19,
            [Instruction("cvt.s")] Cvts = 32,
            [Instruction("cvt.d")] Cvtd = 33,
            [Instruction("cvt.w")] Cvtw = 36,
            [Instruction("cvt.l")] Cvtl = 37,
            [Instruction("c.f")] Cf = 48,
            [Instruction("c.un")] Cun = 49,
            [Instruction("c.eq")] Ceq = 50,
            [Instruction("c.ueq")] Cueq = 51,
            [Instruction("c.olt")] Colt = 52,
            [Instruction("c.ult")] Cult = 53,
            [Instruction("c.ole")] Cole = 54,
            [Instruction("c.ule")] Cule = 55,
            [Instruction("c.sf")] Csf = 56,
            [Instruction("c.ngle")] Cngle = 57,
            [Instruction("c.seq")] Cseq = 58,
            [Instruction("c.ngl")] Cngl = 59,
            [Instruction("c.lt")] Clt = 60,
            [Instruction("c.nge")] Cnge = 61,
            [Instruction("c.le")] Cle = 62,
            [Instruction("c.ngt")] Cngt = 63
        }

        /// <summary>
        /// The float function code.
        /// </summary>
        public readonly Code FuncCode { get; }

        /// <summary>
        /// The instruction for this float function code.
        /// </summary>
        public readonly string Instruction => FuncCode.HasAttribute<Code, InstructionAttribute>(out var attr) ? attr.Instruction : string.Empty;

        /// <summary>
        /// Whether this function code instance represents a valid function code.
        /// </summary>
        public readonly bool IsValid => FuncCode.IsDefined();

        public FloatFunctionCode(uint code)
            : this((Code)code)
        {

        }

        public FloatFunctionCode(Code code)
        {
            FuncCode = code;
        }

        public static implicit operator Code(in FloatFunctionCode code)
        {
            return code.FuncCode;
        }

        public static implicit operator FloatFunctionCode(Code code)
        {
            return new FloatFunctionCode(code);
        }

        public readonly bool Equals(FloatFunctionCode other)
            => FuncCode == other.FuncCode;

        public readonly bool Equals(uint other)
            => (uint)FuncCode == other;

        public readonly override bool Equals(object? obj)
        {
            return obj is FloatFunctionCode rc && Equals(rc);
        }

        public readonly override int GetHashCode()
        {
            return FuncCode.GetHashCode();
        }

        public static bool operator ==(in FloatFunctionCode left, in FloatFunctionCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in FloatFunctionCode left, in FloatFunctionCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(in FloatFunctionCode left, Code right)
        {
            return left.FuncCode == right;
        }

        public static bool operator !=(in FloatFunctionCode left, Code right)
        {
            return !(left == right);
        }

        public static bool operator ==(in FloatFunctionCode left, uint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in FloatFunctionCode left, uint right)
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
