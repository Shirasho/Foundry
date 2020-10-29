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
    public readonly struct BoolNumeric : INumeric<bool>
    {
        public readonly bool One => true;

        public readonly bool Zero => false;

        public readonly bool And(bool left, bool right)
            => left && right;

        public readonly int BitCount(bool value)
            => Number.BitCount(Convert.ToByte(value));

        public readonly bool Create(ulong value)
            => value != 0UL;

        public readonly bool Create(long value)
            => value != 0L;

        public readonly bool IsInValueRange(ulong value)
            => value <= byte.MaxValue;

        public readonly bool IsInValueRange(long value)
            => value >= 0L && value <= byte.MaxValue;

        public readonly bool LeftShift(bool value, int amount)
            => !value && amount == 1;

        public readonly bool LessThan(bool left, bool right)
            => !left && right;

        public readonly bool Not(bool value)
            => !value;

        public readonly bool Or(bool left, bool right)
            => left || right;

        public readonly bool Subtract(bool left, bool right)
            => left ^ right;

        public readonly string ToHexadecimalString(bool value)
            => Convert.ToByte(value).ToString("X2");

        public readonly string ToDecimalString(bool value)
            => Convert.ToByte(value).ToString();

        public readonly bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out bool result)
        {
            bool success = byte.TryParse(s, style, provider, out byte resultAsByte);
            result = Convert.ToBoolean(resultAsByte);
            return success;
        }

        public readonly bool TryParseNative(string? s, out bool result)
            => bool.TryParse(s, out result);

        public readonly bool Xor(bool left, bool right)
            => left != right;

        public readonly sbyte ToSByte(bool value)
            => Convert.ToSByte(value);

        public readonly byte ToByte(bool value)
            => Convert.ToByte(value);

        public readonly short ToInt16(bool value)
            => Convert.ToInt16(value);

        public readonly ushort ToUInt16(bool value)
            => Convert.ToUInt16(value);

        public readonly int ToInt32(bool value)
            => Convert.ToInt32(value);

        public readonly uint ToUInt32(bool value)
            => Convert.ToUInt32(value);

        public readonly long ToInt64(bool value)
            => Convert.ToInt64(value);

        public readonly ulong ToUInt64(bool value)
            => Convert.ToUInt64(value);
    }
}
