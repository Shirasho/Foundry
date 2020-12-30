using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Foundry.Collections;

namespace Foundry
{
    public static class ReadOnlySpanExtensions
    {
        /// <summary>
        /// Splits the contents of <paramref name="span"/> on whitespace characters.
        /// </summary>
        /// <param name="span">The chars to split.</param>
        /// <param name="stringSplitOptions">The split options.</param>
        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
            => Split(span, ' ', stringSplitOptions);

        /// <summary>
        /// Splits the contents of <paramref name="span"/> on the specified separator.
        /// </summary>
        /// <param name="span">The chars to split.</param>
        /// <param name="separator">The character to split on.</param>
        /// <param name="stringSplitOptions">The split options.</param>
        public static SpanSplitEnumerator<char> Split(this ReadOnlySpan<char> span, char separator, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
            => new SpanSplitEnumerator<char>(span, separator, stringSplitOptions);

        /// <summary>
        /// Splits the contents of <paramref name="span"/> on the specified separator.
        /// </summary>
        /// <param name="span">The chars to split.</param>
        /// <param name="separator">The consecutive characters (string) to split on.</param>
        /// <param name="stringSplitOptions">The split options.</param>
        public static SpanSplitSequenceEnumerator<char> Split(this ReadOnlySpan<char> span, ReadOnlySpan<char> separator, StringSplitOptions stringSplitOptions = StringSplitOptions.None)
            => new SpanSplitSequenceEnumerator<char>(span, separator, stringSplitOptions);

        /// <remarks>https://gist.github.com/bbartels/87c7daae28d4905c60ae77724a401b20</remarks>
        public ref struct SpanSplitEnumerator<T>
            where T : IEquatable<T>
        {
            public ReadOnlySpan<T> Current => Sequence.Slice(StartIndex, Offset);

            private readonly ReadOnlySpan<T> Sequence;
            private readonly StringSplitOptions SplitOptions;
            private readonly StackList<int> SeparatorIndices;

            private int Index;
            private int SeparatorIndex;
            private int StartIndex;
            private int Offset;

            internal SpanSplitEnumerator(ReadOnlySpan<T> span, [MaybeNull] T separator, StringSplitOptions stringSplitOptions)
            {
                Sequence = span;
                SplitOptions = stringSplitOptions;

                var separatorIndices = new StackList<int>(2);
                for (int i = 0; i < span.Length; ++i)
                {
                    if (span[i].Equals(separator))
                    {
                        separatorIndices.Append(i);
                    }
                }
                SeparatorIndices = separatorIndices;

                Index = 0;
                SeparatorIndex = 0;
                StartIndex = 0;
                Offset = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SpanSplitEnumerator<T> GetEnumerator()
                => this;

            public bool MoveNext()
            {
                if (Index >= Sequence.Length)
                {
                    return false;
                }

                if (SplitOptions == StringSplitOptions.RemoveEmptyEntries)
                {
                    while (SeparatorIndex < SeparatorIndices.Length && Index == SeparatorIndices[SeparatorIndex])
                    {
                        Index = SeparatorIndices[SeparatorIndex] + 1;
                        ++SeparatorIndex;
                    }
                }

                StartIndex = Index;
                Offset = SeparatorIndex < SeparatorIndices.Length ? SeparatorIndices[SeparatorIndex] - Index : Sequence.Length - StartIndex;

                Index = SeparatorIndex < SeparatorIndices.Length ? SeparatorIndices[SeparatorIndex] + 1 : Sequence.Length;
                ++SeparatorIndex;

                return true;
            }
        }

        /// <remarks>https://gist.github.com/bbartels/87c7daae28d4905c60ae77724a401b20</remarks>
        public ref struct SpanSplitSequenceEnumerator<T>
            where T : IEquatable<T>
        {
            public ReadOnlySpan<T> Current => Sequence.Slice(StartIndex, Offset);

            private readonly ReadOnlySpan<T> Sequence;
            private readonly StringSplitOptions SplitOptions;
            private readonly StackList<int> SeparatorIndices;

            private int Index;
            private int SeparatorIndex;
            private int StartIndex;
            private int Offset;
            private int SeparatorLength;

            internal SpanSplitSequenceEnumerator(ReadOnlySpan<T> span, ReadOnlySpan<T> separator, StringSplitOptions stringSplitOptions)
            {
                Sequence = span;
                SplitOptions = stringSplitOptions;

                var separatorIndices = new StackList<int>(2);
                SeparatorLength = separator.Length;

                //https://github.com/dotnet/runtime/blob/master/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs#L1579
                for (int i = 0; i < span.Length; ++i)
                {
                    if (span[i].Equals(separator[0]) && SeparatorLength <= span.Length - i)
                    {
                        if (SeparatorLength == 1 || span.Slice(i, SeparatorLength).SequenceEqual(separator))
                        {
                            separatorIndices.Append(i);
                            i += SeparatorLength - 1;
                        }
                    }
                }
                SeparatorIndices = separatorIndices;

                Index = 0;
                SeparatorIndex = 0;
                StartIndex = 0;
                Offset = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public SpanSplitSequenceEnumerator<T> GetEnumerator()
                => this;

            public bool MoveNext()
            {
                if (Index >= Sequence.Length) { return false; }

                if (SplitOptions == StringSplitOptions.RemoveEmptyEntries)
                {
                    while (SeparatorIndex < SeparatorIndices.Length && Index == SeparatorIndices[SeparatorIndex])
                    {
                        Index = SeparatorIndices[SeparatorIndex] + SeparatorLength;
                        ++SeparatorIndex;
                    }
                }

                StartIndex = Index;
                Offset = SeparatorIndex < SeparatorIndices.Length ? SeparatorIndices[SeparatorIndex] - Index : Sequence.Length - StartIndex;

                Index = SeparatorIndex < SeparatorIndices.Length ? SeparatorIndices[SeparatorIndex] + SeparatorLength : Sequence.Length;
                ++SeparatorIndex;

                return true;
            }
        }
    }
}
