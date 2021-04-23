using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    /// <summary>
    /// MIPS branching instructions.
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public sealed class BranchInstruction : IEquatable<BranchInstruction>
    {
        public static BranchInstruction bltz { get; } = new BranchInstruction("bltz", 0, "Branch to target if register value is less than 0.");
        public static BranchInstruction bgez { get; } = new BranchInstruction("bgez", 1, "Branch to target if register value is greater than or equal to 0.");
        public static BranchInstruction bltzl { get; } = new BranchInstruction("bltzl", 2, "");
        public static BranchInstruction bgezl { get; } = new BranchInstruction("bgezl", 3, null);
        public static BranchInstruction tgei { get; } = new BranchInstruction("tgei", 8, null);
        public static BranchInstruction tgeiu { get; } = new BranchInstruction("tgeiu", 9, null);
        public static BranchInstruction tlti { get; } = new BranchInstruction("tlti", 10, null);
        public static BranchInstruction tltiu { get; } = new BranchInstruction("tltiu", 11, null);
        public static BranchInstruction tegi { get; } = new BranchInstruction("tegi", 12, null);
        public static BranchInstruction tnei { get; } = new BranchInstruction("tnei", 14, null);
        public static BranchInstruction bltzal { get; } = new BranchInstruction("bltzal", 16, null);
        public static BranchInstruction bgezal { get; } = new BranchInstruction("bgezal", 17, null);
        public static BranchInstruction bltall { get; } = new BranchInstruction("bltall", 18, null);
        public static BranchInstruction bgczall { get; } = new BranchInstruction("bgczall", 19, null);

        /// <summary>
        /// A collection of predefined instructions.
        /// </summary>
        public static IImmutableDictionary<uint, BranchInstruction> Instructions { get; }

        /// <summary>
        /// The mnemonic name of the instruction.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A description of the instruction.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The instruction value.
        /// </summary>
        public uint Value { get; }

        private BranchInstruction(string name, uint value, string description)
        {
            Name = name;
            Description = description;
            Value = value;
        }

        static BranchInstruction()
        {
            Instructions = typeof(BranchInstruction).GetStatics<BranchInstruction>().ToImmutableDictionary(f => ((BranchInstruction)f.GetValue(null)!).Value, f => (BranchInstruction)f.GetValue(null)!);
        }

        /// <summary>
        /// Attempts to get an existing <see cref="BranchInstruction"/> using the value
        /// in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The branch instruction value to find the definition for.</param>
        /// <param name="result">The result of the operation.</param>
        public static bool TryGetValue(uint value, [NotNullWhen(true)] out BranchInstruction? result)
        {
            return Instructions.TryGetValue(value, out result);
        }

        /// <summary>
        /// Attempts to get the name of an existing <see cref="BranchInstruction"/>.
        /// </summary>
        /// <param name="value">The branch instruction value to find the name of.</param>
        public static string? GetName(uint value)
        {
            if (TryGetValue(value, out var result))
            {
                return result.Name;
            }

            return null;
        }

        public static implicit operator BranchInstruction(uint value)
        {
            return TryGetValue(value, out var branchInstruction)
                ? branchInstruction
                : throw new InvalidOperationException($"Value {value} does not refer to a valid branch instruction.");
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(BranchInstruction? other)
        {
            return other is not null && other.Value == Value;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as BranchInstruction);
        }

        public static bool operator ==(BranchInstruction? left, BranchInstruction? right)
        {
            return left is not null && left.Equals(right);
        }

        public static bool operator !=(BranchInstruction? left, BranchInstruction? right)
        {
            return !(left == right);
        }

        public static bool operator ==(BranchInstruction? left, uint right)
        {
            return left is not null && left.Value == right;
        }

        public static bool operator !=(BranchInstruction? left, uint right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
