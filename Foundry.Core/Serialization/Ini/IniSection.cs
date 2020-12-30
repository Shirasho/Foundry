using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Serialization.Ini
{
    public sealed class IniSection
    {
        private readonly struct IniSectionEntry
        {
            public readonly string Key { get; }

            public readonly IList<string> Values { get; }

            public readonly IList<string> Comments { get; }

            public readonly bool IsComment { get; }

            public IniSectionEntry(string key, IList<string> value, bool isComment)
            {
                Key = key;
                Values = value;
                IsComment = isComment;
                Comments = new List<string>(1);
            }
        }

        /// <summary>
        /// The name of this section.
        /// </summary>
        public string Name { get; }

        private readonly IDictionary<string, IniSectionEntry> Values;

        private string? LatestEntryKey;

        public IniSection(string name)
        {
            Guard.IsNotNull(name, nameof(name));

            Name = name;

            Values = new Dictionary<string, IniSectionEntry>();
            LatestEntryKey = null;
        }

        /// <summary>
        /// Gets the value of the key defined in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the value of.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> does not exist in this section.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetValue(string key, StringComparison keyComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (TryGetValue(key, keyComparison, out string? result))
            {
                return result;
            }

            ThrowHelper.ThrowKeyNotFoundException(key);

            return string.Empty;
        }

        /// <summary>
        /// Gets the value of the key defined in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the value of.</param>
        /// <param name="defaultValue">The value to return if <paramref name="key"/> does not exist in this section.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("defaultValue")]
        public string? GetValue(string key, string? defaultValue, StringComparison keyComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (TryGetValue(key, keyComparison, out string? result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Attempts to get the value of the key defined in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the value of.</param>
        /// <param name="result">
        /// When this method returns, contains the value of <paramref name="key"/> if <see langword="true"/> is
        /// returned, else this value will be set to <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the key exists and contains a valid value, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(string key, [NotNullWhen(true)] out string? result)
            => TryGetValue(key, StringComparison.OrdinalIgnoreCase, out result);

        /// <summary>
        /// Attempts to get the value of the key defined in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the value of.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <param name="result">
        /// When this method returns, contains the value of <paramref name="key"/> if <see langword="true"/> is
        /// returned, else this value will be set to <see langword="null"/>.
        /// </param>
        /// <returns><see langword="true"/> if the key exists and contains a valid value, <see langword="false"/> otherwise.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(string key, StringComparison keyComparison, [NotNullWhen(true)] out string? result)
        {
            result = null;

            if (TryGetCollection(key, keyComparison, out var collection))
            {
                result = collection[0];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a collection of values that share the key defined in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the values of.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException"><paramref name="key"/> does not exist in this section.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IImmutableList<string> GetCollection(string key, StringComparison keyComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (TryGetCollection(key, keyComparison, out var result))
            {
                return result;
            }

            ThrowHelper.ThrowKeyNotFoundException(key);

            return ImmutableArray<string>.Empty;
        }

        /// <summary>
        /// Gets a collection of values that share the key defined in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the values of.</param>
        /// <param name="defaultValue">The value to return if <paramref name="key"/> does not exist in this section.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNullIfNotNull("defaultValue")]
        public IImmutableList<string>? GetCollection(string key, IImmutableList<string>? defaultValue, StringComparison keyComparison = StringComparison.OrdinalIgnoreCase)
        {
            if (TryGetCollection(key, keyComparison, out var result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Attempts to get a collection of values that share the key defined in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the values of.</param>
        /// <param name="result">
        /// When this method returns, contains the value of <paramref name="key"/> if <see langword="true"/> is
        /// returned, else this value will be set to <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetCollection(string key, [NotNullWhen(true)] out IImmutableList<string>? result)
            => TryGetCollection(key, StringComparison.OrdinalIgnoreCase, out result);

        /// <summary>
        /// Attempts to get a collection of values that share the key defined in <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The key to get the values of.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <param name="result">
        /// When this method returns, contains the value of <paramref name="key"/> if <see langword="true"/> is
        /// returned, else this value will be set to <see langword="null"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        public bool TryGetCollection(string key, StringComparison keyComparison, [NotNullWhen(true)] out IImmutableList<string>? result)
        {
            Guard.IsNotNull(key, nameof(key));
            GuardEx.IsValid(keyComparison, nameof(keyComparison));

            result = null;

            if (Values.TryGetValue(key, out var entry, keyComparison.ToStringComparer()))
            {
                result = entry.Values.ToImmutableArray();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the value of <paramref name="key"/> to the value defined in <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The name of the key.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="overwrite">
        /// Whether to overwrite the value of the key if the key already exists. If set to <see langword="false"/>, the value will be output
        /// as a duplicate key.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(string key, string? value, bool overwrite = true)
            => SetValue(key, value, StringComparison.OrdinalIgnoreCase, overwrite);

        /// <summary>
        /// Sets the value of <paramref name="key"/> to the value defined in <paramref name="value"/>.
        /// </summary>
        /// <param name="key">The name of the key.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <param name="overwrite">
        /// Whether to overwrite the value of the key if the key already exists. If set to <see langword="false"/>, the value will be output
        /// as a duplicate key.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        public void SetValue(string key, string? value, StringComparison keyComparison, bool overwrite = true)
        {
            Guard.IsNotNull(key, nameof(key));
            GuardEx.IsValid(keyComparison, nameof(keyComparison));

            value ??= string.Empty;

            if (Values.TryGetValue(key, out var entry, keyComparison.ToStringComparer()))
            {
                if (overwrite)
                {
                    entry.Values.Clear();
                }

                entry.Values.Add(value);
            }
            else
            {
                Values.Add(key, new IniSectionEntry(key, new List<string> { value }, string.Equals(key, IniConstants.DefaultCommentMarkerString, StringComparison.Ordinal)));
                LatestEntryKey = key;
            }
        }

        /// <summary>
        /// Adds a comment to this section.
        /// </summary>
        /// <param name="content">The comment content.</param>
        /// <remarks>
        /// This method is position-sensitive. If invoked after the section
        /// has been full created, the comment will be added to the end of
        /// the section.
        ///
        /// If you need to add a comment to a specific property, use the
        /// <see cref="AddComment(string, string?)"/> or <see cref="AddComment(string, string?, StringComparison)"/>
        /// overloads.
        /// </remarks>
        public void AddComment(string? content)
        {
            content ??= string.Empty;

            Values.Add(IniConstants.DefaultCommentMarkerString, new IniSectionEntry(string.Empty, new List<string> { content }, true));
        }

        /// <summary>
        /// Adds a comment to the property defined in <paramref name="key"/>. If the property
        /// does not exist this method does nothing. If this property has multiple values,
        /// the comment will be added to the first value.
        /// </summary>
        /// <param name="key">The key of the property to add a comment to.</param>
        /// <param name="content">The comment content.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddComment(string key, string? content)
            => AddComment(key, content, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Adds a comment to the property defined in <paramref name="key"/>. If the property
        /// does not exist this method does nothing. If this property has multiple values,
        /// the comment will be added to the first value.
        /// </summary>
        /// <param name="key">The key of the property to add a comment to.</param>
        /// <param name="content">The comment content.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        public void AddComment(string key, string? content, StringComparison keyComparison)
        {
            Guard.IsNotNull(key, nameof(key));
            GuardEx.IsValid(keyComparison, nameof(keyComparison));

            content ??= string.Empty;

            if (Values.TryGetValue(key, out var entry, keyComparison.ToStringComparer()))
            {
                entry.Comments.Add(content);
            }
        }

        internal void AddCommentToLatestProperty(string content)
        {
            if (LatestEntryKey is null)
            {
                // Should not be possible, so we can silently fail here.
                return;
            }

            Values[LatestEntryKey].Comments.Add(content);
        }

        /// <summary>
        /// Merges the values of <paramref name="other"/> into this <see cref="IniSection"/>.
        /// </summary>
        /// <param name="other">The section to merge into this one.</param>
        /// <param name="overwrite">Whether to overwrite existing properties in this section.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        public void Merge(IniSection other, bool overwrite = true)
            => Merge(other, StringComparison.OrdinalIgnoreCase, overwrite);

        /// <summary>
        /// Merges the values of <paramref name="other"/> into this <see cref="IniSection"/>.
        /// </summary>
        /// <param name="other">The section to merge into this one.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        /// <param name="overwrite">Whether to overwrite existing properties in this section.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="keyComparison"/> is not valid.</exception>
        public void Merge(IniSection other, StringComparison keyComparison, bool overwrite = true)
        {
            Guard.IsNotNull(other, nameof(other));
            GuardEx.IsValid(keyComparison, nameof(keyComparison));

            var comparer = keyComparison.ToStringComparer();

            foreach (var otherEntry in other.Values)
            {
                if (otherEntry.Value.IsComment)
                {
                    Values.Add(IniConstants.DefaultCommentMarkerString, new IniSectionEntry(string.Empty, new List<string> { otherEntry.Value.Values[0] }, true));
                    continue;
                }

                if (Values.TryGetValue(otherEntry.Key, out var entry, comparer))
                {
                    if (overwrite)
                    {
                        entry.Comments.Clear();
                        entry.Values.Clear();
                    }

                    entry.Comments.AddRange(otherEntry.Value.Comments);
                    entry.Values.AddRange(otherEntry.Value.Values);
                }
                else
                {
                    Values.Add(otherEntry);
                }
            }
        }
    }
}
