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
    public readonly struct Int64Numeric : INumeric<long>
    {
        public readonly string ToHexadecimalString(long value)
            => value.ToString("X16");

        public readonly string ToDecimalString(long value)
            => value.ToString();

        public readonly long One => 1L;

        public readonly long Zero => 0L;

        public readonly long And(long left, long right)
            => left & right;

        public readonly int BitCount(long value)
            => Number.BitCount(value);

        public readonly long Create(ulong value)
            => (long)value;

        public readonly long Create(long value)
            => value;

        public readonly bool IsInValueRange(ulong value)
            => value <= long.MaxValue;

        public readonly bool IsInValueRange(long value)
            => true;

        public readonly long LeftShift(long value, int amount)
            => value << amount;

        public readonly bool LessThan(long left, long right)
            => left < right;

        public readonly long Not(long value)
            => ~value;

        public readonly long Or(long left, long right)
            => left | right;

        public readonly long Subtract(long left, long right)
            => left - right;

        public readonly bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out long result)
            => long.TryParse(s, style, provider, out result);

        public readonly bool TryParseNative(string? s, out long result)
            => long.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);

        public readonly long Xor(long left, long right)
            => left ^ right;

        public readonly sbyte ToSByte(long value)
            => Convert.ToSByte(value);

        public readonly byte ToByte(long value)
            => Convert.ToByte(value);

        public readonly short ToInt16(long value)
            => Convert.ToInt16(value);

        public readonly ushort ToUInt16(long value)
            => Convert.ToUInt16(value);

        public readonly int ToInt32(long value)
            => Convert.ToInt32(value);

        public readonly uint ToUInt32(long value)
            => Convert.ToUInt32(value);

        public readonly long ToInt64(long value)
            => value;

        public readonly ulong ToUInt64(long value)
            => Convert.ToUInt64(value);
    }
}
