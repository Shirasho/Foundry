using System;
using System.Runtime.CompilerServices;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    internal readonly struct RomObject : IEquatable<RomObject>, IEquatable<uint>
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
        public readonly string? KnownName { get; }

        public RomObject(uint address, int? size, string? name)
        {
            Address = address;
            Size = size;
            KnownName = name;
        }

        /// <summary>
        /// Gets the name of this object. If the name is not known,
        /// a name is generated based on the address.
        /// </summary>
        public readonly string GetName()
        {
            return !string.IsNullOrWhiteSpace(KnownName)
                ? KnownName
                : GenerateName(Address);
        }

        /// <summary>
        /// Generates a name for a object based on <paramref name="addr"/>.
        /// </summary>
        /// <param name="addr">The address of the object.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GenerateName(uint addr)
        {
            return $"Obj_0x{addr:X}";
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
