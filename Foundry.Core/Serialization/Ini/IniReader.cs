using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Foundry.Serialization.Ini
{
    internal ref struct IniReader
    {
        /// <summary>
        /// The current INI token type.
        /// </summary>
        public EIniValueKind TokenType { get; private set; }

        /// <summary>
        /// The index that the last processed INI token starts at, skipping any whitespace.
        /// </summary>
        /// <remarks>
        /// For sections this points to the index before the opening '['.
        /// </remarks>
        public long TokenStartIndex { get; private set; }

        /// <summary>
        /// The value of the current token.
        /// </summary>
        public ReadOnlySpan<byte> ValueSpan { get; private set; }

        /// <summary>
        /// The current line number.
        /// </summary>
        public int LineNumber { get; private set; }

        private bool IsLastSpan => IsFinalBlock && IsLastSegment;

        private readonly IniReaderOptions ReaderOptions;
        private readonly ReadOnlySpan<byte> Data;
        private readonly bool IsFinalBlock;
        private readonly bool IsLastSegment;

        private int Consumed;
        private byte[]? UnescapedValueBuffer;

        public IniReader(ReadOnlySpan<byte> data, IniReaderOptions readerOptions)
            : this(data, true, readerOptions)
        {

        }

        public IniReader(ReadOnlySpan<byte> data, bool isFinalBlock, IniReaderOptions readerOptions)
        {
            Data = data;
            TokenType = EIniValueKind.Unknown;
            IsFinalBlock = isFinalBlock;
            IsLastSegment = IsFinalBlock;

            ReaderOptions = readerOptions;
            ValueSpan = ReadOnlySpan<byte>.Empty;

            Consumed = 0;
            TokenStartIndex = 0;
            LineNumber = 0;
            UnescapedValueBuffer = null;
        }

        /// <summary>
        /// Skips the next property or section.
        /// </summary>
        /// <remarks>
        /// If the reader is on a <see cref="EIniValueKind.Key"/>, the reader moves to the property value.
        /// If the reader is on a <see cref="EIniValueKind.Section"/>, the reader moves to the last property value in the current section.
        ///
        /// For all other token types, the reader does not move. The next call to <see cref="Read"/> will cause
        /// the reader to be at the next appropriate reader value.
        /// </remarks>
        public void Skip()
        {
            if (!IsFinalBlock)
            {
                ThrowHelper.ThrowCannotSkipOnPartialData();
            }

            if (TokenType == EIniValueKind.Key)
            {
                Read();
            }
            else if (TokenType == EIniValueKind.Section)
            {
                var restore = this;

                if (Read())
                {
                    if (TokenType == EIniValueKind.Section)
                    {
                        this = restore;
                        return;
                    }
                }
                else
                {
                    return;
                }

                restore = this;

                // Keep reading if we can until we hit the next section.
                while (Read())
                {
                    if (TokenType == EIniValueKind.Section)
                    {
                        this = restore;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Reads the next INI token.
        /// </summary>
        /// <returns><see langword="true"/> if the next token was read successfully, <see langword="false"/> otherwise.</returns>
        public bool Read()
        {
            bool result = ReadSingleSegment();
            if (!result && IsFinalBlock && TokenType == EIniValueKind.Unknown)
            {
                ThrowHelper.ThrowUnexpectedToken();
            }

            return result;
        }

        private bool ReadSingleSegment()
        {
            if (UnescapedValueBuffer is not null)
            {
                ArrayPool<byte>.Shared.Return(UnescapedValueBuffer, true);
                UnescapedValueBuffer = null;
            }

            if (!HasMoreData())
            {
                TokenType = EIniValueKind.Unknown;
                return false;
            }

            byte first = Data[Consumed];

            if (first <= IniConstants.SpaceByte)
            {
                SkipWhiteSpace();
                if (!HasMoreData())
                {
                    TokenType = EIniValueKind.Unknown;
                    return false;
                }

                first = Data[Consumed];
            }

            TokenStartIndex = Consumed;

            if (first == IniConstants.SectionStartMarkerByte)
            {
                if (!ConsumeSection())
                {
                    return false;
                }

                int sameLineNextIndex = Data.IndexOfLessThan(IniConstants.ControlCharLessThan);
                if (sameLineNextIndex != 0 && Data[sameLineNextIndex] != IniConstants.DefaultCommentMarkerByte)
                {
                    // We will allow comments on a section line after the section declaration, but anything else is no-go.
                    throw new IniReaderException($"Nothing may appear on the same line after a section name (Line {LineNumber}).");
                }

                return true;
            }

            if (first == IniConstants.DefaultCommentMarkerByte)
            {
                if (TokenType == EIniValueKind.Key)
                {
                    // We have a situation where the comment comes immediately after the separator.
                    // Ex.  Value =;
                    // In this case we need to return back an empty value first. It also simplifies
                    // our logic later for consuming a property.
                    TokenType = EIniValueKind.Value;
                    ValueSpan = ReadOnlySpan<byte>.Empty;
                    return true;
                }

                return ConsumeComment();
            }

            return ConsumeProperty();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool HasMoreData()
        {
            if (Consumed >= Data.Length)
            {
                if (IsLastSpan)
                {
                    if (ReaderOptions.CommentHandling == EIniCommentHandling.Allow && TokenType == EIniValueKind.Comment)
                    {
                        return false;
                    }

                    if (TokenType == EIniValueKind.Key)
                    {
                        ThrowHelper.ThrowInvalidEndOfIniProperty();
                    }
                }

                return false;
            }

            return true;
        }

        private void SkipWhiteSpace()
        {
            // Create local copy to elide bounds checks.
            var localData = Data;
            for (; Consumed < localData.Length; ++Consumed)
            {
                byte currentByte = localData[Consumed];

                // Defined by spec.
                if (currentByte != IniConstants.SpaceByte &&
                    currentByte != IniConstants.ReturnByte &&
                    currentByte != IniConstants.NewlineByte &&
                    currentByte != IniConstants.TabByte)
                {
                    break;
                }

                if (currentByte == IniConstants.NewlineByte)
                {
                    ++LineNumber;
                }
            }
        }

        private bool ConsumeSection()
        {
            // Create local copy to elide bounds checks.
            var localData = Data[(Consumed + 1)..];

            //TODO: Are tabs not allowed in section names in spec?
            int sectionEnd = localData.IndexOfOrLessThan(IniConstants.SectionEndMarkerByte, IniConstants.ControlCharLessThan);
            if (sectionEnd > 0)
            {
                byte foundByte = localData[sectionEnd];
                if (foundByte == IniConstants.SectionEndMarkerByte)
                {
                    // Get the content without the [].
                    ValueSpan = localData[1..foundByte];
                    TokenType = EIniValueKind.Section;
                    Consumed += sectionEnd + 2;
                    return true;
                }
                else
                {
                    throw new IniReaderException("End of section not found.");
                }
            }
            else if (sectionEnd == 0)
            {
                throw new IniReaderException("Section name cannot be empty.");
            }
            else
            {
                if (IsLastSpan)
                {
                    throw new IniReaderException("End of section not found.");
                }

                return false;
            }
        }

        private bool ConsumeComment()
        {
            // Create local copy to elide bounds checks.
            var localData = Data[(Consumed + 1)..];

            int commentEnd = localData.IndexOf(IniConstants.NewlineByte, IniConstants.ReturnByte);
            if (commentEnd > 0)
            {
                // Get the content without the ;
                ValueSpan = localData[1..localData[commentEnd]];
                TokenType = EIniValueKind.Comment;
                Consumed += commentEnd + 1;
                return true;
            }
            else if (commentEnd == 0)
            {
                ValueSpan = ReadOnlySpan<byte>.Empty;
                TokenType = EIniValueKind.Comment;
                Consumed += commentEnd + 1;
                return true;
            }
            else
            {
                if (IsLastSpan)
                {
                    // Uh, what?
                    throw new IniReaderException("End of comment not found.");
                }

                return false;
            }
        }

        private bool ConsumeProperty()
        {
            // Create local copy to elide bounds checks.
            var localData = Data[(Consumed + 1)..];

            if (TokenType == EIniValueKind.Key)
            {
                // We are now looking for a value.

                int notWhiteSpace = localData.IndexOfNotWhiteSpace();
                localData = localData[notWhiteSpace..];
                byte first = localData[0];

                if (first == IniConstants.NewlineByte ||
                    first == IniConstants.ReturnByte)
                {
                    // This value is empty.
                    ValueSpan = ReadOnlySpan<byte>.Empty;
                    TokenType = EIniValueKind.Value;
                    Consumed += notWhiteSpace + 1; // Add in the newline byte.
                    return true;
                }

                bool startsWithQuote = first == IniConstants.ValueQuoteByte;
                if (startsWithQuote)
                {
                    /*
                     * key = "quoted value" (OK, reported without quotes)
                     * key = \"quoted value (OK, reported with first quote)
                     * key = "quoted value\" (ERROR, missing end quote)
                     * key = "quoted \"value" (OK, reported with quote in middle)
                     * key = regular ;value (OK, reported with trailing whitespace - next token should be a comment)
                     * key = regular \;value (OK, reported as regular value with \ before value)
                     * 
                     * Steps:
                     * 1. Skip all whitespace.
                     * 2. Check for " as byte[0].
                     * 3. If true:
                     *      a. Scan until next " or control character found.
                     *      b. If character != ", throw.
                     *      c. If index - 1 is \, slice and continue repeat from a.
                     *      d. If index - 1 is not \, mark index.
                     *      e. Scan for chars greater than space (control + space).
                     *      f. If index != -1, throw (["value" more content]) is not valid. Either the entire string is escaped or none of it is.
                     *          1. TODO: What does the spec say about this?
                     */
                    throw new NotImplementedException("Quoted values are not yet supported.");
                }
                else
                {
                    TokenType = EIniValueKind.Value;

                    int newLineOrComment = localData.IndexOf(IniConstants.NewlineByte, IniConstants.ReturnByte, IniConstants.DefaultCommentMarkerByte);
                    if (newLineOrComment == -1)
                    {
                        // This value goes to EOF.
                        ValueSpan = localData;
                        Consumed += localData.Length;
                        return true;
                    }

                    if (newLineOrComment == 0)
                    {
                        // We have already checked in ReadSingleSegment() that
                        // this is not the comment marker byte. We do not need
                        // to worry about consuming the marker byte.
                        ValueSpan = ReadOnlySpan<byte>.Empty;
                        ++Consumed;
                        return true;
                    }

                    if (localData[newLineOrComment] != IniConstants.DefaultCommentMarkerByte)
                    {
                        // This is an actual code, not the string representation (for example "\n").
                        // We do not need to bother doing escaping logic for these.
                        ValueSpan = localData.Slice(0, newLineOrComment);
                        Consumed += newLineOrComment + 1;
                        return true;
                    }

                    int workingIndex = newLineOrComment;
                    while (true)
                    {
                        if (localData[newLineOrComment - 1] == IniConstants.EscapeByte)
                        {
                            if (UnescapedValueBuffer is null)
                            {
                                UnescapedValueBuffer = ArrayPool<byte>.Shared.Rent(Data.Length - Consumed);
                                localData.Slice(0, newLineOrComment - 2).CopyTo(UnescapedValueBuffer);
                                //TODO: Finish unescaping logic
                            }

                            //TODO: Can we do something to this byte without allocating something new?
                            ref byte reference = ref MemoryMarshal.GetReference(localData);
                            var span = MemoryMarshal.CreateSpan(ref reference, localData.Length);
                            span[^1] = 0;
                        }
                        else
                        {
                            ValueSpan = Data.Slice(Consumed + 1, newLineOrComment);
                            Consumed += workingIndex;
                            return true;
                        }

                        localData = localData.Slice(0, newLineOrComment + 1);
                        newLineOrComment = localData.IndexOf(IniConstants.DefaultCommentMarkerByte);
                        workingIndex += newLineOrComment;
                    }
                }
            }
            else
            {
                // We are looking for a new key.
                byte propertyDelimiterByte = !ReaderOptions.ColonDelimiter ? (byte)'=' : (byte)':';

                int keyEnd = localData.IndexOf(IniConstants.NewlineByte, IniConstants.ReturnByte, propertyDelimiterByte);
                if (keyEnd != propertyDelimiterByte)
                {
                    throw new IniReaderException($"Unexpected end of key with delimiter {(char)propertyDelimiterByte} on line {LineNumber}");
                }

                TokenType = EIniValueKind.Key;
                ValueSpan = localData.Slice(0, keyEnd);
                return true;
            }
        }
    }
}
