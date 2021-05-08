using System;
using System.Collections.Generic;
using System.IO;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    public class FileEntry
    {
        public string Name { get; }
        public uint VirtualAddress { get; }

        private readonly string? Path;
        private ReadOnlyMemory<byte> Data;

        public FileEntry(string path, string name, uint virtualAddress)
        {
            Name = name;
            VirtualAddress = virtualAddress;
            Path = path;
            Data = ReadOnlyMemory<byte>.Empty;
        }

        public FileEntry(string name, uint virtualAddress, ReadOnlyMemory<byte> data)
        {
            Name = name;
            VirtualAddress = virtualAddress;
            Data = data;
        }

        internal ReadOnlyMemory<byte> GetFileData()
        {
            if (Data.IsEmpty && !string.IsNullOrWhiteSpace(Path))
            {
                Data = File.ReadAllBytes(System.IO.Path.Combine(Path, Name));
            }

            return Data;
        }
    }

    /// <summary>
    /// Options to use when performing MIPS disassembly.
    /// </summary>
    public class DisassemblerOptions
    {
        /// <summary>
        /// Known function names.
        /// </summary>
        public IDictionary<uint, string> KnownFunctions { get; } = new Dictionary<uint, string>();

        /// <summary>
        /// Known variable names.
        /// </summary>
        public IDictionary<uint, string> KnownVariables { get; } = new Dictionary<uint, string>();

        /// <summary>
        /// Known object names.
        /// </summary>
        public IDictionary<uint, string> KnownObjects { get; } = new Dictionary<uint, string>();

        /// <summary>
        /// Known file regions.
        /// </summary>
        public ISet<FileEntry> KnownFiles { get; } = new HashSet<FileEntry>(EqualityComparer.Create<FileEntry>((a, b) => a is null || b is null || a.VirtualAddress == b.VirtualAddress));

        /// <summary>
        /// Data regions.
        /// </summary>
        public ISet<DataRegion> Regions { get; } = new HashSet<DataRegion>(EqualityComparer.Create<DataRegion>((a, b) => a is null || b is null || a.StartAddress == b.StartAddress));

        /// <summary>
        /// The directory to place output files.
        /// </summary>
        public DirectoryInfo OutputDir { get; set; } = new DirectoryInfo(AppContext.BaseDirectory);

        /// <summary>
        /// Whether to split ASM output into separate files based on detected object boundaries.
        /// </summary>
        public bool SplitFiles { get; set; }
    }
}
