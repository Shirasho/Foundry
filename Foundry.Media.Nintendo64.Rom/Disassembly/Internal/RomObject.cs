using System;

namespace Foundry.Media.Nintendo64.Rom.Disassembly.Internal
{
    internal readonly struct RomObject : IEquatable<RomObject>
    {
        public readonly uint Address { get; }

        public readonly string? Name { get; }

        public RomObject(uint address, string? name = null)
        {
            Address = address;
            Name = name;
        }

        public readonly bool Equals(RomObject other)
        {
            return Address == other.Address;
        }
    }
}
