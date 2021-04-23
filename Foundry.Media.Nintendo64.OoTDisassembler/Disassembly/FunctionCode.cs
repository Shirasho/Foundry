using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    /// <summary>
    /// MIPS function codes.
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public readonly struct FunctionCode : IEquatable<FunctionCode>
    {
        public static FunctionCode sll { get; } = new FunctionCode("sll", 0);
        public static FunctionCode srl { get; } = new FunctionCode("srl", 2);
        public static FunctionCode sra { get; } = new FunctionCode("sra", 3);
        public static FunctionCode sllv { get; } = new FunctionCode("sllv", 4);
        public static FunctionCode srlv { get; } = new FunctionCode("srlv", 6);
        public static FunctionCode srav { get; } = new FunctionCode("srav", 7);
        public static FunctionCode jr { get; } = new FunctionCode("jr", 8);
        public static FunctionCode jalr { get; } = new FunctionCode("jalr", 9);
        public static FunctionCode movz { get; } = new FunctionCode("movz", 10);
        public static FunctionCode movn { get; } = new FunctionCode("movn", 11);
        public static FunctionCode syscall { get; } = new FunctionCode("syscall", 12);
        public static FunctionCode @break { get; } = new FunctionCode("break", 13);
        public static FunctionCode sync { get; } = new FunctionCode("sync", 15);
        public static FunctionCode mfhi { get; } = new FunctionCode("mfhi", 16);
        public static FunctionCode mthi { get; } = new FunctionCode("mthi", 17);
        public static FunctionCode mflo { get; } = new FunctionCode("mflo", 18);
        public static FunctionCode mtlo { get; } = new FunctionCode("mtlo", 19);
        public static FunctionCode dsllv { get; } = new FunctionCode("dsllv", 20);
        public static FunctionCode dsrlv { get; } = new FunctionCode("dsrlv", 22);
        public static FunctionCode dsrav { get; } = new FunctionCode("dsrav", 23);
        public static FunctionCode mult { get; } = new FunctionCode("mult", 24);
        public static FunctionCode multu { get; } = new FunctionCode("multu", 25);
        public static FunctionCode div { get; } = new FunctionCode("div", 26);
        public static FunctionCode divu { get; } = new FunctionCode("divu", 27);
        public static FunctionCode dmult { get; } = new FunctionCode("dmult", 28);
        public static FunctionCode dmultu { get; } = new FunctionCode("dmultu", 29);
        public static FunctionCode ddiv { get; } = new FunctionCode("ddiv", 30);
        public static FunctionCode ddivu { get; } = new FunctionCode("ddivu", 31);
        public static FunctionCode add { get; } = new FunctionCode("add", 32);
        public static FunctionCode addu { get; } = new FunctionCode("addu", 33);
        public static FunctionCode sub { get; } = new FunctionCode("sub", 34);
        public static FunctionCode subu { get; } = new FunctionCode("subu", 35);
        public static FunctionCode and { get; } = new FunctionCode("and", 36);
        public static FunctionCode or { get; } = new FunctionCode("or", 37);
        public static FunctionCode xor { get; } = new FunctionCode("xor", 38);
        public static FunctionCode nor { get; } = new FunctionCode("nor", 39);
        public static FunctionCode slt { get; } = new FunctionCode("slt", 42);
        public static FunctionCode sltu { get; } = new FunctionCode("sltu", 43);
        public static FunctionCode dadd { get; } = new FunctionCode("dadd", 44);
        public static FunctionCode daddu { get; } = new FunctionCode("daddu", 45);
        public static FunctionCode tge { get; } = new FunctionCode("tge", 48);
        public static FunctionCode tgeu { get; } = new FunctionCode("tgeu", 49);
        public static FunctionCode tlt { get; } = new FunctionCode("tlt", 50);
        public static FunctionCode teq { get; } = new FunctionCode("teq", 52);
        public static FunctionCode tne { get; } = new FunctionCode("tne", 54);
        public static FunctionCode tltu { get; } = new FunctionCode("tltu", 51);
        public static FunctionCode dsll { get; } = new FunctionCode("dsll", 56);
        public static FunctionCode dsra { get; } = new FunctionCode("dsra", 59);
        public static FunctionCode dsll32 { get; } = new FunctionCode("dsll32", 60);
        public static FunctionCode dsra32 { get; } = new FunctionCode("dsra32", 63);

        /// <summary>
        /// A collection of predefined function codes.
        /// </summary>
        public static IImmutableDictionary<uint, FunctionCode> Codes { get; }

        /// <summary>
        /// The name of the function code.
        /// </summary>
        public readonly string Name { get; }

        /// <summary>
        /// The bytes representing this function code.
        /// </summary>
        public readonly uint Code { get; }

        /// <summary>
        /// Whether this function code is valid.
        /// </summary>
        public readonly bool IsValid { get; }

        private FunctionCode(string name, uint code, bool isValid = true)
        {
            Name = name;
            Code = code;
            IsValid = isValid;
        }

        [SuppressMessage("Performance", "HAA0602:Delegate on struct instance caused a boxing allocation")]
        static FunctionCode()
        {
            Codes = typeof(FunctionCode).GetStatics<FunctionCode>().ToImmutableDictionary(f => ((FunctionCode)f.GetValue(null)!).Code, f => (FunctionCode)f.GetValue(null)!);
        }

        /// <summary>
        /// Attempts to get an existing <see cref="FunctionCode"/> using the function code specified
        /// in <paramref name="functionCode"/>.
        /// </summary>
        /// <param name="functionCode">The function code to find the definition for.</param>
        /// <remarks>
        /// If <paramref name="functionCode"/> is not a known function, a <see cref="FunctionCode"/>
        /// with the provided function code and the name "UNKNOWN" is returned.
        /// </remarks>
        public static FunctionCode GetValue(uint functionCode)
        {
            if (Codes.TryGetValue(functionCode, out var result))
            {
                return result;
            }

            return new FunctionCode("UNKNOWN", functionCode, false);
        }

        /// <summary>
        /// Attempts to get an existing <see cref="FunctionCode"/> using the function code specified
        /// in <paramref name="functionCode"/>.
        /// </summary>
        /// <param name="functionCode">The function code to find the definition for.</param>
        /// <param name="result">The result of the operation.</param>
        public static bool TryGetValue(uint functionCode, out FunctionCode result)
        {
            return Codes.TryGetValue(functionCode, out result);
        }

        /// <summary>
        /// Attempts to get the name of an existing <see cref="FunctionCode"/>.
        /// </summary>
        /// <param name="functionCode">The function code to find the name of.</param>
        public static string? GetName(uint functionCode)
        {
            if (TryGetValue(functionCode, out var result))
            {
                return result.Name;
            }

            return null;
        }

        public bool Equals(FunctionCode other)
        {
            return Code == other.Code;
        }

        public override int GetHashCode()
            => Code.GetHashCode();

        public override bool Equals(object? obj)
        {
            return obj is FunctionCode fc && Equals(fc);
        }

        public static bool operator ==(FunctionCode left, FunctionCode right)
        {
            return left.Code == right.Code;
        }

        public static bool operator !=(FunctionCode left, FunctionCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(FunctionCode left, uint right)
        {
            return left.Code == right;
        }

        public static bool operator !=(FunctionCode left, uint right)
        {
            return !(left == right);
        }
    }
}
