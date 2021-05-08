using System;

namespace Foundry.Media.Nintendo64.Rom.Disassembly.Internal
{
    internal sealed class RomVariable : IEquatable<RomVariable>
    {
        public uint Address { get; }

        public string Name { get; }

        public RomVariable(uint address, string? name = null)
        {
            Address = address;
            Name = name ?? MakeName(address);
        }

        public static string MakeName(uint code)
        {
            return $"Sym_{code:X8}";
        }

        public bool Equals(RomLabel? other)
        {
            return other is not null && Address == other.Address;
        }

        public bool Equals(RomVariable? other)
        {
            return other is not null && Address == other.Address;
        }
    }
}
