// Copyright (c) 2016 Tyler Brinkley
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;

namespace Foundry
{
    public readonly struct ByteNumeric : INumeric<byte>
    {
        public readonly string ToHexadecimalString(byte value)
            => value.ToString("X2");

        public readonly string ToDecimalString(byte value)
            => value.ToString();

        public readonly byte One => 1;

        public readonly byte Zero => 0;

        public readonly byte And(byte left, byte right)
            => (byte)(left & right);

        public readonly int BitCount(byte value)
            => Number.BitCount(value);

        public readonly byte Create(ulong value)
            => (byte)value;

        public readonly byte Create(long value)
            => (byte)value;

        public readonly bool IsInValueRange(ulong value)
            => value <= byte.MaxValue;

        public readonly bool IsInValueRange(long value)
            => value >= byte.MinValue && value <= byte.MaxValue;

        public readonly byte LeftShift(byte value, int amount)
            => (byte)(value << amount);

        public readonly bool LessThan(byte left, byte right)
            => left < right;

        public readonly byte Not(byte value)
            => (byte)~value;

        public readonly byte Or(byte left, byte right)
            => (byte)(left | right);

        public readonly byte Subtract(byte left, byte right)
            => (byte)(left - right);

        public readonly bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out byte result)
            => byte.TryParse(s, style, provider, out result);

        public readonly bool TryParseNative(string? s, out byte result)
            => byte.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);

        public readonly byte Xor(byte left, byte right)
            => (byte)(left ^ right);

        public readonly sbyte ToSByte(byte value)
            => Convert.ToSByte(value);

        public readonly byte ToByte(byte value)
            => value;

        public readonly short ToInt16(byte value)
            => value;

        public readonly ushort ToUInt16(byte value)
            => value;

        public readonly int ToInt32(byte value)
            => value;

        public readonly uint ToUInt32(byte value)
            => value;

        public readonly long ToInt64(byte value)
            => value;

        public readonly ulong ToUInt64(byte value)
            => value;
    }
}
