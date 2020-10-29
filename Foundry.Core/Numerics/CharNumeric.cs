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
    public readonly struct CharNumeric : INumeric<char>
    {
        public readonly char One => (char)1;

        public readonly char Zero => (char)0;

        public readonly char And(char left, char right)
            => (char)(left & right);

        public readonly int BitCount(char value)
            => Number.BitCount(value);

        public readonly char Create(ulong value)
            => (char)value;

        public readonly char Create(long value)
            => (char)value;

        public readonly bool IsInValueRange(ulong value)
            => value <= char.MaxValue;

        public readonly bool IsInValueRange(long value)
            => value >= 0L && value <= char.MaxValue;

        public readonly char LeftShift(char value, int amount)
            => (char)(value << amount);

        public readonly bool LessThan(char left, char right)
            => left < right;

        public readonly char Not(char value)
            => (char)~value;

        public readonly char Or(char left, char right)
            => (char)(left | right);

        public readonly char Subtract(char left, char right)
            => (char)(left - right);

        public readonly string ToHexadecimalString(char value)
            => ((ushort)value).ToString("X4");

        public readonly string ToDecimalString(char value)
            => ((ushort)value).ToString();

        public readonly bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out char result)
        {
            bool success = ushort.TryParse(s, style, provider, out ushort resultAsUShort);
            result = (char)resultAsUShort;
            return success;
        }

        public readonly bool TryParseNative(string? s, out char result)
            => char.TryParse(s, out result);

        public readonly char Xor(char left, char right)
            => (char)(left ^ right);

        public readonly sbyte ToSByte(char value)
            => Convert.ToSByte(value);

        public readonly byte ToByte(char value)
            => Convert.ToByte(value);

        public readonly short ToInt16(char value)
            => Convert.ToInt16(value);

        public readonly ushort ToUInt16(char value)
            => value;

        public readonly int ToInt32(char value)
            => value;

        public readonly uint ToUInt32(char value)
            => value;

        public readonly long ToInt64(char value)
            => value;

        public readonly ulong ToUInt64(char value)
            => value;
    }
}
