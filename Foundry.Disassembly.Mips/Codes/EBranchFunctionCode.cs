using System;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Disassembly.Mips.Codes
{
    public readonly struct BranchFunctionCode : IEquatable<BranchFunctionCode>, IEquatable<uint>
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public enum Code : uint
        {
            [Instruction("bltz")] Bltz = 0,
            [Instruction("bgez")] Bgez = 1,
            [Instruction("bltzl")] Bltzl = 2,
            [Instruction("bgezl")] Bgezl = 3,
            [Instruction("tgei")] Tgei = 8,
            [Instruction("tgeiu")] Tgeiu = 9,
            [Instruction("tlti")] Tlti = 10,
            [Instruction("tltiu")] Tltiu = 11,
            [Instruction("tegi")] Tegi = 12,
            [Instruction("tnei")] Tnei = 14,
            [Instruction("bltzal")] Bltzal = 16,
            [Instruction("bgezal")] Bgezal = 17,
            [Instruction("bltall")] Bltall = 18,
            [Instruction("bgczall")] Bgczall = 19
        }

        /// <summary>
        /// The function code.
        /// </summary>
        public readonly Code BranchCode { get; }

        /// <summary>
        /// The instruction for this branch code.
        /// </summary>
        public readonly string Instruction => BranchCode.HasAttribute<Code, InstructionAttribute>(out var attr) ? attr.Instruction : string.Empty;

        /// <summary>
        /// Whether this branch code instance represents a valid function code.
        /// </summary>
        public readonly bool IsValid => BranchCode.IsDefined();

        public BranchFunctionCode(uint code)
            : this((Code)code)
        {

        }

        public BranchFunctionCode(Code code)
        {
            BranchCode = code;
        }

        public static implicit operator Code(in BranchFunctionCode code)
        {
            return code.BranchCode;
        }

        public static implicit operator BranchFunctionCode(Code code)
        {
            return new BranchFunctionCode(code);
        }

        public readonly bool Equals(BranchFunctionCode other)
            => BranchCode == other.BranchCode;

        public readonly bool Equals(uint other)
            => (uint)BranchCode == other;

        public readonly override bool Equals(object? obj)
        {
            return obj is BranchFunctionCode rc && Equals(rc);
        }

        public readonly override int GetHashCode()
        {
            return BranchCode.GetHashCode();
        }

        public static bool operator ==(in BranchFunctionCode left, in BranchFunctionCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in BranchFunctionCode left, in BranchFunctionCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(in BranchFunctionCode left, Code right)
        {
            return left.BranchCode == right;
        }

        public static bool operator !=(in BranchFunctionCode left, Code right)
        {
            return !(left == right);
        }

        public static bool operator ==(in BranchFunctionCode left, uint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in BranchFunctionCode left, uint right)
        {
            return !(left == right);
        }

        public readonly override string ToString()
            => ToString(ECodeStringFormat.Instruction);

        public readonly string ToString(ECodeStringFormat format)
        {
            return format switch
            {
                ECodeStringFormat.Decimal => BranchCode.ToString("D"),
                ECodeStringFormat.Hex => $"0x{(uint)BranchCode:X4}",
                ECodeStringFormat.Instruction when format.IsDefined() => BranchCode.ToString(),
                _ => $"0x{(uint)BranchCode:X4}"
            };
        }
    }
}
