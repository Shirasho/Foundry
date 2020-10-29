using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    /// <summary>
    /// Options that indicate how to split a line of text.
    /// </summary>
    public class LineSplitOptions
    {
        /// <summary>
        /// The delimiter to use when attempting to cleanly split words.
        /// </summary>
        public char Delimiter { get; set; }

        /// <summary>
        /// The line split behavior to use when a single word is longer
        /// than the maximum length.
        /// </summary>
        public EWorldSplitOption WordSplitOption { get; set; }

        /// <summary>
        /// Whether to trim <see cref="Delimiter"/> from the beginning of a line portion
        /// before returning the result. This can occur if multiple delimiters appear in
        /// sequence.
        /// </summary>
        public bool TrimOutput { get; set; }

        /// <summary>
        /// Whether to return empty delimiter entries.
        /// </summary>
        public bool ReturnEmptyDelimiterEntries { get; set; }

        public LineSplitOptions()
            : this(EWorldSplitOption.None)
        {
        }

        public LineSplitOptions(char delimiter)
            : this(delimiter, EWorldSplitOption.Split)
        {
        }

        public LineSplitOptions(EWorldSplitOption splitOption)
            : this(splitOption, true, true)
        {
        }

        public LineSplitOptions(char delimiter, EWorldSplitOption splitOption)
            : this(delimiter, splitOption, true, true)
        {
        }

        public LineSplitOptions(bool trimOutput, bool returnEmptyDelimiterEntries)
            : this(EWorldSplitOption.Split, trimOutput, returnEmptyDelimiterEntries)
        {
        }

        public LineSplitOptions(char delimiter, bool trimOutput, bool returnEmptyDelimiterEntries)
            : this(delimiter, EWorldSplitOption.Split, trimOutput, returnEmptyDelimiterEntries)
        {
        }

        public LineSplitOptions(EWorldSplitOption wordSplitOption, bool trimOutput, bool returnEmptyDelimiterEntries)
            : this(' ', wordSplitOption, trimOutput, returnEmptyDelimiterEntries)
        {
        }

        public LineSplitOptions(char delimiter, EWorldSplitOption wordSplitOption, bool trimOutput, bool returnEmptyDelimiterEntries)
        {
            Delimiter = delimiter;
            WordSplitOption = wordSplitOption;
            TrimOutput = trimOutput;
            ReturnEmptyDelimiterEntries = returnEmptyDelimiterEntries;
        }
    }

    /// <summary>
    /// Defines how to split a word when splitting lines of text.
    /// </summary>
    public enum EWorldSplitOption : byte
    {
        /// <summary>
        /// Words that are longer than the maximum length will be left at
        /// the mercy of the caller settings. Depending on the caller this
        /// will usually have the same effect as <see cref="Truncate"/>.
        /// </summary>
        /// <example>
        /// AnExampleOfAVeryLongWord
        /// </example>
        None,

        /// <summary>
        /// Words that are longer than the maximum length will be truncated.
        /// </summary>
        /// <example>
        /// AnExampleOfAVer
        /// </example>
        Truncate,

        /// <summary>
        /// Words that are longer than the maximum length will be split and
        /// placed in the next line group.
        /// </summary>
        /// <example>
        /// AnExampleOfAVer
        /// yLongWord
        /// </example>
        Split
    }

    public static class StringExtensions
    {
        /// <summary>
        /// Returns whether <paramref name="element"/> contains <paramref name="substring"/>
        /// using the <paramref name="comparisonType"/> rules.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <param name="substring">The substring to search for.</param>
        /// <param name="comparisonType">The search comparison rules.</param>
        /// <exception cref="ArgumentNullException"><paramref name="substring"/> is <see langword="null"/></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contains(this string element, string substring, StringComparison comparisonType)
        {
            Guard.IsNotNull(element, nameof(element));
            Guard.IsNotNullOrEmpty(substring, nameof(substring));

            return element.IndexOf(substring, comparisonType) >= 0;
        }

        /// <summary>
        /// Returns whether <paramref name="collection"/> contains <paramref name="value"/>
        /// using the <paramref name="comparisonType"/> rules.
        /// </summary>
        /// <param name="collection">The <see cref="string"/> element.</param>
        /// <param name="value">The value to search for.</param>
        /// <param name="comparisonType">The search comparison rules.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/></exception>
        public static bool Contains(this IEnumerable<string?> collection, string? value, StringComparison comparisonType)
        {
            Guard.IsNotNull(collection, nameof(collection));

            foreach (string? element in collection)
            {
                if (string.Equals(value, element, comparisonType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts this <see cref="string"/> into a byte
        /// array using the specified encoding.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="encoding">The encoding to use. If <see langword="null"/>, UTF8 is used.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] GetBytes(this string? element, Encoding? encoding = null)
        {
            return !string.IsNullOrEmpty(element)
                ? (encoding ?? Encoding.UTF8).GetBytes(element)
                : Array.Empty<byte>();
        }

        /// <summary>
        /// Ensures a string ends with the value specified in <paramref name="value"/>.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <param name="value">The character that <paramref name="element"/> must end with.</param>
        public static string EnsureEndsWith(this string? element, char value)
        {
            if (string.IsNullOrEmpty(element))
            {
                return value.ToString();
            }

            if (element!.EndsWith(value.ToString()))
            {
                return element;
            }

            return element + value;
        }

        /// <summary>
        /// Ensures a string starts with the value specified in <paramref name="value"/>.
        /// </summary>
        /// <param name="element">The element to check.</param>
        /// <param name="value">The character that <paramref name="element"/> must end with.</param>
        public static string EnsureStartsWith(this string? element, char value)
        {
            if (string.IsNullOrEmpty(element))
            {
                return value.ToString();
            }

            if (element!.StartsWith(value.ToString()))
            {
                return element;
            }

            return value + element;
        }

        /// <summary>
        /// Appends all elements in the <see cref="IEnumerable{T}"/> to one another using the specified <paramref name="glue"/>.
        /// </summary>
        /// <param name="elements">The elements to combine.</param>
        /// <param name="glue">The glue.</param>
        /// <returns>The elements glued together, or <see cref="string.Empty"/> if <paramref name="elements"/> is null or empty.</returns>
        /// <remarks>
        /// This is an alias of <see cref="string.Join(char, string[])"/>.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Implode(this IEnumerable<string> elements, string? glue = ",")
        {
            Guard.IsNotNull(elements, nameof(elements));

            return string.Join(glue, elements);
        }

        /// <summary>
        /// Returns whether this <see cref="string"/> contains unicode characters.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <returns><see langword="true"/> if this <see cref="string"/> contains unicode characters, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="EncoderFallbackException">A fallback occurred (see Character Encoding in the .NET Framework for complete explanation)-and-<see cref="P:System.Text.Encoding.EncoderFallback" /> is set to <see cref="T:System.Text.EncoderExceptionFallback" />.</exception>
        public static bool IsUnicode(this string? element)
        {
            if (string.IsNullOrEmpty(element))
            {
                return false;
            }

            int asciiBytesCount = Encoding.ASCII.GetByteCount(element);
            int unicodeBytesCount = Encoding.Unicode.GetByteCount(element);

            return asciiBytesCount != unicodeBytesCount;
        }

        /// <summary>
        /// Masks the entire <see cref="string"/> using the '*' char as the mask.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <returns>The masked <see cref="string"/>.</returns>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("element")]
        public static string? Mask(this string? element)
        {
            return Mask(element, 0);
        }

        /// <summary>
        /// Masks the <see cref="string"/> using the '*' char as the mask.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <param name="numExposed">The number of characters to leave exposed at the end of the <see cref="string"/>.</param>
        /// <returns>The masked string.</returns>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("element")]
        public static string? Mask(this string? element, int numExposed)
        {
            return Mask(element, '*', numExposed);
        }

        /// <summary>
        /// Masks the <see cref="string"/>.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <param name="maskChar">The char to use as the mask.</param>
        /// <param name="numExposed">The number of characters to leave exposed at the end of the <see cref="string"/>.</param>
        /// <returns>The masked <see cref="string"/>.</returns>
        /// <exception cref="OutOfMemoryException">Out of memory.</exception>
        [return: NotNullIfNotNull("element")]
        public static string? Mask(this string? element, char maskChar, int numExposed)
        {
            if (string.IsNullOrEmpty(element))
            {
                return element;
            }

            if (numExposed >= element!.Length)
            {
                return element;
            }

            var builder = new StringBuilder(element.Length);
            builder.Append(maskChar, element.Length - numExposed);
            builder.Append(element.RemoveFirst(element.Length - numExposed));

            return builder.ToString();
        }

        /// <summary>
        /// Pads both sides of the string evenly using the space character so
        /// it reaches the specified total length.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <param name="totalLength">The desired total length of the resulting <see cref="string"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string PadBoth(this string? element, int totalLength)
        {
            return PadBoth(element, totalLength, ' ');
        }

        /// <summary>
        /// Pads both sides of the string evenly using the specified padding character so it
        /// reaches the specified total length.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <param name="totalLength">The desired total length of the resulting <see cref="string"/>.</param>
        /// <param name="paddingChar">The character to use to pad the <see cref="string"/>.</param>
        public static string PadBoth(this string? element, int totalLength, char paddingChar)
        {
            if (string.IsNullOrEmpty(element))
            {
                return new string(paddingChar, totalLength);
            }

            int padding = totalLength - element!.Length;
            int paddingLeft = padding / 2 + element.Length;

            return element.PadLeft(paddingLeft, paddingChar).PadRight(totalLength, paddingChar);
        }

        /// <summary>
        /// Removes the first character from the front of the <see cref="string"/>.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <returns>The <see cref="string"/> element with the first character removed.</returns>
        [return: NotNullIfNotNull("element")]
        public static string? RemoveFirst(this string? element)
        {
            return RemoveFirst(element, 1);
        }

        /// <summary>
        /// Removes the specified number of characters from the front of the <see cref="string"/>.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <param name="count">The number of characters to remove.</param>
        /// <returns>The <see cref="string"/> element with the specified number of characters removed from the front.</returns>
        [return: NotNullIfNotNull("element")]
        public static string? RemoveFirst(this string? element, int count)
        {
            if (string.IsNullOrEmpty(element))
            {
                return element;
            }

            return count > element!.Length ? string.Empty : element.Substring(count);
        }

        /// <summary>
        /// Removes the last character from the front of the <see cref="string"/>.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <returns>The <see cref="string"/> element with the last character removed.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("element")]
        public static string? RemoveLast(this string? element)
        {
            return RemoveLast(element, 1);
        }

        /// <summary>
        /// Removes the specified number of characters from the end of the <see cref="string"/>.
        /// </summary>
        /// <param name="element">The <see cref="string"/> element.</param>
        /// <param name="count">The number of characters to remove.</param>
        /// <returns>The <see cref="string"/> element with the specified number of characters removed from the end.</returns>
        [return: NotNullIfNotNull("element")]
        public static string? RemoveLast(this string? element, int count)
        {
            if (string.IsNullOrEmpty(element))
            {
                return element;
            }

            if (count > element!.Length)
            {
                return string.Empty;
            }

            return element.Substring(0, element.Length - count);
        }

        /// <summary>
        /// Splits the contents of <paramref name="text"/> such that each line is no longer
        /// than <paramref name="maxLength"/> and attempts to maintain the integrity of
        /// words contained within <paramref name="text"/>. If a word within <paramref name="text"/>
        /// is longer than <paramref name="maxLength"/> it will not be split.
        /// </summary>
        /// <param name="text">The value to split.</param>
        /// <param name="maxLength">The soft maximum length of a segment.</param>
        /// <param name="options">The options to use.</param>
        public static IEnumerable<string> SplitLines(this string text, int maxLength, LineSplitOptions? options = null)
        {
            Guard.IsNotNull(text, nameof(text));

            return _(); IEnumerable<string> _()
            {
                if (string.IsNullOrEmpty(text))
                {
                    yield return string.Empty;
                    yield break;
                }

                if (text.Length <= maxLength || maxLength <= 0)
                {
                    yield return text;
                    yield break;
                }

                options ??= new LineSplitOptions();

                int lineStartIndex = 0;
                int lastWhiteSpaceIndex = maxLength - 1;

                do
                {
                    // While we haven't hit our delimiter...
                    while (lastWhiteSpaceIndex > 0 && lastWhiteSpaceIndex < text.Length - 1 && text[lastWhiteSpaceIndex] != options.Delimiter)
                    {
                        // If the word is longer than the max width...
                        if (lastWhiteSpaceIndex <= lineStartIndex)
                        {
                            // Break out.
                            break;
                        }

                        // Last char case - if the last char + 1 is our delimiter we can split there.
                        if (lastWhiteSpaceIndex + 1 < text.Length && text[lastWhiteSpaceIndex + 1] == options.Delimiter)
                        {
                            ++lastWhiteSpaceIndex;
                            break;
                        }

                        --lastWhiteSpaceIndex;
                    }

                    // If our word was longer than the max width...
                    bool brokeWord = false;
                    if (lastWhiteSpaceIndex == lineStartIndex)
                    {
                        switch (options.WordSplitOption)
                        {
                            // If there is no line split option...
                            case EWorldSplitOption.None:
                                // Find the next delimiter breakpoint or the end of the statement
                                // and break there.
                                while (lastWhiteSpaceIndex < text.Length && text[lastWhiteSpaceIndex] != options.Delimiter)
                                {
                                    ++lastWhiteSpaceIndex;
                                }

                                break;
                            case EWorldSplitOption.Truncate:
                                // Go back to the max length.
                                lastWhiteSpaceIndex += maxLength;
                                break;
                            case EWorldSplitOption.Split:
                                // Go back to the max length.
                                lastWhiteSpaceIndex += maxLength;
                                brokeWord = true;
                                break;
                        }
                    }

                    // Somewhere the logic is incorrect and will truncate the last character of the value.
                    // This is a bit of a hack to restore that last character.
                    if (lastWhiteSpaceIndex == text.Length - 1 && text[lastWhiteSpaceIndex] != options.Delimiter)
                    {
                        ++lastWhiteSpaceIndex;
                    }

                    // Weird things can happen if the delimiter is in a specific place.
                    // This removes those edge cases due to a lack of catching those
                    // edge cases in other processing areas.
                    int length = lastWhiteSpaceIndex - lineStartIndex;
                    if (length > text.Length - lineStartIndex)
                    {
                        length = text.Length - lineStartIndex;
                    }

                    string returnValue = text.Substring(lineStartIndex, length);
                    if (options.ReturnEmptyDelimiterEntries || !string.IsNullOrWhiteSpace(returnValue))
                    {
                        if (options.TrimOutput)
                        {
                            yield return returnValue.TrimStart(options.Delimiter);
                        }
                        else
                        {
                            yield return returnValue;
                        }
                    }

                    switch (options.WordSplitOption)
                    {
                        case EWorldSplitOption.None:
                            // No action needed.
                            break;
                        case EWorldSplitOption.Truncate:
                            // Find the next delimiter breakpoint or the end of the statement
                            // and break there.
                            while (lastWhiteSpaceIndex < text.Length && text[lastWhiteSpaceIndex] != options.Delimiter)
                            {
                                ++lastWhiteSpaceIndex;
                            }
                            break;
                        case EWorldSplitOption.Split:
                            if (brokeWord)
                            {
                                lastWhiteSpaceIndex -= 1;
                            }
                            break;
                    }

                    // Add 1 to remove the delimiter.
                    lineStartIndex = lastWhiteSpaceIndex + 1;
                    lastWhiteSpaceIndex = Math.Min(lineStartIndex + maxLength - 1, text.Length - 1);
                }
                while (lineStartIndex < text.Length);
            }
        }

        /// <summary>
        /// Safely obtains the first <paramref name="length"/> elements of
        /// the string.
        /// </summary>
        /// <param name="element">The string to get the substring of.</param>
        /// <param name="length">The maximum length of the string.</param>
        /// <exception cref="InvalidOperationException">The start index when specifying a negative length is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("element")]
        public static string? SubstringSafe(this string? element, int length)
        {
            return SubstringSafe(element, length, null);
        }

        /// <summary>
        /// Safely obtains the first <paramref name="length"/> elements of
        /// the string starting at the <paramref name="startIndex"/> character.
        /// </summary>
        /// <param name="element">The string to get the substring of.</param>
        /// <param name="startIndex">The index to start taking characters from.</param>
        /// <param name="length">The maximum length of the string.</param>
        /// <exception cref="InvalidOperationException">The start index when specifying a negative length is negative.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("element")]
        public static string? SubstringSafe(this string? element, int startIndex, int length)
        {
            return SubstringSafe(element, startIndex, length, null);
        }

        /// <summary>
        /// Safely obtains the first <paramref name="length"/> elements of
        /// the string. If the length of the string is longer than <paramref name="length"/>,
        /// the string will be trimmed and <paramref name="trailingChars"/> will be appended to
        /// the string to indicate it has been truncated. The resulting string will never exceed
        /// <paramref name="length"/>, even with <paramref name="trailingChars"/> appended.
        /// </summary>
        /// <param name="element">The string to get the substring of.</param>
        /// <param name="length">The maximum length of the string.</param>
        /// <param name="trailingChars">The characters to use to indicate the string has been truncated.</param>
        /// <exception cref="InvalidOperationException">The start index when specifying a negative length is negative.</exception>
        [return: NotNullIfNotNull("element")]
        public static string? SubstringSafe(this string? element, int length, string? trailingChars)
        {
            if (string.IsNullOrEmpty(element))
            {
                return element;
            }

            if (length < 0)
            {
                int startIndex = length + element!.Length;
                if (startIndex < 0)
                {
                    throw new InvalidOperationException($"Invalid start index '{length}'");
                }

                return SubstringSafe(element, startIndex, element.Length - startIndex, trailingChars);
            }

            return SubstringSafe(element, 0, length, trailingChars);
        }

        /// <summary>
        /// Safely obtains the first <paramref name="length"/> elements of
        /// the string. If the length of the string is longer than <paramref name="length"/>,
        /// the string will be trimmed and <paramref name="trailingChars"/> will be appended to
        /// the string to indicate it has been truncated. The resulting string will never exceed
        /// <paramref name="length"/>, even with <paramref name="trailingChars"/> appended.
        /// </summary>
        /// <param name="element">The string to get the substring of.</param>
        /// <param name="startIndex">The index to start taking characters from.</param>
        /// <param name="length">The maximum length of the string.</param>
        /// <param name="trailingChars">The characters to use to indicate the string has been truncated.</param>
        /// <exception cref="InvalidOperationException">The start index when specifying a negative length is negative.</exception>
        [return: NotNullIfNotNull("element")]
        public static string? SubstringSafe(this string? element, int startIndex, int length, string? trailingChars)
        {
            if (string.IsNullOrEmpty(element))
            {
                return element;
            }

            length = Math.Abs(length);

            if (startIndex < 0)
            {
                startIndex += element!.Length;
                if (startIndex < 0)
                {
                    throw new InvalidOperationException($"Invalid start index '{startIndex}'");
                }
            }

            int takeLength = Math.Min(startIndex + length, element!.Length - startIndex);

            if (string.IsNullOrEmpty(trailingChars))
            {
                return element.Substring(startIndex, takeLength);
            }

            return element.Substring(startIndex, takeLength) + trailingChars;
        }

        /// <summary>
        /// Returns whether the contents of two <see cref="string"/>s
        /// contain the exact same bits. This method is time-safe.
        /// </summary>
        /// <param name="element">The first element.</param>
        /// <param name="other">The second element.</param>
        public static bool TimeSafeEquals(this string? element, string? other)
        {
            if (string.IsNullOrEmpty(element) && string.IsNullOrEmpty(other))
            {
                return true;
            }

            if (element is null || other is null)
            {
                return false;
            }

            int length1 = element.Length;
            int length2 = other.Length;

            if (length1 != length2)
            {
                return false;
            }

            bool areEqual = true;
            for (int i = 0; i < length1; ++i)
            {
                char c1 = element[i];
                char c2 = other[i];

                areEqual &= c1 == c2;
            }

            return areEqual;
        }

        /// <summary>
        /// Camel-casing refers to a casing practice wherein the first letter of
        /// a word is a lowercase letter, and multiple words are concatenated together
        /// with the first letter of successive words being uppercase.
        /// </summary>
        /// <param name="element">The string to make camel-case.</param>
        /**
         * TODO: Tests
         * "PascalCase",
		 * "Snake_Case",
		 * "_StartsWithUnderscore",
		 * "_sTartsWithUnderscoreAndSecondLetterIsCapitalized",
		 * "Has A Bunch Of Spaces",
		 * "a mix_of spaces_and underscores",
		 * "A senTANce wiTH MIxed LETTER CasinG"
         */
        [return: NotNullIfNotNull("element")]
        public static string? ToCamelCase(this string? element)
        {
            if (string.IsNullOrEmpty(element))
            {
                return element;
            }

            var result = new StringBuilder();

            // TextInfo.ToTitleCase has special checks for dutch - we will ignore these.

            bool nextCharIsCapital = false;
            bool hasLowercasedFirstLetter = false;
            for (int i = 0; i < element!.Length; ++i)
            {
                // The core implementation for this is deep, as you would expect from globalization.
                // We will just do this stupidly based on invariant culture.
                if (nextCharIsCapital)
                {
                    result.Append(char.ToUpperInvariant(element[i]));
                    nextCharIsCapital = false;
                    continue;
                }

                if (element[i] == '_' || char.IsWhiteSpace(element[i]))
                {
                    if (hasLowercasedFirstLetter)
                    {
                        nextCharIsCapital = true;
                    }
                    continue;
                }

                if (!hasLowercasedFirstLetter)
                {
                    result.Append(char.ToLowerInvariant(element[i]));
                    hasLowercasedFirstLetter = true;
                    continue;
                }

                result.Append(element[i]);
            }

            return result.ToString();
        }
    }
}
