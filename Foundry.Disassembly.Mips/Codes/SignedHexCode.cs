using System;

namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// A wrapper around a <see cref="int"/> that will print its value out
    /// as a hex value in the format 0x00000000 when <see cref="ToString"/>
    /// is called.
    /// </summary>
    public readonly struct SignedHexCode : IEquatable<SignedHexCode>, IEquatable<int>
    {
        /// <summary>
        /// The value this instance represents.
        /// </summary>
        public readonly int Value { get; }

        public SignedHexCode(int value)
        {
            Value = value;
        }

        public readonly bool Equals(int other)
            => Value == other;

        public readonly bool Equals(SignedHexCode other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object? obj)
        {
            if (obj is HexCode h)
            {
                return Equals(h);
            }

            if (obj is int u)
            {
                return Equals(u);
            }

            return false;
        }

        public readonly override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public static implicit operator int(in SignedHexCode code)
        {
            return code.Value;
        }

        public static implicit operator SignedHexCode(int code)
        {
            return new SignedHexCode(code);
        }

        public static bool operator ==(in SignedHexCode left, in SignedHexCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(in SignedHexCode left, in SignedHexCode right)
        {
            return !(left == right);
        }

        public static bool operator ==(in SignedHexCode left, int right)
        {
            return left.Value == right;
        }

        public static bool operator !=(in SignedHexCode left, int right)
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
