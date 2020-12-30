using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Serialization.Ini
{
    public sealed class IniDocument : IDisposable
    {
        private readonly IDictionary<string, IniSection> Sections;

        private readonly ReadOnlyMemory<byte> Data;
        private byte[]? ExtraBytes;

        private IniDocument(ReadOnlyMemory<byte> data, byte[]? extraBytes)
        {
            Data = data;
            ExtraBytes = extraBytes;
            Sections = new Dictionary<string, IniSection>();
        }

        public void Dispose()
        {
            int length = Data.Length;
            if (length == 0)
            {
                return;
            }


            byte[]? extraBytes = Interlocked.Exchange(ref ExtraBytes, null);
            if (extraBytes is not null)
            {
                extraBytes.AsSpan(0, length).Clear();
                ArrayPool<byte>.Shared.Return(extraBytes);
            }
        }

        public static Task<IniDocument> ParseAsync(string file, CancellationToken cancellationToken)
            => ParseAsync(file, default, cancellationToken);

        public static Task<IniDocument> ParseAsync(FileInfo file, CancellationToken cancellationToken)
            => ParseAsync(file, default, cancellationToken);

        public static Task<IniDocument> ParseAsync(string file, IniReaderOptions options, CancellationToken cancellationToken)
        {
            Guard.IsNotNullOrWhitespace(file, nameof(file));

            return ParseAsync(new FileInfo(file), options, cancellationToken);
        }

        public static async Task<IniDocument> ParseAsync(FileInfo file, IniReaderOptions options, CancellationToken cancellationToken)
        {
            Guard.IsNotNull(file, nameof(file));

            await using var fs = file.OpenRead();
            return await ParseAsync(fs, options, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<IniDocument> ParseAsync(Stream stream, IniReaderOptions options, CancellationToken cancellationToken)
        {
            Guard.IsNotNull(stream, nameof(stream));

            var segments = await ReadToEndAsync(stream, cancellationToken).ConfigureAwait(false);
            try
            {
                return Parse(segments.AsMemory(), options, segments.Array, cancellationToken);
            }
            catch
            {
                segments.AsSpan().Clear();
                ArrayPool<byte>.Shared.Return(segments.Array!);
                throw;
            }
        }

        private static IniDocument Parse(in ReadOnlyMemory<byte> data, IniReaderOptions options, byte[]? extraRentedBytes, CancellationToken cancellationToken)
        {
            var reader = new IniReader(data.Span, options);

            var document = new IniDocument(data, extraRentedBytes);
            var section = new IniSection(string.Empty);
            var key = ReadOnlySpan<byte>.Empty;

            int lastLine = -1;
            var lastTokenType = EIniValueKind.Unknown;
            var lastComment = ReadOnlySpan<byte>.Empty;
            EIniValueKind tokenType;

            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();

                tokenType = reader.TokenType;

                if (tokenType == EIniValueKind.Section)
                {
                    if (lastComment.Length != 0)
                    {
                        // If we still have a lingering comment that has not been associated with a
                        // property or section, add it to the section.
                        section.AddComment(GetString(lastComment));
                        lastComment = ReadOnlySpan<byte>.Empty;
                    }

                    document.AddSection(section);
                    section = new IniSection(GetString(reader.ValueSpan));
                    goto UpdateLocals;
                }

                if (tokenType == EIniValueKind.Key)
                {
                    key = reader.ValueSpan;
                    goto UpdateLocals;
                }

                if (tokenType == EIniValueKind.Value)
                {
                    if (key.Length == 0)
                    {
                        throw new IniReaderException($"Missing key on line {reader.LineNumber}.");
                    }

                    section.SetValue(GetString(key), GetString(reader.ValueSpan));
                    if (lastComment.Length != 0)
                    {
                        section.AddCommentToLatestProperty(GetString(lastComment));
                        lastComment = ReadOnlySpan<byte>.Empty;
                    }
                    key = ReadOnlySpan<byte>.Empty;
                    goto UpdateLocals;
                }

                if (options.CommentHandling == EIniCommentHandling.Allow && tokenType == EIniValueKind.Comment)
                {
                    if (reader.LineNumber == lastLine)
                    {
                        if (lastTokenType == EIniValueKind.Comment)
                        {
                            // Parser error. We messed up our logic. Comments tokens cannot be detected on the same line.
                            throw new IniReaderException($"Invalid parse on line {reader.LineNumber}. Comments should not contain comment tokens on the same line.");
                        }
                        if (lastTokenType == EIniValueKind.Key)
                        {
                            // Parser error. We messed up our logic.
                            throw new IniReaderException($"Invalid parse on line {reader.LineNumber}. Comment should not be the next token after {EIniValueKind.Key}.");
                        }
                        if (lastTokenType == EIniValueKind.Value)
                        {
                            // This comment belongs to the previous property.
                            section.AddCommentToLatestProperty(GetString(reader.ValueSpan));
                        }
                        if (lastTokenType == EIniValueKind.Section)
                        {
                            section.AddComment(GetString(reader.ValueSpan));
                        }
                    }
                    else
                    {
                        // We need to assume this belongs to the next property.
                        lastComment = reader.ValueSpan;
                    }
                    goto UpdateLocals;
                }

                if (tokenType == EIniValueKind.Unknown)
                {
                    throw new IniReaderException($"Encountered unknown token type on line {reader.LineNumber}.");
                }

                UpdateLocals:
                lastLine = reader.LineNumber;
                if (tokenType != EIniValueKind.Comment || options.CommentHandling == EIniCommentHandling.Allow)
                {
                    lastTokenType = tokenType;
                }
            }

            document.AddSection(section);

            return document;
        }

        private static async ValueTask<ArraySegment<byte>> ReadToEndAsync(Stream stream, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            int written = 0;
            byte[]? rented = null;

            ReadOnlyMemory<byte> bom = new byte[] { 0xEF, 0xBB, 0xBF };

            try
            {
                if (stream.CanSeek)
                {
                    long expectedLength = Math.Max(bom.Length, stream.Length - stream.Position) + 1;
                    rented = ArrayPool<byte>.Shared.Rent(checked((int)expectedLength));
                }
                else
                {
                    rented = ArrayPool<byte>.Shared.Rent(4096);
                }

                int lastRead;

                do
                {
                    lastRead = await stream.ReadAsync(rented.AsMemory(written, bom.Length - written), cancellationToken).ConfigureAwait(false);
                    written += lastRead;
                }
                while (lastRead > 0 && written < bom.Length);

                if (written == bom.Length && bom.Span.SequenceEqual(rented.AsSpan(0, 3)))
                {
                    written = 0;
                }

                do
                {
                    if (rented.Length == written)
                    {
                        byte[] toReturn = rented;
                        rented = ArrayPool<byte>.Shared.Rent(toReturn.Length * 2);
                        Buffer.BlockCopy(toReturn, 0, rented, 0, toReturn.Length);
                        ArrayPool<byte>.Shared.Return(toReturn, true);
                    }

                    lastRead = await stream.ReadAsync(rented.AsMemory(written), cancellationToken).ConfigureAwait(false);
                    written += lastRead;
                }
                while (lastRead > 0);

                return new ArraySegment<byte>(rented, 0, written);
            }
            catch
            {
                if (rented is not null)
                {
                    rented.AsSpan(0, written).Clear();
                    ArrayPool<byte>.Shared.Return(rented);
                }

                throw;
            }
        }

        /// <summary>
        /// Adds a section to the document.
        /// </summary>
        /// <param name="section">The section to add.</param>
        public void AddSection(IniSection section)
            => AddSection(section, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Adds a section to the document.
        /// </summary>
        /// <param name="section">The section to add.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        public void AddSection(IniSection section, StringComparison keyComparison)
        {
            Guard.IsNotNull(section, nameof(section));
            GuardEx.IsValid(keyComparison, nameof(keyComparison));

            if (Sections.TryGetValue(section.Name, out var existingSection, keyComparison.ToStringComparer()))
            {
                existingSection!.Merge(section);
            }
            else
            {
                Sections.Add(section.Name, section);
            }
        }

        /// <summary>
        /// Removes a section from the document.
        /// </summary>
        /// <param name="name">The name of the section to remove.</param>
        public void RemoveSection(string name)
            => RemoveSection(name, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Removes a section from the document.
        /// </summary>
        /// <param name="name">The name of the section to remove.</param>
        /// <param name="keyComparison">The comparison to use on the key.</param>
        public void RemoveSection(string name, StringComparison keyComparison)
        {
            Guard.IsNotNull(name, nameof(name));

            if (keyComparison == StringComparison.Ordinal)
            {
                Sections.Remove(name);
                return;
            }

            RemoveSectionCore(name, keyComparison);
        }

        private void RemoveSectionCore(string name, StringComparison keyComparison)
        {
            string? matchingName = null;

            foreach (var section in Sections)
            {
                if (string.Equals(section.Key, name, keyComparison))
                {
                    matchingName = section.Key;
                    break;
                }
            }

            if (matchingName is not null)
            {
                Sections.Remove(matchingName);
            }
        }

        private static string GetString(ReadOnlySpan<byte> span)
        {
            if (span.IsEmpty)
            {
                return string.Empty;
            }

            int maxLength = Encoding.UTF8.GetMaxCharCount(span.Length);

            bool rented = maxLength > 2048;

            char[]? charArray = null;
            Span<char> buffer = !rented
                ? stackalloc char[maxLength]
                : (charArray = ArrayPool<char>.Shared.Rent(maxLength));

            int effectiveLength = Encoding.UTF8.GetChars(span, buffer);
            buffer = buffer.Slice(0, effectiveLength);

            string result = new string(buffer);
            if (rented)
            {
                ArrayPool<char>.Shared.Return(charArray!);
            }

            return result;
        }
    }
}
