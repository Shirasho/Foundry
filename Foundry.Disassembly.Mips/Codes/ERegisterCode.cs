using System;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Disassembly.Mips.Codes
{
    public readonly struct RegisterCode : IEquatable<RegisterCode>, IEquatable<uint>
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public enum Code : uint
        {
            [Instruction("$zero")] Zero = 0,
            [Instruction("$at")] At = 1,
            [Instruction("$v0")] V0 = 2,
            [Instruction("$v1")] V1 = 3,
            [Instruction("$a0")] A0 = 4,
            [Instruction("$a1")] A1 = 5,
            [Instruction("$a2")] A2 = 6,
            [Instruction("$a3")] A3 = 7,
            [Instruction("$t0")] T0 = 8,
            [Instruction("$t1")] T1 = 9,
            [Instruction("$t2")] T2 = 10,
            [Instruction("$t3")] T3 = 11,
            [Instruction("$t4")] T4 = 12,
            [Instruction("$t5")] T5 = 13,
            [Instruction("$t6")] T6 = 14,
            [Instruction("$t7")] T7 = 15,
            [Instruction("$s0")] S0 = 16,
            [Instruction("$s1")] S1 = 17,
            [Instruction("$s2")] S2 = 18,
            [Instruction("$s3")] S3 = 19,
            [Instruction("$s4")] S4 = 20,
            [Instruction("$s5")] S5 = 21,
            [Instruction("$s6")] S6 = 22,
            [Instruction("$s7")] S7 = 23,
            [Instruction("$t8")] T8 = 24,
            [Instruction("$t9")] T9 = 25,
            [Instruction("$k0")] K0 = 26,
            [Instruction("$k1")] K1 = 27,
            [Instruction("$gp")] Gp = 28,
            [Instruction("$sp")] Sp = 29,
            [Instruction("$fp")] Fp = 30,
            [Instruction("$ra")] Ra = 31
        }

        /// <summary>
        /// The register code.
        /// </summary>
        public readonly Code RegCode { get; }

        /// <summary>
        /// The name of this register.
        /// </summary>
        public readonly string RegisterName => RegCode.HasAttribute<Code, InstructionAttribute>(out var attr) ? attr.Instruction : string.Empty;

        /// <summary>
        /// The number of this register.
        /// </summary>
        public readonly string RegisterNumber => $"${(uint)RegCode}";

        /// <summary>
        /// Whether this register instance represents a valid register.
        /// </summary>
        public readonly bool IsValid => RegCode.IsDefined();

        public RegisterCode(uint code)
            : this((Code)code)
        {

        }

        public RegisterCode(Code code)
        {
            RegCode = code;
        }

        public static implicit operator Code(in RegisterCode code)
        {
            return code.RegCode;
        }

        public static implicit operator RegisterCode(Code code)
        {
            return new RegisterCode(code);
        }

        public readonly bool Equals(RegisterCode other)
            => RegCode == other.RegCode;

        public readonly bool Equals(uint other)
            => (uint)RegCode == other;

        public readonly override bool Equals(object? obj)
        {
            return obj is RegisterCode rc && Equals(rc);
        }

        public readonly override int GetHashCode()
        {
            return RegCode.GetHashCode();
        }

        public static bool operator ==(in RegisterCode left, in RegisterCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in RegisterCode left, in RegisterCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(in RegisterCode left, Code right)
        {
            return left.RegCode == right;
        }

        public static bool operator !=(in RegisterCode left, Code right)
        {
            return !(left == right);
        }

        public static bool operator ==(in RegisterCode left, uint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in RegisterCode left, uint right)
        {
            return !(left == right);
        }

        public readonly override string ToString()
            => ToString(ECodeStringFormat.Instruction);

        public readonly string ToString(ECodeStringFormat format)
        {
            return format switch
            {
                ECodeStringFormat.Decimal => RegCode.ToString("D"),
                ECodeStringFormat.Hex => $"0x{(uint)RegCode:X4}",
                ECodeStringFormat.Instruction when format.IsDefined() => RegCode.ToString(),
                _ => $"0x{(uint)RegCode:X4}"
            };
        }
    }
}
