using System;

namespace Foundry.Media.Nintendo64.Rom.Disassembly.Internal
{
    internal sealed class RomLabel : IEquatable<RomLabel>
    {
        public uint Address { get; }

        public string Name { get; }

        public RomLabel(uint address, string? name = null)
        {
            Address = address;
            Name = name ?? MakeName(address);
        }

        public static string MakeName(uint code)
        {
            return $".L_{code:X8}";
        }

        public bool Equals(RomLabel? other)
        {
            return other is not null && Address == other.Address;
        }
    }
}
