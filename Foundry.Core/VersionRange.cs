using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    /// <summary>
    /// Defines a range of <see cref="Version"/>.
    /// </summary>
    public class VersionRange : IEquatable<VersionRange>
    {
        /// <summary>
        /// The lower inclusive bound of this <see cref="VersionRange"/>.
        /// </summary>
        public Version Minimum { get; }

        /// <summary>
        /// The upper inclusive bound of this <see cref="VersionRange"/>.
        /// </summary>
        public Version Maximum { get; }

        public VersionRange(Version minimum)
            : this(minimum, null)
        {

        }

        public VersionRange(Version minimum, Version? maximum)
        {
            Guard.IsNotNull(minimum, nameof(minimum));

            maximum ??= new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

            Guard.IsLessThanOrEqualTo(minimum, maximum, nameof(minimum));

            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// Returns whether this <see cref="VersionRange"/> contains the specified <see cref="Version"/>.
        /// </summary>
        /// <param name="version">The version to check is in the range of this <see cref="VersionRange"/>.</param>
        /// <param name="lowerInclusivity">The lower inclusivity.</param>
        /// <param name="upperInclusivity">The upper inclusivity.</param>
        public bool Contains(Version version, EInclusivity lowerInclusivity = EInclusivity.Inclusive, EInclusivity upperInclusivity = EInclusivity.Inclusive)
        {
            Guard.IsNotNull(version, nameof(version));
            GuardEx.IsValid(lowerInclusivity, nameof(lowerInclusivity));
            GuardEx.IsValid(upperInclusivity, nameof(upperInclusivity));

            return lowerInclusivity switch
            {
                EInclusivity.Exclusive when upperInclusivity == EInclusivity.Exclusive => Minimum < version && version < Maximum,
                EInclusivity.Exclusive when upperInclusivity == EInclusivity.Inclusive => Minimum < version && version <= Maximum,
                EInclusivity.Inclusive when upperInclusivity == EInclusivity.Exclusive => Minimum <= version && version < Maximum,
                _ => Minimum <= version && version <= Maximum
            };
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns><see langword="true" /> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <see langword="false" />.</returns>
        public bool Equals([NotNullWhen(true)] VersionRange? other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Minimum.Equals(other.Minimum) &&
                   Maximum.Equals(other.Maximum);
        }
    }
}
