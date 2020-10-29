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
    public readonly struct UInt16Numeric : INumeric<ushort>
    {
        public readonly string ToHexadecimalString(ushort value)
            => value.ToString("X4");

        public readonly string ToDecimalString(ushort value)
            => value.ToString();

        public readonly ushort One => 1;

        public readonly ushort Zero => 0;

        public readonly ushort And(ushort left, ushort right)
            => (ushort)(left & right);

        public readonly int BitCount(ushort value)
            => Number.BitCount(value);

        public readonly ushort Create(ulong value)
            => (ushort)value;

        public readonly ushort Create(long value)
            => (ushort)value;

        public readonly bool IsInValueRange(ulong value)
            => value <= ushort.MaxValue;

        public readonly bool IsInValueRange(long value)
            => value >= ushort.MinValue && value <= ushort.MaxValue;

        public readonly ushort LeftShift(ushort value, int amount)
            => (ushort)(value << amount);

        public readonly bool LessThan(ushort left, ushort right)
            => left < right;

        public readonly ushort Not(ushort value)
            => (ushort)~value;

        public readonly ushort Or(ushort left, ushort right)
            => (ushort)(left | right);

        public readonly ushort Subtract(ushort left, ushort right)
            => (ushort)(left - right);

        public readonly bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out ushort result)
            => ushort.TryParse(s, style, provider, out result);

        public readonly bool TryParseNative(string? s, out ushort result)
            => ushort.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);

        public readonly ushort Xor(ushort left, ushort right)
            => (ushort)(left ^ right);

        public readonly sbyte ToSByte(ushort value)
            => Convert.ToSByte(value);

        public readonly byte ToByte(ushort value)
            => Convert.ToByte(value);

        public readonly short ToInt16(ushort value)
            => Convert.ToInt16(value);

        public readonly ushort ToUInt16(ushort value)
            => value;

        public readonly int ToInt32(ushort value)
            => value;

        public readonly uint ToUInt32(ushort value)
            => value;

        public readonly long ToInt64(ushort value)
            => value;

        public readonly ulong ToUInt64(ushort value)
            => value;
    }
}
