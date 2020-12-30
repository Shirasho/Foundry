using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Foundry.Serialization.Ini
{
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowCannotSkipOnPartialData()
        {
            throw new InvalidOperationException("Cannot skip on partial data.");
        }

        [DoesNotReturn]
        public static void ThrowUnexpectedToken()
        {
            throw new IniReaderException("Unexpected token.");
        }

        [DoesNotReturn]
        public static void ThrowInvalidEndOfIniProperty()
        {
            throw new IniReaderException("Invalid end of INI property.");
        }

        [DoesNotReturn]
        public static void ThrowKeyNotFoundException(string key)
        {
            throw new KeyNotFoundException($"The key {key} does not exist in the collection.");
        }
    }
}
