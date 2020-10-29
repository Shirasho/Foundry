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
    public interface INumeric<T>
        where T : struct, IComparable<T>, IEquatable<T>
    {
        bool LessThan(T left, T right);

        T And(T left, T right);

        T Or(T left, T right);

        T Xor(T left, T right);

        T Not(T value);

        T LeftShift(T value, int amount);

        T Subtract(T left, T right);

        T Create(long value);

        T Create(ulong value);

        bool IsInValueRange(long value);

        bool IsInValueRange(ulong value);

        bool TryParseNumber(string? s, NumberStyles style, IFormatProvider provider, out T result);

        bool TryParseNative(string? s, out T result);

        string ToHexadecimalString(T value);

        string ToDecimalString(T value);

        int BitCount(T value);

        T Zero { get; }

        T One { get; }

        sbyte ToSByte(T value);

        byte ToByte(T value);

        short ToInt16(T value);

        ushort ToUInt16(T value);

        int ToInt32(T value);

        uint ToUInt32(T value);

        long ToInt64(T value);

        ulong ToUInt64(T value);
    }
}
