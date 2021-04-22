using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Media.Nintendo64.OotDecompiler.Disassembly
{
    internal static class FunctionCode
    {
        public const uint sll = 0;
        public const uint srl = 2;
        public const uint sra = 3;
        public const uint sllv = 4;
        public const uint srlv = 6;
        public const uint srav = 7;
        public const uint jr = 8;
        public const uint jalr = 9;
        public const uint movz = 10;
        public const uint movn = 11;
        public const uint syscall = 12;
        public const uint @break = 13;
        public const uint sync = 15;
        public const uint mfhi = 16;
        public const uint mthi = 17;
        public const uint mflo = 18;
        public const uint mtlo = 19;
        public const uint dsllv = 20;
        public const uint dsrlv = 22;
        public const uint dsrav = 23;
        public const uint mult = 24;
        public const uint multu = 25;
        public const uint div = 26;
        public const uint divu = 27;
        public const uint dmult = 28;
        public const uint dmultu = 29;
        public const uint ddiv = 30;
        public const uint ddivu = 31;
        public const uint add = 32;
        public const uint addu = 33;
        public const uint sub = 34;
        public const uint subu = 35;
        public const uint and = 36;
        public const uint or = 37;
        public const uint xor = 38;
        public const uint nor = 39;
        public const uint slt = 42;
        public const uint sltu = 43;
        public const uint dadd = 44;
        public const uint daddu = 45;
        public const uint tge = 48;
        public const uint tgeu = 49;
        public const uint tlt = 50;
        public const uint teq = 52;
        public const uint tne = 54;
        public const uint tltu = 51;
        public const uint dsll = 56;
        public const uint dsra = 59;
        public const uint dsll32 = 60;
        public const uint dsra32 = 63;

        public static IImmutableDictionary<uint, string> FunctionCodes { get; }

        static FunctionCode()
        {
            FunctionCodes = typeof(FunctionCode).GetConstants<uint>().ToImmutableDictionary(f => (uint)f.GetRawConstantValue()!, f => f.Name);
        }

        public static bool TryGetValue(uint functionCode, [NotNullWhen(true)] out string? functionName)
        {
            return FunctionCodes.TryGetValue(functionCode, out functionName);
        }

        public static string? GetName(uint functionCode)
        {
            return FunctionCodes.TryGetValue(functionCode, out string? functionName)
                ? functionName
                : null;
        }
    }
}
