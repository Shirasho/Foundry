using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    internal interface IEnumInfo<TEnum>
        where TEnum : struct, Enum
    {
        /// <summary>
        /// Obtains the <see cref="System.TypeCode"/> for this enum.
        /// </summary>
        TypeCode TypeCode { get; }

        /// <summary>
        /// Obtains the underlying type for this enum.
        /// </summary>
        Type UnderlyingType { get; }

        /// <summary>
        /// Whether this enum is a flag enum (decorated with <see cref="FlagsAttribute"/>).
        /// </summary>
        bool IsFlagEnum { get; }

        /// <summary>
        /// Gets the number of enum members using the specified selection rules.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members are counted.</param>
        int GetMemberCount(EEnumMemberSelection memberSelection = EEnumMemberSelection.All);

        /// <summary>
        /// Gets the number of flags in this enum.
        /// </summary>
        int GetFlagCount();

        /// <summary>
        /// Gets the number of flats set in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to check the flag count of.</param>
        int GetFlagCount(TEnum value);

        /// <summary>
        /// Gets the <see cref="EnumMember{TEnum}"/> of the provided enum object.
        /// </summary>
        /// <param name="value">The enum object to get the <see cref="EnumMember{TEnum}"/> of.</param>
        EnumMember<TEnum> GetMember(TEnum value);

        /// <summary>
        /// Attempts to get the <see cref="EnumMember{TEnum}"/> of the enum value represented by
        /// the value stored in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The string representation of the enum value to get the <see cref="EnumMember{TEnum}"/> of.</param>
        /// <param name="ignoreCase">Whether to ignore casing when attempting to parse the value in <paramref name="value"/>.</param>
        EnumMember<TEnum> GetMember(string value, bool ignoreCase = false);

        /// <summary>
        /// Attempts to get the <see cref="EnumMember{TEnum}"/> of the enum value represented by
        /// the value stored in <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The string representation of the enum value to get the <see cref="EnumMember{TEnum}"/> of.</param>
        /// <param name="ignoreCase">Whether to ignore casing when attempting to parse the value in <paramref name="value"/>.</param>
        EnumMember<TEnum> GetMember(ReadOnlySpan<char> value, bool ignoreCase = false);

        /// <summary>
        /// Obtains the <see cref="EnumMember{TEnum}"/> of all enum values.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return.</param>
        IEnumerable<EnumMember<TEnum>> GetMembers(EEnumMemberSelection memberSelection = EEnumMemberSelection.All);

        /// <summary>
        /// Obtains the individual <typeparamref name="TEnum"/> flags that make up a composite <typeparamref name="TEnum"/> value.
        /// </summary>
        /// <param name="value">The composite flag enum value.</param>
        IEnumerable<TEnum> GetFlags(TEnum value);

        /// <summary>
        /// Obtains the <see cref="EnumMember{TEnum}"/> of the individual <typeparamref name="TEnum"/> flags that make up
        /// a composite <typeparamref name="TEnum"/> value.
        /// </summary>
        /// <param name="value">The composite flag enum value.</param>
        IEnumerable<EnumMember<TEnum>> GetFlagMembers(TEnum value);

        /// <summary>
        /// Gets the member name of this enum value.
        /// </summary>
        /// <param name="value">The enum member to get the name of.</param>
        string GetName(TEnum value);

        /// <summary>
        /// Obtains the names of all enum values.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return.</param>
        IEnumerable<string> GetNames(EEnumMemberSelection memberSelection = EEnumMemberSelection.All);

        /// <summary>
        /// Obtains the underlying value for this enum value.
        /// </summary>
        /// <param name="value">The enum member to get the underlying value of.</param>
        object GetUnderlyingValue(TEnum value);

        /// <summary>
        /// Obtains the values of all enum members defined in this enum.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return the values of.</param>
        IEnumerable<TEnum> GetValues(EEnumMemberSelection memberSelection = EEnumMemberSelection.All);

        /// <summary>
        /// Returns whether this enum value has every possible flag set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        bool HasAllFlags(TEnum value);

        /// <summary>
        /// Returns whether this enum value has every flag defined in <paramref name="flags"/> set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        /// <remarks>
        /// This is equivalent to ((<paramref name="value"/> & <paramref name="flags"/>) == <paramref name="flags"/>),
        /// but has support for enum types that do not support bit manipulation.
        /// </remarks>
        bool HasAllFlags(TEnum value, TEnum flags);

        /// <summary>
        /// Returns whether this enum value has any flags set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        bool HasAnyFlags(TEnum value);

        /// <summary>
        /// Returns whether this enum value has any flag defined in <paramref name="flags"/> set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        /// <remarks>
        /// This is equivalent to ((<paramref name="value"/> & <paramref name="flags"/>) != 0),
        /// but has support for enum types that do not support bit manipulation.
        /// </remarks>
        bool HasAnyFlags(TEnum value, TEnum flags);

        /// <summary>
        /// Returns whether <paramref name="value"/> is defined in the enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        bool IsDefined(TEnum value);

        /// <summary>
        /// Returns whether <paramref name="value"/> is defined in the enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        bool IsDefined(object? value);

        /// <summary>
        /// Returns whether <paramref name="value"/> is a valid flag combination for this enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        bool IsValidFlagCombination(TEnum value);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"./></exception>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        TEnum Parse(string value, bool ignoreCase = false);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        TEnum Parse(ReadOnlySpan<char> value, bool ignoreCase = false);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"./></exception>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        TEnum ParseFlags(string value, bool ignoreCase = false, string? delimiter = null);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        TEnum ParseFlags(ReadOnlySpan<char> value, bool ignoreCase = false, string? delimiter = null);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        TEnum ParseFlags(ReadOnlySpan<char> value, ReadOnlySpan<char> delimiter, bool ignoreCase = false);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParse([NotNullWhen(true)] string? value, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParse([NotNullWhen(true)] string? value, bool ignoreCase, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParse(ReadOnlySpan<char> value, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags([NotNullWhen(true)] string? value, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags([NotNullWhen(true)] string? value, bool ignoreCase, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags([NotNullWhen(true)] string? value, string? delimiter, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags([NotNullWhen(true)] string? value, bool ignoreCase, string? delimiter, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags(ReadOnlySpan<char> value, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags(ReadOnlySpan<char> value, string? delimiter, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags(ReadOnlySpan<char> value, ReadOnlySpan<char> delimiter, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags(ReadOnlySpan<char> value, bool ignoreCase, string? delimiter, out TEnum result);

        /// <summary>
        /// Attempts to parse <paramref name="value"/> into an enum of type <typeparamref name="TEnum"/>.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        bool TryParseFlags(ReadOnlySpan<char> value, bool ignoreCase, ReadOnlySpan<char> delimiter, out TEnum result);

        /// <summary>
        /// Validates that <paramref name="value"/> is valid for the enum type <typeparamref name="TEnum"/>. If the type
        /// is decorated with <see cref="FlagsAttribute"/>, the value is checked to make sure it represents a valid flag
        /// combination. If validation fails an <see cref="ArgumentOutOfRangeException"/> is thrown.
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="paramName">The name of the parameter that stores <paramref name="value"/>.</param>
        /// <exception cref="ArgumentException"><paramref name="paramName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a valid value or flag combination of type <typeparamref name="TEnum"/>.</exception>
        TEnum Validate(TEnum value, string paramName);
    }

    internal sealed class EnumInfo<TEnum, TNumeric, TNumericProvider> : IEnumInfo<TEnum>
        where TEnum : struct, Enum
        where TNumeric : struct, IComparable<TNumeric>, IEquatable<TNumeric>
        where TNumericProvider : struct, INumeric<TNumeric>
    {
        private static readonly ReadOnlyMemory<char> DefaultDelimiterMemory = new ReadOnlyMemory<char>(new[] { ',' });
        private static readonly Type EnumType = typeof(TEnum);

        /// <summary>
        /// Obtains the <see cref="TypeCode" /> for this enum.
        /// </summary>
        public TypeCode TypeCode { get; }

        /// <summary>
        /// Obtains the underlying type for this enum.
        /// </summary>
        public Type UnderlyingType { get; }

        /// <summary>
        /// Whether this enum is a flag enum (decorated with <see cref="FlagsAttribute" />).
        /// </summary>
        public bool IsFlagEnum { get; }

        private readonly TNumericProvider Provider = new TNumericProvider();

        private readonly Dictionary<TNumeric, EnumMember<TEnum, TNumeric, TNumericProvider>> Cache;
        private readonly List<EnumMember<TEnum, TNumeric, TNumericProvider>> DuplicateMembers;
        private readonly bool IsContiguous;
        private readonly TNumeric AllFlags;
        private readonly TNumeric Min;
        private readonly TNumeric Max;

        public EnumInfo(Type underlyingType, TypeCode typeCode)
        {
            UnderlyingType = underlyingType;
            IsFlagEnum = EnumType.IsDefined(typeof(FlagsAttribute), false);
            TypeCode = typeCode;

            var fields = EnumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            Cache = new Dictionary<TNumeric, EnumMember<TEnum, TNumeric, TNumericProvider>>(fields.Length);
            if (fields.Length == 0)
            {
                DuplicateMembers = new List<EnumMember<TEnum, TNumeric, TNumericProvider>>(0);
                return;
            }

            // This is necessary due to a .NET reflection issue with retrieving Boolean Enums values
            Dictionary<string, TNumeric>? fieldDictionary = null;
            List<EnumMember<TEnum, TNumeric, TNumericProvider>>? duplicateValues = null;

            bool isBoolean = typeof(TNumeric) == typeof(bool);
            if (isBoolean)
            {
                fieldDictionary = new Dictionary<string, TNumeric>();
                var values = (TNumeric[])Enum.GetValues(EnumType);
                string[] names = Enum.GetNames(EnumType);
                for (int i = 0; i < names.Length; ++i)
                {
                    fieldDictionary.Add(names[i], values[i]);
                }
            }

            foreach (var field in fields)
            {
                string name = field.Name;
                var value = isBoolean ? fieldDictionary![name] : (TNumeric)field.GetValue(null)!;
                var member = new EnumMember<TEnum, TNumeric, TNumericProvider>(name, value);
                if (Cache.ContainsKey(value))
                {
                    (duplicateValues ??= new List<EnumMember<TEnum, TNumeric, TNumericProvider>>()).Add(member);
                }
                else
                {
                    Cache.Add(value, member);
                    // Is Power of Two
                    if (Provider.BitCount(value) == 1)
                    {
                        AllFlags = Provider.Or(AllFlags, value);
                    }
                }
            }

            bool isInOrder = true;
            var previous = default(TNumeric);
            bool isFirst = true;
            foreach (var pair in Cache)
            {
                var key = pair.Key;
                if (isFirst)
                {
                    Min = key;
                    isFirst = false;
                }
                else if (previous.CompareTo(key) > 0)
                {
                    isInOrder = false;
                    break;
                }
                previous = key;
            }

            if (isInOrder)
            {
                Max = previous;
            }
            else
            {
                // Makes sure is in increasing value order, due to no removals
                var values = Cache.ToArray();
                Array.Sort(values, static (first, second) => first.Key.CompareTo(second.Key));
                Cache = new Dictionary<TNumeric, EnumMember<TEnum, TNumeric, TNumericProvider>>(Cache.Count);

                foreach (var value in values)
                {
                    Cache.Add(value.Key, value.Value);
                }

                Max = values[^1].Key;
                Min = values[0].Key;
            }

            IsContiguous = Provider.Subtract(Min, Provider.Create(Cache.Count - 1)).Equals(Min);

            if (duplicateValues is not null)
            {
                duplicateValues.TrimExcess();
                // Makes sure is in increasing order
                duplicateValues.Sort(static (first, second) => first.Value.CompareTo(second.Value));
                DuplicateMembers = duplicateValues;
                DuplicateMembers.Capacity = DuplicateMembers.Count;
            }
            else
            {
                DuplicateMembers = new List<EnumMember<TEnum, TNumeric, TNumericProvider>>(0);
            }
        }

        /// <summary>
        /// Converts an underlying <typeparamref name="TNumeric"/> into a <typeparamref name="TEnum"/> value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TEnum ToEnum(TNumeric value)
        {
            return Unsafe.As<TNumeric, TEnum>(ref value);
        }

        /// <summary>
        /// Converts a <typeparamref name="TEnum"/> value into its underlying <typeparamref name="TNumeric"/> value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TNumeric ToNumeric(TEnum value)
        {
            return Unsafe.As<TEnum, TNumeric>(ref value);
        }

        /// <summary>
        /// Attempts to convert an object into a <typeparamref name="TNumeric"/> value.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TNumeric ToNumeric(object value)
        {
            return (TNumeric)value;
        }

        /// <summary>
        /// Gets the number of enum members using the specified selection rules.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members are counted.</param>
        public int GetMemberCount(EEnumMemberSelection memberSelection = EEnumMemberSelection.All)
        {
            return memberSelection switch
            {
                EEnumMemberSelection.Distinct => Cache.Count,
                EEnumMemberSelection.All => Cache.Count + (DuplicateMembers?.Count ?? 0),
                _ => Cache.Count + (DuplicateMembers?.Count ?? 0)
            };
        }

        /// <summary>
        /// Gets the number of flags in this enum.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFlagCount()
            => Provider.BitCount(AllFlags);

        /// <summary>
        /// Gets the number of flats set in <paramref name="value" />.
        /// </summary>
        /// <param name="value">The value to check the flag count of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFlagCount(TEnum value)
            => Provider.BitCount(Provider.And(ToNumeric(value), AllFlags));

        /// <summary>
        /// Obtains the names of all enum values.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<string> GetNames(EEnumMemberSelection memberSelection = EEnumMemberSelection.All)
            => GetMembers(memberSelection).Select(m => m.Name);

        /// <summary>
        /// Gets the <see cref="EnumMember{TEnum}" /> of the provided enum object.
        /// </summary>
        /// <param name="value">The enum object to get the <see cref="EnumMember{TEnum}" /> of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public EnumMember<TEnum> GetMember(TEnum value)
        {
            if (!Cache.TryGetValue(ToNumeric(value), out var member))
            {
                ThrowHelper.ThrowInvalidEnumValue(value, EnumType);
            }

            return member;
        }

        /// <summary>
        /// Attempts to get the <see cref="EnumMember{TEnum}" /> of the enum value represented by
        /// the value stored in <paramref name="value" />.
        /// </summary>
        /// <param name="value">The string representation of the enum value to get the <see cref="EnumMember{TEnum}" /> of.</param>
        /// <param name="ignoreCase">Whether to ignore casing when attempting to parse the value in <paramref name="value" />.</param>
        public EnumMember<TEnum> GetMember(string value, bool ignoreCase = false)
        {
            Guard.IsNotNull(value, nameof(value));

            value = value.Trim();

            var result = Parse(value, ignoreCase);
            return new EnumMember<TEnum, TNumeric, TNumericProvider>(value, result);
        }

        /// <summary>
        /// Attempts to get the <see cref="EnumMember{TEnum}" /> of the enum value represented by
        /// the value stored in <paramref name="value" />.
        /// </summary>
        /// <param name="value">The string representation of the enum value to get the <see cref="EnumMember{TEnum}" /> of.</param>
        /// <param name="ignoreCase">Whether to ignore casing when attempting to parse the value in <paramref name="value" />.</param>
        public EnumMember<TEnum> GetMember(ReadOnlySpan<char> value, bool ignoreCase = false)
        {
            value = value.Trim();

            TryParse(value, ignoreCase, out var result);

            return new EnumMember<TEnum, TNumeric, TNumericProvider>(new string(value), result);
        }

        /// <summary>
        /// Obtains the <see cref="EnumMember{TEnum}" /> of all enum values.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return.</param>
        public IEnumerable<EnumMember<TEnum>> GetMembers(EEnumMemberSelection memberSelection = EEnumMemberSelection.All)
        {
            return memberSelection switch
            {
                EEnumMemberSelection.Distinct => Cache.Values.ToList(),
                EEnumMemberSelection.All => GetAllMembers(),
                var _ => GetAllMembers()
            };
        }

        private IEnumerable<EnumMember<TEnum>> GetAllMembers()
        {
            using var primaryEnumerator = Cache.GetEnumerator();
            bool primaryIsActive = primaryEnumerator.MoveNext();
            var primaryMember = primaryEnumerator.Current.Value;

            using var duplicateEnumerator = DuplicateMembers.GetEnumerator();
            bool duplicateIsActive = duplicateEnumerator.MoveNext();
            var duplicateMember = duplicateEnumerator.Current;

            while (primaryIsActive || duplicateIsActive)
            {
                if (duplicateIsActive && (!primaryIsActive || Provider.LessThan(ToNumeric(duplicateMember.Value), ToNumeric(primaryMember.Value))))
                {
                    yield return duplicateMember;
                    if (duplicateIsActive = duplicateEnumerator.MoveNext())
                    {
                        duplicateMember = duplicateEnumerator.Current;
                    }
                }
                else
                {
                    yield return primaryMember;
                    if (primaryIsActive = primaryEnumerator.MoveNext())
                    {
                        primaryMember = primaryEnumerator.Current.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Obtains the individual <typeparamref name="TEnum" /> flags that make up a composite <typeparamref name="TEnum" /> value.
        /// </summary>
        /// <param name="value">The composite flag enum value.</param>
        public IEnumerable<TEnum> GetFlags(TEnum value)
        {
            var validValue = Provider.And(ToNumeric(value), AllFlags);
            bool isLessThanZero = Provider.LessThan(validValue, Provider.Zero);
            for (var currentValue = Provider.One; (isLessThanZero || !Provider.LessThan(validValue, currentValue)) && !currentValue.Equals(Provider.Zero); currentValue = Provider.LeftShift(currentValue, 1))
            {
                if (HasAnyFlags(ToEnum(validValue), ToEnum(currentValue)))
                {
                    yield return ToEnum(currentValue);
                }
            }
        }

        /// <summary>
        /// Obtains the <see cref="EnumMember{TEnum}" /> of the individual <typeparamref name="TEnum" /> flags that make up
        /// a composite <typeparamref name="TEnum" /> value.
        /// </summary>
        /// <param name="value">The composite flag enum value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<EnumMember<TEnum>> GetFlagMembers(TEnum value)
            => GetFlags(value).Select(GetMember);

        /// <summary>
        /// Gets the member name of this enum value.
        /// </summary>
        /// <param name="value">The enum member to get the name of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetName(TEnum value)
            => GetMember(value).Name;

        /// <summary>
        /// Obtains the underlying value for this enum value.
        /// </summary>
        /// <param name="value">The enum member to get the underlying value of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetUnderlyingValue(TEnum value)
            => ToNumeric(value);

        /// <summary>
        /// Obtains the values of all enum members defined in this enum.
        /// </summary>
        /// <param name="memberSelection">The rules to determine which enum members to return the values of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IEnumerable<TEnum> GetValues(EEnumMemberSelection memberSelection = EEnumMemberSelection.All)
            => GetMembers(memberSelection).Select(member => member.Value);

        /// <summary>
        /// Returns whether this enum value has every possible flag set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllFlags(TEnum value)
            => HasAllFlags(value, ToEnum(AllFlags));

        /// <summary>
        /// Returns whether this enum value has every flag defined in <paramref name="flags"/> set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        /// <remarks>
        /// This is equivalent to ((<paramref name="value"/> & <paramref name="flags"/>) == <paramref name="flags"/>),
        /// but has support for enum types that do not support bit manipulation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAllFlags(TEnum value, TEnum flags)
            => Provider.And(ToNumeric(value), ToNumeric(flags)).Equals(ToNumeric(flags));

        /// <summary>
        /// Returns whether this enum value has any flags set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyFlags(TEnum value)
            => ToNumeric(value).Equals(Provider.Zero);

        /// <summary>
        /// Returns whether this enum value has any flag defined in <paramref name="flags"/> set.
        /// </summary>
        /// <param name="value">The enum value to check the flags of.</param>
        /// <remarks>
        /// This is equivalent to ((<paramref name="value"/> & <paramref name="flags"/>) != 0),
        /// but has support for enum types that do not support bit manipulation.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasAnyFlags(TEnum value, TEnum flags)
            => !Provider.And(ToNumeric(value), ToNumeric(flags)).Equals(Provider.Zero);

        /// <summary>
        /// Returns whether <paramref name="value" /> is defined in the enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDefined(TEnum value)
            => IsContiguous ? !(Provider.LessThan(ToNumeric(value), Min) || Provider.LessThan(Max, ToNumeric(value))) : Cache.ContainsKey(ToNumeric(value));

        /// <summary>
        /// Returns whether <paramref name="value" /> is defined in the enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsDefined(object? value)
            => value is not null && (IsContiguous ? !(Provider.LessThan(ToNumeric(value), Min) || Provider.LessThan(Max, ToNumeric(value))) : Cache.ContainsKey(ToNumeric(value)));

        /// <summary>
        /// Returns whether <paramref name="value" /> is a valid flag combination for this enum.
        /// </summary>
        /// <param name="value">The value to check.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValidFlagCombination(TEnum value)
            => Provider.And(AllFlags, ToNumeric(value)).Equals(ToNumeric(value));

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"./></exception>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        public TEnum Parse(string value, bool ignoreCase = false)
        {
            Guard.IsNotNull(value, nameof(value));

            if (IsFlagEnum)
            {
                return ParseFlags(value, ignoreCase);
            }

            value = value.Trim();

            if (!TryParse(value, ignoreCase, out var result))
            {
                ThrowHelper.ThrowInvalidEnumValue(value, EnumType);
            }

            return result;
        }

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"./></exception>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        public TEnum ParseFlags(string value, bool ignoreCase = false, string? delimiter = null)
        {
            Guard.IsNotNull(value, nameof(value));

            string realDelimiter = delimiter?.Trim()!;
            if (string.IsNullOrEmpty(realDelimiter))
            {
                realDelimiter = ",";
            }

            var result = Provider.Zero;
            int startIndex = 0;
            int valueLength = value.Length;
            while (startIndex < valueLength)
            {
                while (startIndex < valueLength && char.IsWhiteSpace(value[startIndex]))
                {
                    ++startIndex;
                }
                int delimiterIndex = value.IndexOf(realDelimiter, startIndex, StringComparison.Ordinal);
                if (delimiterIndex < 0)
                {
                    delimiterIndex = valueLength;
                }
                int newStartIndex = delimiterIndex + realDelimiter.Length;
                while (delimiterIndex > startIndex && char.IsWhiteSpace(value[delimiterIndex - 1]))
                {
                    --delimiterIndex;
                }
                string currentValue = value[startIndex..delimiterIndex];

                if (!TryParse(currentValue, ignoreCase, out var valueAsTInt))
                {
                    ThrowHelper.ThrowInvalidEnumValue(currentValue, EnumType);
                }

                result = Provider.Or(result, ToNumeric(valueAsTInt));

                startIndex = newStartIndex;
            }
            return ToEnum(result);
        }

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParse([NotNullWhen(true)] string? value, out TEnum result)
            => TryParse(value, false, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        public bool TryParse([NotNullWhen(true)] string? value, bool ignoreCase, out TEnum result)
        {
            var members = new List<EnumMember<TEnum>>(1);
            foreach (var member in GetMembers())
            {
                if (member.Name.Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) ||
                    ToNumeric(member.Value).ToString()!.Equals(value, StringComparison.Ordinal) ||
                    member.Value.ToString().Equals(value, StringComparison.Ordinal))
                {
                    members.Add(member);
                }
            }

            if (members.Count > 1)
            {
                throw new AmbiguousMatchException($"Multiple values on the type {EnumType} match the value {value} (ignoreCase = {ignoreCase}).");
            }

            if (members.Count == 1)
            {
                result = members[0].Value;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParseFlags([NotNullWhen(true)] string? value, out TEnum result)
            => TryParseFlags(value, false, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParseFlags([NotNullWhen(true)] string? value, bool ignoreCase, out TEnum result)
            => TryParseFlags(value, ignoreCase, ",", out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParseFlags([NotNullWhen(true)] string? value, string? delimiter, out TEnum result)
            => TryParseFlags(value, false, delimiter, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException"><paramref name="value"/> matches more than one possible <typeparamref name="TEnum"/> value.</exception>
        public bool TryParseFlags([NotNullWhen(true)] string? value, bool ignoreCase, string? delimiter, out TEnum result)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                result = default;
                return false;
            }

            string realDelimiter = delimiter?.Trim()!;
            if (string.IsNullOrEmpty(realDelimiter))
            {
                realDelimiter = ",";
            }

            var r = Provider.Zero;
            int startIndex = 0;
            int valueLength = value!.Length;
            while (startIndex < valueLength)
            {
                while (startIndex < valueLength && char.IsWhiteSpace(value[startIndex]))
                {
                    ++startIndex;
                }
                int delimiterIndex = value.IndexOf(realDelimiter, startIndex, StringComparison.Ordinal);
                if (delimiterIndex < 0)
                {
                    delimiterIndex = valueLength;
                }
                int newStartIndex = delimiterIndex + realDelimiter.Length;
                while (delimiterIndex > startIndex && char.IsWhiteSpace(value[delimiterIndex - 1]))
                {
                    --delimiterIndex;
                }
                string currentValue = value[startIndex..delimiterIndex];
                if (TryParse(currentValue, ignoreCase, out var valueAsTInt))
                {
                    r = Provider.Or(r, ToNumeric(valueAsTInt));
                }
                else
                {
                    result = default;
                    return false;
                }
                startIndex = newStartIndex;
            }

            result = ToEnum(r);
            return true;
        }

        /// <summary>
        /// Validates that <paramref name="value" /> is valid for the enum type <typeparamref name="TEnum" />. If the type
        /// is decorated with <see cref="Flags" />, the value is checked to make sure it represents a valid flag
        /// combination. If validation fails an <see cref="ArgumentOutOfRangeException" /> is thrown.
        /// </summary>
        /// <param name="value">The value being validated.</param>
        /// <param name="paramName">The name of the parameter that stores <paramref name="value" />.</param>
        /// <exception cref="ArgumentException"><paramref name="paramName"/> is <see langword="null"/>, empty, or whitespace.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value" /> is not a valid value or flag combination of type <typeparamref name="TEnum" />.</exception>
        public TEnum Validate(TEnum value, string paramName)
        {
            Guard.IsNotNullOrWhiteSpace(paramName, nameof(paramName));

            return !IsFlagEnum
                ? IsDefined(value) ? value : throw new ArgumentOutOfRangeException(paramName, EnumErrors<TEnum>.InvalidEnumValue(value))
                : IsValidFlagCombination(value) ? value : throw new ArgumentOutOfRangeException(paramName, EnumErrors<TEnum>.InvalidFlagEnumValue(value));
        }

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        public TEnum Parse(ReadOnlySpan<char> value, bool ignoreCase = false)
        {
            if (IsFlagEnum)
            {
                return ParseFlags(value, ignoreCase);
            }

            var v = value.Trim();

            return TryParse(v, ignoreCase, out var result)
                ? result
                : throw new FormatException($"The value {new string(value)} is not a member of type {EnumType} or is outside the underlying type's range.");
        }

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TEnum ParseFlags(ReadOnlySpan<char> value, bool ignoreCase = false, string? delimiter = null)
            => ParseFlags(value, delimiter is not null ? delimiter.AsSpan() : ReadOnlySpan<char>.Empty);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <exception cref="FormatException"><paramref name="value"/> is not a member of type <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        public TEnum ParseFlags(ReadOnlySpan<char> value, ReadOnlySpan<char> delimiter, bool ignoreCase = false)
        {
            if (delimiter.IsEmpty)
            {
                delimiter = ",";
            }

            var result = Provider.Zero;
            int startIndex = 0;
            int valueLength = value.Length;
            while (startIndex < valueLength)
            {
                while (startIndex < valueLength && char.IsWhiteSpace(value[startIndex]))
                {
                    ++startIndex;
                }
                int delimiterIndex = value[startIndex..].IndexOf(delimiter, StringComparison.Ordinal);
                if (delimiterIndex < 0)
                {
                    delimiterIndex = valueLength;
                }
                int newStartIndex = delimiterIndex + delimiter.Length;
                while (delimiterIndex > startIndex && char.IsWhiteSpace(value[delimiterIndex - 1]))
                {
                    --delimiterIndex;
                }

                var currentValue = value[startIndex..delimiterIndex];
                result = TryParse(currentValue, ignoreCase, out var valueAsTInt)
                    ? Provider.Or(result, ToNumeric(valueAsTInt))
                    : throw new FormatException($"The value {new string(value)} is not a member of type {EnumType} or is outside the underlying type's range.");
                startIndex = newStartIndex;
            }
            return ToEnum(result);
        }

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParse(ReadOnlySpan<char> value, out TEnum result)
            => TryParse(value, false, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        public bool TryParse(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result)
        {
            var members = new List<EnumMember<TEnum>>();

            // Cannot have ref struct in anonymous lambda or local func (LINQ),
            // so we need to do this the old fashion way.
            foreach (var member in GetMembers())
            {
                if (member.Name.AsSpan().Equals(value, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) ||
                    // No known way to optimize this, unfortunately.
                    ToNumeric(member.Value).ToString().AsSpan().Equals(value, StringComparison.Ordinal) ||
                    member.Value.ToString().AsSpan().Equals(value, StringComparison.Ordinal))
                {
                    members.Add(member);
                }
            }
            if (members.Count > 1)
            {
                string valueError = new string(value);
                throw new AmbiguousMatchException($"Multiple values on the type {EnumType} match the value {valueError} (ignoreCase = {ignoreCase}).");
            }

            if (members.Count == 1)
            {
                result = members[0].Value;
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParseFlags(ReadOnlySpan<char> value, out TEnum result)
            => TryParseFlags(value, false, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParseFlags(ReadOnlySpan<char> value, bool ignoreCase, out TEnum result)
            => TryParseFlags(value, ignoreCase, DefaultDelimiterMemory.Span, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParseFlags(ReadOnlySpan<char> value, string? delimiter, out TEnum result)
            => TryParseFlags(value, delimiter.AsSpan(), out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParseFlags(ReadOnlySpan<char> value, ReadOnlySpan<char> delimiter, out TEnum result)
            => TryParseFlags(value, false, delimiter, out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryParseFlags(ReadOnlySpan<char> value, bool ignoreCase, string? delimiter, out TEnum result)
            => TryParseFlags(value, ignoreCase, delimiter is null ? ReadOnlySpan<char>.Empty : delimiter.AsSpan(), out result);

        /// <summary>
        /// Attempts to parse <paramref name="value" /> into an enum of type <typeparamref name="TEnum" />.
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <param name="ignoreCase">Whether to ignore casing when parsing the value.</param>
        /// <param name="delimiter">The delimiter that acts as a separator for the individual flag components.</param>
        /// <param name="result">The result if parsing was successful.</param>
        /// <exception cref="AmbiguousMatchException">Multiple values on the type <typeparamref name="TEnum"/> match <paramref name="value"/>.</exception>
        public bool TryParseFlags(ReadOnlySpan<char> value, bool ignoreCase, ReadOnlySpan<char> delimiter, out TEnum result)
        {
            if (delimiter.IsEmpty)
            {
                delimiter = ",";
            }

            var r = Provider.Zero;
            int startIndex = 0;
            int valueLength = value.Length;
            while (startIndex < valueLength)
            {
                while (startIndex < valueLength && char.IsWhiteSpace(value[startIndex]))
                {
                    ++startIndex;
                }
                int delimiterIndex = value[startIndex..].IndexOf(delimiter, StringComparison.Ordinal);
                if (delimiterIndex < 0)
                {
                    delimiterIndex = valueLength;
                }
                int newStartIndex = delimiterIndex + delimiter.Length;
                while (delimiterIndex > startIndex && char.IsWhiteSpace(value[delimiterIndex - 1]))
                {
                    --delimiterIndex;
                }
                var currentValue = value[startIndex..delimiterIndex];
                if (TryParse(currentValue, ignoreCase, out var valueAsTInt))
                {
                    r = Provider.Or(r, ToNumeric(valueAsTInt));
                }
                else
                {
                    result = default;
                    return false;
                }
                startIndex = newStartIndex;
            }

            result = ToEnum(r);
            return true;
        }
    }
}
