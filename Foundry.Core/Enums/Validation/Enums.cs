using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Foundry
{
    /// <summary>
    /// A faster alternative to <see cref="Enum"/>.
    /// </summary>
    public static class Enums
    {
        /// <summary>
        /// Returns whether <typeparamref name="TEnum"/> is a flag enum (decorated with <see cref="FlagsAttribute" />).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagEnum<TEnum>()
            where TEnum : struct, Enum => Enums<TEnum>.Info.IsFlagEnum;

        /// <summary>
        /// Returns whether this value is part of a flag enum (decorated with <see cref="FlagsAttribute" />).
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFlagEnum<TEnum>(this TEnum _)
            where TEnum : struct, Enum => IsFlagEnum<TEnum>();

        /// <summary>
        /// Gets the number of enum members using the specified selection rules.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members are counted.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMemberCount<TEnum>(EEnumMemberSelection memberSelection = EEnumMemberSelection.All)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetMemberCount(memberSelection);

        /// <summary>
        /// Gets the number of flags in this enum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlagCount<TEnum>()
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetFlagCount();

        /// <summary>
        /// Gets the number of flats set in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to check the flag count of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFlagCount<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetFlagCount(value);

        /// <summary>
        /// Gets the <see cref="EnumMember{TEnum}"/> of the provided enum object.
        /// </summary>
        /// <param name="value">The enum object to get the <see cref="EnumMember{TEnum}"/> of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumMember<TEnum> GetMember<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetMember(value);

        /// <summary>
        /// Attempts to get the <see cref="EnumMember{TEnum}"/> of the enum value represented by
        /// the value stored in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The string representation of the enum value to get the <see cref="EnumMember{TEnum}"/> of.</param>
        /// <param name="ignoreCase">Whether to ignore casing when attempting to parse the value in <paramref name="value"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumMember<TEnum> GetMember<TEnum>(string value, bool ignoreCase = false)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetMember(value, ignoreCase);

        /// <summary>
        /// Attempts to get the <see cref="EnumMember{TEnum}"/> of the enum value represented by
        /// the value stored in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The string representation of the enum value to get the <see cref="EnumMember{TEnum}"/> of.</param>
        /// <param name="ignoreCase">Whether to ignore casing when attempting to parse the value in <paramref name="value"/>.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static EnumMember<TEnum> GetMember<TEnum>(ReadOnlySpan<char> value, bool ignoreCase = false)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetMember(value, ignoreCase);

        /// <summary>
        /// Obtains the <see cref="EnumMember{TEnum}"/> of all enum values.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<EnumMember<TEnum>> GetMembers<TEnum>(EEnumMemberSelection memberSelection = EEnumMemberSelection.All)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetMembers(memberSelection);

        /// <summary>
        /// Obtains the individual <typeparamref name="TEnum"/> flags that make up a composite <typeparamref name="TEnum"/> value.
        /// </summary>
        /// <param name="value">The composite flag enum value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetFlags(value);

        /// <summary>
        /// Obtains the <see cref="EnumMember{TEnum}"/> of the individual <typeparamref name="TEnum"/> flags that make up
        /// a composite <typeparamref name="TEnum"/> value.
        /// </summary>
        /// <param name="value">The composite flag enum value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<EnumMember<TEnum>> GetFlagMembers<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetFlagMembers(value);

        /// <summary>
        /// Gets the member name of this enum value.
        /// </summary>
        /// <param name="value">The enum member to get the name of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetName<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetName(value);

        /// <summary>
        /// Obtains the names of all enum values.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<string> GetNames<TEnum>(EEnumMemberSelection memberSelection = EEnumMemberSelection.All)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetNames(memberSelection);

        /// <summary>
        /// Obtains the underlying type for this enum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetUnderlyingType<TEnum>()
            where TEnum : struct, Enum => Enums<TEnum>.Info.UnderlyingType;

        /// <summary>
        /// Obtains the underlying type for this enum value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetUnderlyingType<TEnum>(this TEnum _)
            where TEnum : struct, Enum => Enums<TEnum>.Info.UnderlyingType;

        /// <summary>
        /// Obtains the underlying value for the enum value.
        /// </summary>
        /// <param name="value">The enum member to get the underlying value of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetUnderlyingValue<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetUnderlyingValue(value);

        /// <summary>
        /// Obtains the values of all enum members defined in this enum.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return the values of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TEnum> GetValues<TEnum>(EEnumMemberSelection memberSelection = EEnumMemberSelection.All)
            where TEnum : struct, Enum => Enums<TEnum>.Info.GetValues(memberSelection);

        /// <summary>
        /// Returns whether this enum value has every possible flag set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAllFlags<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.HasAllFlags(value);

        /// <summary>
        /// Returns whether this enum value has every flag defined in <paramref name="flags"/> set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        /// <remarks>
        /// This is equivalent to ((<paramref name="value"/> & <paramref name="flags"/>) == <paramref name="flags"/>),
        /// but has support for enum types that do not support bit manipulation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAllFlags<TEnum>(this TEnum value, TEnum flags)
            where TEnum : struct, Enum => Enums<TEnum>.Info.HasAllFlags(value, flags);

        /// <summary>
        /// Returns whether this enum value has any flags set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyFlags<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.HasAnyFlags(value);

        /// <summary>
        /// Returns whether this enum value has any flag defined in <paramref name="flags"/> set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        /// <remarks>
        /// This is equivalent to ((<paramref name="value"/> & <paramref name="flags"/>) != 0),
        /// but has support for enum types that do not support bit manipulation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyFlags<TEnum>(this TEnum value, TEnum flags)
            where TEnum : struct, Enum => Enums<TEnum>.Info.HasAnyFlags(value, flags);

        /// <summary>
        /// Returns whether <paramref name="value"/> is defined in the enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefined<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.IsDefined(value);

        /// <summary>
        /// Returns whether <paramref name="value"/> is defined in the enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDefined<TEnum>(object value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.IsDefined(value);

        /// <summary>
        /// Returns whether <paramref name="value"/> is a valid flag combination for this enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidFlagCombination<TEnum>(this TEnum value)
            where TEnum : struct, Enum => Enums<TEnum>.Info.IsValidFlagCombination(value);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"./></exception>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum Parse<TEnum>(string value, bool ignoreCase = false)
            where TEnum : struct, Enum => Enums<TEnum>.Info.Parse(value, ignoreCase);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum Parse<TEnum>(ReadOnlySpan<char> value, bool ignoreCase = false)
            where TEnum : struct, Enum => Enums<TEnum>.Info.Parse(value, ignoreCase);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"./></exception>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum ParseFlags<TEnum>(string value, bool ignoreCase = false, string? delimiter = null)
            where TEnum : struct, Enum => Enums<TEnum>.Info.ParseFlags(value, ignoreCase, delimiter);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum ParseFlags<TEnum>(ReadOnlySpan<char> value, bool ignoreCase = false, string? delimiter = null)
            where TEnum : struct, Enum => Enums<TEnum>.Info.ParseFlags(value, ignoreCase, delimiter);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum ParseFlags<TEnum>(ReadOnlySpan<char> value, ReadOnlySpan<char> delimiter, bool ignoreCase = false)
            where TEnum : struct, Enum => Enums<TEnum>.Info.ParseFlags(value, delimiter, ignoreCase);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse<TEnum>([NotNullWhen(true)] string? value, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParse(value, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse<TEnum>([NotNullWhen(true)] string? value, bool ignoreCase, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParse(value, ignoreCase, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse<TEnum>(ReadOnlySpan<char> value, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParse(value, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse<TEnum>(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParse(value, ignoreCase, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>([NotNullWhen(true)] string? value, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>([NotNullWhen(true)] string? value, bool ignoreCase, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, ignoreCase, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>([NotNullWhen(true)] string? value, string delimiter, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, delimiter, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>([NotNullWhen(true)] string? value, bool ignoreCase, string delimiter, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, ignoreCase, delimiter, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>(ReadOnlySpan<char> value, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, ignoreCase, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>(ReadOnlySpan<char> value, string delimiter, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, delimiter, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>(ReadOnlySpan<char> value, ReadOnlySpan<char> delimiter, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, delimiter, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>(ReadOnlySpan<char> value, bool ignoreCase, string delimiter, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, ignoreCase, delimiter, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParseFlags<TEnum>(ReadOnlySpan<char> value, bool ignoreCase, ReadOnlySpan<char> delimiter, out TEnum result)
            where TEnum : struct, Enum => Enums<TEnum>.Info.TryParseFlags(value, ignoreCase, delimiter, out result);

        /// <summary>
        /// Validates that <paramref name="value"/> is valid for the enum type <typeparamref name="TEnum"/>. If the type
        /// is decorated with <see cref="FlagsAttribute"/>, the value is checked to make sure it represents a valid flag
        /// combination. If validation fails an <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="paramName">The name of the parameter that stores <paramref name="value"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="paramName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a valid value or flag combination of type <typeparamref name="TEnum"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TEnum Validate<TEnum>(this TEnum value, string paramName)
            where TEnum : struct, Enum => Enums<TEnum>.Info.Validate(value, paramName);

        /// <summary>
        /// Validates that <paramref name="value"/> is valid for the enum type <typeparamref name="TEnum"/>. If the type
        /// is decorated with <see cref="FlagsAttribute"/>, the value is checked to make sure it represents a valid flag
        /// combination. If validation fails an <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="paramName">The name of the parameter that stores <paramref name="value"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="paramName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a valid value or flag combination of type <typeparamref name="TEnum"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("value")]
        public static TEnum? Validate<TEnum>(this TEnum? value, string paramName)
            where TEnum : struct, Enum => value.HasValue ? Enums<TEnum>.Info.Validate(value.Value, paramName) : null;

        internal static IEnumInfo<TEnum> CreateEnumInfo<TEnum>()
            where TEnum : struct, Enum
        {
            var type = Enum.GetUnderlyingType(typeof(TEnum));
            var typeCode = Type.GetTypeCode(type);

            return typeCode switch
            {
                TypeCode.SByte => (IEnumInfo<TEnum>)new EnumInfo<TEnum, sbyte, SByteNumeric>(type, typeCode),
                TypeCode.Byte => new EnumInfo<TEnum, byte, ByteNumeric>(type, typeCode),
                TypeCode.Int16 => new EnumInfo<TEnum, short, Int16Numeric>(type, typeCode),
                TypeCode.UInt16 => new EnumInfo<TEnum, ushort, UInt16Numeric>(type, typeCode),
                TypeCode.Int32 => new EnumInfo<TEnum, int, Int32Numeric>(type, typeCode),
                TypeCode.UInt32 => new EnumInfo<TEnum, uint, UInt32Numeric>(type, typeCode),
                TypeCode.Int64 => new EnumInfo<TEnum, long, Int64Numeric>(type, typeCode),
                TypeCode.UInt64 => new EnumInfo<TEnum, ulong, UInt64Numeric>(type, typeCode),
                TypeCode.Boolean => new EnumInfo<TEnum, bool, BoolNumeric>(type, typeCode),
                TypeCode.Char => new EnumInfo<TEnum, char, CharNumeric>(type, typeCode),
                var _ => throw new NotSupportedException($"An enum with the underlying type {type} is not supported.")
            };
        }
    }

    internal static class Enums<TEnum>
        where TEnum : struct, Enum
    {
        public static IEnumInfo<TEnum> Info { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; } = Enums.CreateEnumInfo<TEnum>();
    }
}
