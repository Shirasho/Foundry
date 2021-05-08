using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Foundry.Reflection;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// Information about a register.
    /// </summary>
    public sealed class Register
    {
        private static readonly IImmutableDictionary<uint, Register> Registers;

        public static Register Zero { get; } = new Register(0, "$zero");
        public static Register At { get; } = new Register(1, "$at");
        public static Register V0 { get; } = new Register(2, "$v0");
        public static Register V1 { get; } = new Register(3, "$v1");
        public static Register A0 { get; } = new Register(4, "$a0");
        public static Register A1 { get; } = new Register(5, "$a1");
        public static Register A2 { get; } = new Register(6, "$a2");
        public static Register A3 { get; } = new Register(7, "$a3");
        public static Register T0 { get; } = new Register(8, "$t0");
        public static Register T1 { get; } = new Register(9, "$t1");
        public static Register T2 { get; } = new Register(10, "$t2");
        public static Register T3 { get; } = new Register(11, "$t3");
        public static Register T4 { get; } = new Register(12, "$t4");
        public static Register T5 { get; } = new Register(13, "$t5");
        public static Register T6 { get; } = new Register(14, "$t6");
        public static Register T7 { get; } = new Register(15, "$t7");
        public static Register S0 { get; } = new Register(16, "$s0");
        public static Register S1 { get; } = new Register(17, "$s1");
        public static Register S2 { get; } = new Register(18, "$s2");
        public static Register S3 { get; } = new Register(19, "$s3");
        public static Register S4 { get; } = new Register(20, "$s4");
        public static Register S5 { get; } = new Register(21, "$s5");
        public static Register S6 { get; } = new Register(22, "$s6");
        public static Register S7 { get; } = new Register(23, "$s7");
        public static Register T8 { get; } = new Register(24, "$t8");
        public static Register T9 { get; } = new Register(25, "$t9");
        public static Register K0 { get; } = new Register(26, "$k0");
        public static Register K1 { get; } = new Register(27, "$k1");
        public static Register Gp { get; } = new Register(28, "$gp");
        public static Register Sp { get; } = new Register(29, "$sp");
        public static Register Fp { get; } = new Register(30, "$fp");
        public static Register Ra { get; } = new Register(31, "$ra");

        /// <summary>
        /// The register index.
        /// </summary>
        public uint Index { get; }

        /// <summary>
        /// The name of the register.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The register number.
        /// </summary>
        /// <remarks>
        /// This is usually <see cref="Index"/> with a $ prepended to it.
        /// </remarks>
        public string Number { get; }

        private Register(uint index, string name)
        {
            Index = index;
            Name = name;
            Number = $"${index}";
        }

        static Register()
        {
            Registers = typeof(Register).GetStatics<Register>().Select(f => f.GetValue<Register>()!).ToImmutableDictionary(r => r.Index, r => r);
        }

        /// <summary>
        /// Attempts to find the register associated with the specified index.
        /// </summary>
        /// <param name="index">The register index.</param>
        /// <param name="result">If successful, will contain the matching register.</param>
        public static bool TryGetRegister(uint index, [NotNullWhen(true)] out Register? result)
        {
            return Registers.TryGetValue(index, out result);
        }

        /// <summary>
        /// Returns the register associated with <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the register to find.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> does not represent a valid register.</exception>
        public static Register GetRegister(uint index)
        {
            if (!TryGetRegister(index, out var register))
            {
                ThrowIndexOutOfRangeException();
            }

            return register;
        }

        [DoesNotReturn]
        private static void ThrowIndexOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException("index", $"Index must be between {Zero.Index} and {Ra.Index}.");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
