using System;

namespace Foundry.Media.Nintendo64.OotDecompiler.Disassembly
{
    internal partial class Disassembler
    {
        public readonly struct RomLabel : IEquatable<RomLabel>, IEquatable<uint>
        {
            /// <summary>
            /// The address of the label.
            /// </summary>
            public readonly uint Address { get; }

            /// <summary>
            /// The name of the label.
            /// </summary>
            public readonly string Name { get; }

            public RomLabel(uint address, string name)
            {
                Address = address;
                Name = name;
            }

            public readonly bool Equals(RomLabel other)
            {
                return other.Address == Address;
            }

            public readonly bool Equals(uint other)
            {
                return Address == other;
            }
        }
    }
}
