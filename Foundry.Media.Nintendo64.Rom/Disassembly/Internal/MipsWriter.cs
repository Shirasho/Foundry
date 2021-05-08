using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Foundry.Media.Nintendo64.Rom.Disassembly.Internal
{
    internal class MipsWriter : IDisposable
    {
        public readonly ref struct WriteResult
        {
            public readonly bool ValidationResult { get; }
            public readonly string WrittenLine { get; }

            public WriteResult(string line, bool validated)
            {
                WrittenLine = line;
                ValidationResult = validated;
            }
        }

        public int CurrentLine { get; private set; }
        public int NextLine => CurrentLine + 1;

        private readonly TextWriter Writer;

        public MipsWriter(Stream output)
        {
            Writer = new StreamWriter(output, Encoding.UTF8, leaveOpen: false);
        }

        public void Dispose()
        {
            Flush();
            Writer.Dispose();
        }

        public void Flush()
        {
            Writer.Flush();
        }

        public void WriteLine()
        {
            Writer.WriteLine();
            ++CurrentLine;
        }

        public void WriteHeader()
        {
            WriteLine(".set noat # allow use of $at", false);
            WriteLine(".set noreorder # disallow insertion of nop after branches", false);
            WriteLine(".set gp=64 # allow use of 64bit registers", false);
            WriteLine(".macro glabel label", false);
            WriteLine("    .global \\label", false);
            WriteLine("    \\label:", false);
            WriteLine(".endm", false);
            WriteLine();
        }

        public WriteResult WriteLine(string value)
        {
            return WriteLine(value, true);
        }

        public WriteResult WriteLabel(uint atAddress, string? name = null)
        {
            return WriteLine(name?.EnsureEndsWith(':') ?? $".L_{atAddress:X8}:", false);
        }

        public WriteResult WriteSwitchCase(uint atAddress)
        {
            return WriteLine($"glabel .L_{atAddress:X8}", false);
        }

        public WriteResult WriteFunction(RomFunction function)
            => WriteFunction(function.Name);

        public WriteResult WriteFunction(string function)
        {
            return WriteLine($"glabel {function}", false);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private WriteResult WriteLine(string line, bool verify)
        {
            Writer.WriteLine(line);
            return new WriteResult(line, !verify || VerifyEntry(line));
        }

        private static bool VerifyEntry(string entry)
        {
            string[] parts = entry.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return parts.Length switch
            {
                4 => VerifyEntry4(parts),
                _ => true
            };
        }

        private static bool VerifyEntry4(string[] parts)
        {
            if (parts[0].StartsWith("sll") && parts[1].AsSpan().TrimEnd(',').SequenceEqual("$zero"))
            {
                // Don't care about the 3rd register arg or the shift amount.
                // $zero is a read-only register.
                return false;
            }

            return true;
        }
    }
}
