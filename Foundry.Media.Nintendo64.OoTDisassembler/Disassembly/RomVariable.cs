using System;
using System.Runtime.CompilerServices;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    internal readonly struct RomVariable : IEquatable<RomVariable>, IEquatable<uint>
    {
        /// <summary>
        /// The address of the variable.
        /// </summary>
        public readonly uint Address { get; }

        /// <summary>
        /// The variable size.
        /// </summary>
        public readonly int Size { get; }

        /// <summary>
        /// The name of the variable, if known.
        /// </summary>
        public readonly string? KnownName { get; }

        public RomVariable(uint address, int size, string? knownName)
        {
            Address = address;
            Size = size;
            KnownName = knownName;
        }

        /// <summary>
        /// Gets the name of this variable. If the name is not known,
        /// a name is generated based on the address.
        /// </summary>
        public readonly string GetName()
        {
            return !string.IsNullOrWhiteSpace(KnownName)
                ? KnownName
                : GenerateName(Address);
        }

        /// <summary>
        /// Generates a name for a variable based on <paramref name="addr"/>.
        /// </summary>
        /// <param name="addr">The address of the variable.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateName(uint addr)
        {
            return $"Variable_0x{addr:X}";
        }

        public readonly bool Equals(RomVariable other)
        {
            return other.Address == Address && other.Size == Size;
        }

        public readonly bool Equals(uint other)
        {
            return Address == other;
        }
    }
}
