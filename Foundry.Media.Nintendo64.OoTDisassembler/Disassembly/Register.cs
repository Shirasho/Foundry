using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Foundry.Reflection;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    /// <summary>
    /// MIPS registers.
    /// </summary>
    [SuppressMessage("Style", "IDE1006:Naming Styles")]
    public sealed class Register : IEquatable<Register>
    {
        public static Register zero { get; } = new Register("$zero", 0, "Read-only register that always equals 0");
        public static Register at { get; } = new Register("$at", 1, "Reserved as assembler temporary register");
        public static Register v0 { get; } = new Register("$v0", 2, "Value returned by subroutine");
        public static Register v1 { get; } = new Register("$v1", 3, "Value returned by subroutine");
        public static Register a0 { get; } = new Register("$a0", 4, "Argument to a subroutine");
        public static Register a1 { get; } = new Register("$a1", 5, "Argument to a subroutine");
        public static Register a2 { get; } = new Register("$a2", 6, "Argument to a subroutine");
        public static Register a3 { get; } = new Register("$a3", 7, "Argument to a subroutine");
        public static Register t0 { get; } = new Register("$t0", 8, "Temporary (not preserved across a function call)");
        public static Register t1 { get; } = new Register("$t1", 9, "Temporary (not preserved across a function call)");
        public static Register t2 { get; } = new Register("$t2", 10, "Temporary (not preserved across a function call)");
        public static Register t3 { get; } = new Register("$t3", 11, "Temporary (not preserved across a function call)");
        public static Register t4 { get; } = new Register("$t4", 12, "Temporary (not preserved across a function call)");
        public static Register t5 { get; } = new Register("$t5", 13, "Temporary (not preserved across a function call)");
        public static Register t6 { get; } = new Register("$t6", 14, "Temporary (not preserved across a function call)");
        public static Register t7 { get; } = new Register("$t7", 15, "Temporary (not preserved across a function call)");
        public static Register s0 { get; } = new Register("$s0", 16, "Saved register (preserved across a function call)");
        public static Register s1 { get; } = new Register("$s1", 17, "Saved register (preserved across a function call)");
        public static Register s2 { get; } = new Register("$s2", 18, "Saved register (preserved across a function call)");
        public static Register s3 { get; } = new Register("$s3", 19, "Saved register (preserved across a function call)");
        public static Register s4 { get; } = new Register("$s4", 20, "Saved register (preserved across a function call)");
        public static Register s5 { get; } = new Register("$s5", 21, "Saved register (preserved across a function call)");
        public static Register s6 { get; } = new Register("$s6", 22, "Saved register (preserved across a function call)");
        public static Register s7 { get; } = new Register("$s7", 23, "Saved register (preserved across a function call)");
        public static Register t8 { get; } = new Register("$t8", 24, "Temporary");
        public static Register t9 { get; } = new Register("$t9", 25, "Temporary");
        public static Register k0 { get; } = new Register("$k0", 26, "Register reserved for Kernel");
        public static Register k1 { get; } = new Register("$k1", 27, "Register reserved for Kernel");
        public static Register gp { get; } = new Register("$gp", 28, "Global pointer");
        public static Register sp { get; } = new Register("$sp", 29, "Stack pointer");
        public static Register fp { get; } = new Register("$fp", 30, "Frame pointer");
        public static Register ra { get; } = new Register("$ra", 31, "Return address");

        /// <summary>
        /// A collection of predefined registers.
        /// </summary>
        public static IImmutableDictionary<uint, Register> Registers { get; }

        /// <summary>
        /// The mnemonic name of the register.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The name of the register.
        /// </summary>
        public string RegisterNumber { get; }

        /// <summary>
        /// A description of the register.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The register index.
        /// </summary>
        public uint Index { get; }

        private Register(string name, uint index, string description)
        {
            Name = name;
            RegisterNumber = $"${index}";
            Index = index;
            Description = description;
        }

        static Register()
        {
            Registers = typeof(Register).GetStatics<Register>().ToImmutableDictionary(f => ((Register)f.GetValue(null)!).Index, f => (Register)f.GetValue(null)!);
        }

        /// <summary>
        /// Attempts to get an existing <see cref="Register"/> using the index
        /// in <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The register index to find the definition for.</param>
        /// <param name="result">The result of the operation.</param>
        public static bool TryGetValue(uint index, [NotNullWhen(true)] out Register? result)
        {
            return Registers.TryGetValue(index, out result);
        }

        /// <summary>
        /// Attempts to get the name of an existing <see cref="Register"/>.
        /// </summary>
        /// <param name="index">The register index to find the name of.</param>
        public static string? GetName(uint index)
        {
            if (TryGetValue(index, out var result))
            {
                return result.Name;
            }

            return null;
        }

        public static implicit operator uint(Register register)
        {
            return register.Index;
        }

        public static implicit operator Register(uint index)
        {
            return TryGetValue(index, out var register)
                ? register
                : throw new InvalidOperationException($"Value '{index}' does not refer to a valid register index.");
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Register? other)
        {
            return other is not null && other.Index == Index;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Register);
        }

        public static bool operator ==(Register? left, Register? right)
        {
            return left is not null && left.Equals(right);
        }

        public static bool operator !=(Register? left, Register? right)
        {
            return !(left == right);
        }

        public static bool operator ==(Register? left, uint right)
        {
            return left is not null && left.Index == right;
        }

        public static bool operator !=(Register? left, uint right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode();
        }
    }
}
