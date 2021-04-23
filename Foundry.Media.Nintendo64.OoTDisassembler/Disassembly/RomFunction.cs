using System;
using System.Runtime.CompilerServices;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    internal readonly struct RomFunction : IEquatable<RomFunction>, IEquatable<uint>
    {
        /// <summary>
        /// The address of the function.
        /// </summary>
        public readonly uint Address { get; }

        /// <summary>
        /// The name of the function.
        /// </summary>
        public readonly string? KnownName { get; }

        public RomFunction(uint address, string? knownName)
        {
            Address = address;
            KnownName = knownName;
        }

        /// <summary>
        /// Gets the name of this function. If the name is not known,
        /// a name is generated based on the address.
        /// </summary>
        public readonly string GetName()
        {
            return !string.IsNullOrWhiteSpace(KnownName)
                ? KnownName
                : GenerateName(Address);
        }

        /// <summary>
        /// Generates a name for a function based on <paramref name="addr"/>.
        /// </summary>
        /// <param name="addr">The address of the function.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateName(uint addr)
        {
            return $"Func_0x{addr:X}";
        }

        public readonly bool Equals(RomFunction other)
        {
            return other.Address == Address;
        }

        public readonly bool Equals(uint other)
        {
            return Address == other;
        }
    }
}
