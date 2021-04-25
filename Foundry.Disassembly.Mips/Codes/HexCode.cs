using System;

namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// A wrapper around a <see cref="uint"/> that will print its value out
    /// as a hex value in the format 0x00000000 when <see cref="ToString"/>
    /// is called.
    /// </summary>
    public readonly struct HexCode : IEquatable<HexCode>, IEquatable<uint>
    {
        /// <summary>
        /// The value this instance represents.
        /// </summary>
        public readonly uint Value { get; }

        public HexCode(uint value)
        {
            Value = value;
        }

        public readonly bool Equals(uint other)
            => Value == other;

        public readonly bool Equals(HexCode other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is HexCode h)
            {
                return Equals(h);
            }

            if (obj is uint u)
            {
                return Equals(u);
            }

            return false;
        }

        public readonly override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator uint(in HexCode code)
        {
            return code.Value;
        }

        public static implicit operator HexCode(uint code)
        {
            return new HexCode(code);
        }

        public static bool operator ==(in HexCode left, in HexCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in HexCode left, in HexCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(in HexCode left, uint right)
        {
            return left.Value == right;
        }

        public static bool operator !=(in HexCode left, uint right)
        {
            return !(left == right);
        }

        public readonly override string ToString()
            => ToString(false);

        public readonly string ToString(bool asDecimal)
        {
            return !asDecimal
                ? ToString(8)
                : Value.ToString();
        }

        public readonly string ToString(int hexPlaces)
        {
            return $"0x{Value.ToString($"X{hexPlaces}")}";
        }
    }
}
