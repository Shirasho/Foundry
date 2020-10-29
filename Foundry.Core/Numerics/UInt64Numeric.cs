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
    public readonly struct UInt64Numeric : INumeric<ulong>
    {
        public readonly string ToHexadecimalString(ulong value)
            => value.ToString("X16");

        public readonly string ToDecimalString(ulong value)
            => value.ToString();

        public readonly ulong One => 1UL;

        public readonly ulong Zero => 0UL;

        public readonly ulong And(ulong left, ulong right)
            => left & right;

        public readonly int BitCount(ulong value)
            => Number.BitCount((long)value);

        public readonly ulong Create(ulong value)
            => value;

        public readonly ulong Create(long value)
            => (ulong)value;

        public readonly bool IsInValueRange(ulong value)
            => true;

        public readonly bool IsInValueRange(long value)
            => value >= 0L;

        public readonly ulong LeftShift(ulong value, int amount)
            => value << amount;

        public readonly bool LessThan(ulong left, ulong right)
            => left < right;

        public readonly ulong Not(ulong value)
            => ~value;

        public readonly ulong Or(ulong left, ulong right)
            => left | right;

        public readonly ulong Subtract(ulong left, ulong right)
            => left - right;

        public readonly bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out ulong result)
            => ulong.TryParse(s, style, provider, out result);

        public readonly bool TryParseNative(string? s, out ulong result)
            => ulong.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);

        public readonly ulong Xor(ulong left, ulong right)
            => left ^ right;

        public readonly sbyte ToSByte(ulong value)
            => Convert.ToSByte(value);

        public readonly byte ToByte(ulong value)
            => Convert.ToByte(value);

        public readonly short ToInt16(ulong value)
            => Convert.ToInt16(value);

        public readonly ushort ToUInt16(ulong value)
            => Convert.ToUInt16(value);

        public readonly int ToInt32(ulong value)
            => Convert.ToInt32(value);

        public readonly uint ToUInt32(ulong value)
            => Convert.ToUInt32(value);

        public readonly long ToInt64(ulong value)
            => Convert.ToInt64(value);

        public readonly ulong ToUInt64(ulong value)
            => value;
    }
}
