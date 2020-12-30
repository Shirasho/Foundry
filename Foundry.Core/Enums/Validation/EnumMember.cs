using System;
using System.Runtime.CompilerServices;
using Foundry.Components;

namespace Foundry
{
    /// <summary>
    /// Information about a <typeparamref name="TEnum"/> member.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    public abstract class EnumMember<TEnum> : FieldDescriptor
        where TEnum : struct, Enum
    {
        /// <summary>
        /// The name of the <typeparamref name="TEnum"/> member.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The value of the <typeparamref name="TEnum"/> member.
        /// </summary>
        public TEnum Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GetValue(ValueField);
        }

        private readonly object ValueField;

        /// <inheritdoc />
        internal EnumMember(string name, object value)
            : base(typeof(TEnum).GetField(name)!, typeof(TEnum))
        {
            Name = name;
            ValueField = value;
        }

        protected abstract TEnum GetValue(object value);
    }

    internal class EnumMember<TEnum, TNumeric, TNumericProvider> : EnumMember<TEnum>
        where TEnum : struct, Enum
        where TNumeric : struct, IComparable<TNumeric>, IEquatable<TNumeric>
        where TNumericProvider : struct, INumeric<TNumeric>
    {
        public Type UnderlyingType { get; } = typeof(TNumeric);
        public TNumericProvider Provider { get; }

        /// <inheritdoc />
        internal EnumMember(string name, object value)
            : base(name, value)
        {
        }

        /// <inheritdoc />
        protected override TEnum GetValue(object value)
        {
            return EnumInfo<TEnum, TNumeric, TNumericProvider>.ToEnum((TNumeric)value);
        }
    }
}
