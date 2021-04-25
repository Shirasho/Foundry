using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Foundry.Reflection;

namespace Foundry.Disassembly.Mips.Codes
{
    public readonly struct CoprocessorFunctionCode : IEquatable<CoprocessorFunctionCode>, IEquatable<uint>
    {
        [SuppressMessage("Style", "IDE1006:Naming Styles")]
        public enum Code : uint
        {
            [Instruction("lwc0"), Description("Load word from coprocessor 0")] Lwc0,
            [Instruction("lwc1"), Description("Load word from coprocessor 1")] Lwc1,
            [Instruction("lwc2"), Description("Load word from coprocessor 2")] Lwc2,

            [Instruction("ldc0"), Description("Load double from coprocessor 0")] Ldc0,
            [Instruction("ldc1"), Description("Load double from coprocessor 1")] Ldc1,
            [Instruction("ldc2"), Description("Load double from coprocessor 2")] Ldc2,

            [Instruction("swc0"), Description("Store word into coprocessor 0")] Swc0,
            [Instruction("swc1"), Description("Store word into coprocessor 1")] Swc1,
            [Instruction("swc2"), Description("Store word into coprocessor 2")] Swc2,

            [Instruction("sdc0"), Description("Store double into coprocessor 0")] Sdc0,
            [Instruction("sdc1"), Description("Store double into coprocessor 1")] Sdc1,
            [Instruction("sdc2"), Description("Store double into coprocessor 2")] Sdc2,

            [Instruction("mfc0"), Description("Move word from coprocessor 0")] Mfc0,
            [Instruction("mfc1"), Description("Move word from coprocessor 1")] Mfc1,
            [Instruction("mfc2"), Description("Move word from coprocessor 2")] Mfc2,

            [Instruction("mtc0"), Description("Move word to coprocessor 0")] Mtc0,
            [Instruction("mtc1"), Description("Move word to coprocessor 1")] Mtc1,
            [Instruction("mtc2"), Description("Move word to coprocessor 2")] Mtc2,

            [Instruction("dmfc0"), Description("Move doubleword from coprocessor 0")] Dmfc0,
            [Instruction("dmfc1"), Description("Move doubleword from coprocessor 1")] Dmfc1,
            [Instruction("dmfc2"), Description("Move doubleword from coprocessor 2")] Dmfc2,

            [Instruction("dmtc0"), Description("Move doubleword to coprocessor 0")] Dmtc0,
            [Instruction("dmtc1"), Description("Move doubleword to coprocessor 1")] Dmtc1,
            [Instruction("dmtc2"), Description("Move doubleword to coprocessor 2")] Dmtc2,

            [Instruction("bc0f"), Description("Branch to the specified label when coprocessor 0 asserts a false condition.")] Bc0f,
            [Instruction("bc1f"), Description("Branch to the specified label when coprocessor 1 asserts a false condition.")] Bc1f,
            [Instruction("bc2f"), Description("Branch to the specified label when coprocessor 2 asserts a false condition.")] Bc2f,

            [Instruction("bc0t"), Description("Branch to the specified label when coprocessor 0 asserts a true condition")] Bc0t,
            [Instruction("bc1t"), Description("Branch to the specified label when coprocessor 1 asserts a true condition")] Bc1t,
            [Instruction("bc2t"), Description("Branch to the specified label when coprocessor 2 asserts a true condition")] Bc2t,

            [Instruction("bc0fl"), Description("Branch to the specified label when coprocessor 0 asserts a false condition (likely variant).")] Bc0fl,
            [Instruction("bc1fl"), Description("Branch to the specified label when coprocessor 1 asserts a false condition (likely variant).")] Bc1fl,
            [Instruction("bc2fl"), Description("Branch to the specified label when coprocessor 2 asserts a false condition (likely variant).")] Bc2fl,

            [Instruction("bc0tl"), Description("Branch to the specified label when coprocessor 0 asserts a true condition (likely variant)")] Bc0tl,
            [Instruction("bc1tl"), Description("Branch to the specified label when coprocessor 1 asserts a true condition (likely variant)")] Bc1tl,
            [Instruction("bc2tl"), Description("Branch to the specified label when coprocessor 2 asserts a true condition (likely variant)")] Bc2tl,

            [Instruction("c0"), Description("Execute operation on coprocessor 0")] C0,
            [Instruction("c1"), Description("Execute operation on coprocessor 1")] C1,
            [Instruction("c2"), Description("Execute operation on coprocessor 2")] C2,

            [Instruction("cfc0"), Description("Store the content of coprocessor 0 control register into general register")] Cfc0,
            [Instruction("cfc1"), Description("Store the content of coprocessor 1 control register into general register")] Cfc1,
            [Instruction("cfc2"), Description("Store the content of coprocessor 2 control register into general register")] Cfc2,

            [Instruction("ctc0"), Description("Store the content of general register into control register of coprocessor 0")] Ctc0,
            [Instruction("ctc1"), Description("Store the content of general register into control register of coprocessor 1")] Ctc1,
            [Instruction("ctc2"), Description("Store the content of general register into control register of coprocessor 1")] Ctc2
        }

        /// <summary>
        /// The coprocessor function code.
        /// </summary>
        public readonly Code FuncCode { get; }

        /// <summary>
        /// The instruction for this coprocessor function code.
        /// </summary>
        public readonly string Instruction => FuncCode.HasAttribute<Code, InstructionAttribute>(out var attr) ? attr.Instruction : string.Empty;

        /// <summary>
        /// The coprocessor this function code targets.
        /// </summary>
        /// <exception cref="InvalidOperationException">This instance does not point to a valid <see cref="Code"/> operation.</exception>
        public readonly uint Coprocessor
        {
            get
            {
                if (!IsValid)
                {
                    throw new InvalidOperationException("This coprocessor code is not valid.");
                }

                string name = FuncCode.ToString();
                char num = name.First(static c => char.IsNumber(c));
                return (uint)(-48 + num);
            }
        }

        /// <summary>
        /// Whether this coprocessor function code instance represents a valid operation.
        /// </summary>
        public readonly bool IsValid => FuncCode.IsDefined();

        public CoprocessorFunctionCode(uint code)
            : this((Code)code)
        {

        }

        public CoprocessorFunctionCode(Code code)
        {
            FuncCode = code;
        }

        public static implicit operator Code(in CoprocessorFunctionCode code)
        {
            return code.FuncCode;
        }

        public static implicit operator CoprocessorFunctionCode(Code code)
        {
            return new CoprocessorFunctionCode(code);
        }

        public readonly bool Equals(CoprocessorFunctionCode other)
            => FuncCode == other.FuncCode;

        public readonly bool Equals(uint other)
            => (uint)FuncCode == other;

        public readonly override bool Equals(object? obj)
        {
            return obj is CoprocessorFunctionCode rc && Equals(rc);
        }

        public readonly override int GetHashCode()
        {
            return FuncCode.GetHashCode();
        }

        public static bool operator ==(in CoprocessorFunctionCode left, in CoprocessorFunctionCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in CoprocessorFunctionCode left, in CoprocessorFunctionCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(in CoprocessorFunctionCode left, Code right)
        {
            return left.FuncCode == right;
        }

        public static bool operator !=(in CoprocessorFunctionCode left, Code right)
        {
            return !(left == right);
        }

        public static bool operator ==(in CoprocessorFunctionCode left, uint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in CoprocessorFunctionCode left, uint right)
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
