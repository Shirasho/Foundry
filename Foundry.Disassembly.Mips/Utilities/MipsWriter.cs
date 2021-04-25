using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Foundry.Disassembly.Mips.Codes;

namespace Foundry.Disassembly.Mips.Utilities
{
    internal class MipsWriter : IDisposable
    {
        private readonly struct OutputEntry
        {
            public readonly uint VirtualAddress { get; }
            public readonly string? EntryStamp { get; }
            public readonly string Entry { get; }

            public OutputEntry(uint address, string? entryStamp, string entry)
            {
                VirtualAddress = address;
                EntryStamp = entryStamp;
                Entry = entry;
            }
        }

        private readonly IDictionary<uint, IList<OutputEntry>> OutputEntries = new SortedDictionary<uint, IList<OutputEntry>>();
        private readonly ISet<string> Labels = new SortedSet<string>();
        private readonly TextWriter Writer;
        private int WrittenLines;

        public MipsWriter(TextWriter writer)
        {
            Writer = writer;
        }

        public void Dispose()
        {
            Flush();
        }

        public void Flush()
        {

            foreach (var entry in OutputEntries)
            {
                foreach (OutputEntry message in entry.Value)
                {
                    if (!string.IsNullOrWhiteSpace(message.EntryStamp))
                    {
                        Writer.Write(message.EntryStamp);
                        Writer.WriteLine(message.Entry);
                    }
                    else
                    {
                        Writer.WriteLine(message.Entry);
                    }
                    ++WrittenLines;
                    if (!VerifyEntry(message))
                    {
                        Writer.Flush();
                        //Debug.Fail($"Instruction on line {WrittenLines} is not allowed.");
                    }
                }
            }
            OutputEntries.Clear();
            Labels.Clear();
            Writer.Flush();
        }

        public void WriteLine()
        {

        }

        public void WriteHeader()
        {
            Writer.WriteLine(".set noat # allow use of $at");
            Writer.WriteLine(".set noreorder # disallow insertion of nop after branches");
            Writer.WriteLine(".set gp=64 # allow use of 64bit registers");
            Writer.WriteLine(".macro glabel label");
            Writer.WriteLine("    .global \\label");
            Writer.WriteLine("    \\label:");
            Writer.WriteLine(".endm");
            Writer.WriteLine();

            WrittenLines += 8;
        }

        public void WriteNop(in HexCode instruction, in HexCode virtualAddress, uint instructionAddress)
        {
            AddEntry(new OutputEntry(virtualAddress, $"/* 0x{instructionAddress:X5} {virtualAddress} {instruction} */ ", "nop"));
        }

        public void WriteComment(in HexCode instruction, in HexCode virtualAddress, uint instructionAddress, params string[] value)
        {
            foreach (string inst in value)
            {
                AddEntry(new OutputEntry(virtualAddress, $"/* 0x{instructionAddress:X5} {virtualAddress} {instruction} */ ", $"/* {inst} */"));
            }
        }

        public string WriteLabel(uint atAddress, string? name = null)
        {
            string n = name?.EnsureEndsWith(':') ?? $".L_{atAddress:X8}:";
            if (Labels.Add(n))
            {
                AddEntry(new OutputEntry(atAddress, null, n), true);
            }
            return n.RemoveLast();
        }

        public void WriteInstruction(in HexCode instruction, in HexCode virtualAddress, uint instructionAddress, params string[] value)
        {
            foreach (string inst in value)
            {
                AddEntry(new OutputEntry(virtualAddress, $"/* 0x{instructionAddress:X5} {virtualAddress} {instruction} */ ", inst));
            }
        }

        public void WriteError(in HexCode instruction, in HexCode virtualAddress, uint instructionAddress, string value)
        {
            AddEntry(new OutputEntry(virtualAddress, $"/* 0x{instructionAddress:X5} {virtualAddress} {instruction} */ ", $"/* {value} */"));
        }

        private void AddEntry(in OutputEntry entry, bool prepend = false)
        {
            if (OutputEntries.TryGetValue(entry.VirtualAddress, out var entries))
            {
                if (prepend)
                {
                    entries.Insert(0, entry);
                }
                else
                {
                    entries.Add(entry);
                }
            }
            else
            {
                entries = new List<OutputEntry> { entry };
                OutputEntries.Add(entry.VirtualAddress, entries);
            }
        }

        private static bool VerifyEntry(in OutputEntry entry)
        {
            string[] parts = entry.Entry.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);
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
