using System;

namespace Foundry.Media.Nintendo64.OotDecompiler.Disassembly
{
    internal partial class Disassembler
    {
        public readonly struct RomObject : IEquatable<RomObject>, IEquatable<uint>
        {
            /// <summary>
            /// The address of the object.
            /// </summary>
            public readonly uint Address { get; }

            /// <summary>
            /// The object size, if known.
            /// </summary>
            public readonly int? Size { get; }

            /// <summary>
            /// The name of the object, if known.
            /// </summary>
            public readonly string? Name { get; }

            public RomObject(uint address, int? size, string? name)
            {
                Address = address;
                Size = size;
                Name = name;
            }

            public readonly bool Equals(RomObject other)
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
