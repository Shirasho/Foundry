using System;

namespace Foundry.Media.Nintendo64.OotDecompiler.Disassembly
{
    internal partial class Disassembler
    {
        public readonly struct RomVariable : IEquatable<RomVariable>, IEquatable<uint>
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
}
