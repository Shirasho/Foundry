using System;

namespace Foundry.Media.Nintendo64.Rom.Disassembly.Internal
{
    internal sealed class RomFunction : IEquatable<RomFunction>
    {
        public uint Address { get; }

        public string Name { get; }

        public RomFunction(uint address, string? name = null)
        {
            Address = address;
            Name = name ?? MakeName(address);
        }

        public static string MakeName(uint address)
        {
            return $"Func_{address:X8}";
        }

        public bool Equals(RomFunction? other)
        {
            return other is not null && Address == other.Address;
        }
    }
}
