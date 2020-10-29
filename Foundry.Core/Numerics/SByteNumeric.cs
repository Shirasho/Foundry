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
    public readonly struct SByteNumeric : INumeric<sbyte>
    {
        public readonly string ToHexadecimalString(sbyte value)
            => value.ToString("X2");

        public readonly string ToDecimalString(sbyte value)
            => value.ToString();

        public readonly sbyte One => 1;

        public readonly sbyte Zero => 0;

        public readonly sbyte And(sbyte left, sbyte right)
            => (sbyte)(left & right);

        public readonly int BitCount(sbyte value)
            => Number.BitCount(value);

        public readonly sbyte Create(ulong value)
            => (sbyte)value;

        public readonly sbyte Create(long value)
            => (sbyte)value;

        public readonly bool IsInValueRange(ulong value)
            => value <= (ulong)sbyte.MaxValue;

        public readonly bool IsInValueRange(long value)
            => value >= sbyte.MinValue && value <= sbyte.MaxValue;

        public readonly sbyte LeftShift(sbyte value, int amount)
            => (sbyte)(value << amount);

        public readonly bool LessThan(sbyte left, sbyte right)
            => left < right;

        public readonly sbyte Not(sbyte value)
            => (sbyte)~value;

        public readonly sbyte Or(sbyte left, sbyte right)
            => (sbyte)(left | right);

        public readonly sbyte Subtract(sbyte left, sbyte right)
            => (sbyte)(left - right);

        public readonly bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out sbyte result)
            => sbyte.TryParse(s, style, provider, out result);

        public readonly bool TryParseNative(string? s, out sbyte result)
            => sbyte.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);

        public readonly sbyte Xor(sbyte left, sbyte right)
            => (sbyte)(left ^ right);

        public readonly sbyte ToSByte(sbyte value)
            => value;

        public readonly byte ToByte(sbyte value)
            => Convert.ToByte(value);

        public readonly short ToInt16(sbyte value)
            => value;

        public readonly ushort ToUInt16(sbyte value)
            => Convert.ToUInt16(value);

        public readonly int ToInt32(sbyte value)
            => value;

        public readonly uint ToUInt32(sbyte value)
            => Convert.ToUInt32(value);

        public readonly long ToInt64(sbyte value)
            => value;

        public readonly ulong ToUInt64(sbyte value)
            => Convert.ToUInt64(value);
    }
}
