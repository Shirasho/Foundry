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
    public readonly struct UInt32Numeric : INumeric<uint>
    {
        public readonly string ToHexadecimalString(uint value)
            => value.ToString("X8");

        public readonly string ToDecimalString(uint value)
            => value.ToString();

        public readonly uint One => 1U;

        public readonly uint Zero => 0U;

        public readonly uint And(uint left, uint right)
            => left & right;

        public readonly int BitCount(uint value)
            => Number.BitCount((int)value);

        public readonly uint Create(ulong value)
            => (uint)value;

        public readonly uint Create(long value)
            => (uint)value;

        public readonly bool IsInValueRange(ulong value)
            => value <= uint.MaxValue;

        public readonly bool IsInValueRange(long value)
            => value >= uint.MinValue && value <= uint.MaxValue;

        public readonly uint LeftShift(uint value, int amount)
            => value << amount;

        public readonly bool LessThan(uint left, uint right)
            => left < right;

        public readonly uint Not(uint value)
            => ~value;

        public readonly uint Or(uint left, uint right)
            => left | right;

        public readonly uint Subtract(uint left, uint right)
            => left - right;

        public readonly bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out uint result)
            => uint.TryParse(s, style, provider, out result);

        public readonly bool TryParseNative(string? s, out uint result)
            => uint.TryParse(s, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);

        public readonly uint Xor(uint left, uint right)
            => left ^ right;

        public readonly sbyte ToSByte(uint value)
            => Convert.ToSByte(value);

        public readonly byte ToByte(uint value)
            => Convert.ToByte(value);

        public readonly short ToInt16(uint value)
            => Convert.ToInt16(value);

        public readonly ushort ToUInt16(uint value)
            => Convert.ToUInt16(value);

        public readonly int ToInt32(uint value)
            => Convert.ToInt32(value);

        public readonly uint ToUInt32(uint value)
            => value;

        public readonly long ToInt64(uint value)
            => value;

        public readonly ulong ToUInt64(uint value)
            => value;
    }
}
