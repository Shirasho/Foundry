using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;
using Foundry.Media.Nintendo64.Rom;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Disassembly.OoT
{
    /// <summary>
    /// Options for the <see cref="Disassembler"/>.
    /// </summary>
    public class DisassemblerOptions
    {
        /// <summary>
        /// ROM Data segments.
        /// </summary>
        /// <remarks>
        /// If you do not know what you are doing, call <see cref="LoadDefaultSegmentsAndRegions"/> instead.
        /// </remarks>
        public ISet<DataSegment> Segments { get; } = new HashSet<DataSegment>(EqualityComparer.Create<DataSegment>((a, b) => a?.VirtualAddress == b?.VirtualAddress));

        /// <summary>
        /// ROM Data regions.
        /// </summary>
        /// <remarks>
        /// If you do not know what you are doing, call <see cref="LoadDefaultSegmentsAndRegions"/> instead.
        /// </remarks>
        public ISet<DataRegion> Regions { get; } = new HashSet<DataRegion>(EqualityComparer.Create<DataRegion>((a, b) => a?.StartAddress == b?.StartAddress));

        /// <summary>
        /// A collection of known functions.
        /// </summary>
        public IList<KnownFunction> KnownFunctions { get; } = new List<KnownFunction>();

        /// <summary>
        /// A collection of known objects.
        /// </summary>
        public IList<KnownObject> KnownObjects { get; } = new List<KnownObject>();

        /// <summary>
        /// A collection of known variables.
        /// </summary>
        public IList<KnownVariable> KnownVariables { get; } = new List<KnownVariable>();

        /// <summary>
        /// The directory to store temporary data.
        /// </summary>
        /// <remarks>
        /// Defaults to %<see cref="AppContext.BaseDirectory"/>%/temp/
        /// </remarks>
        public DirectoryInfo TempDir { get; init; } = new DirectoryInfo(AppContext.BaseDirectory).CombineDirectory("temp");

        /// <summary>
        /// The <see cref="TextWriter"/> to write non-critical errors to.
        /// </summary>
        public TextWriter ErrorWriter { get; init; } = Console.Error;

        /// <summary>
        /// The <see cref="TextWriter"/> to write status updates and log statements to.
        /// </summary>
        public TextWriter LogWriter { get; init; } = Console.Out;

        /// <summary>
        /// The ROM data to disassemble.
        /// </summary>
        public IRomData Data { get; }

        /// <summary>
        /// Whether to include comments in MIPS temp files.
        /// </summary>
        /// <remarks>
        /// This is useful if you are new to MIPS or assembly and want to
        /// understand what is happening in order to write your own
        /// <see cref="ISourceBuilder"/>.
        /// </remarks>
        public bool IncludeComments { get; init; } = true;

        public DisassemblerOptions(IRomData data)
        {
            Guard.IsNotNull(data, nameof(data));

            Data = data;
        }

        public DisassemblerOptions(DisassemblerOptions other)
        {
            Guard.IsNotNull(other, nameof(other));

            Segments.AddRange(other.Segments);
            Regions.AddRange(other.Regions);
            KnownFunctions.AddRange(other.KnownFunctions);
            KnownObjects.AddRange(other.KnownObjects);
            KnownVariables.AddRange(other.KnownVariables);
            TempDir = other.TempDir;
            ErrorWriter = other.ErrorWriter;
            LogWriter = other.LogWriter;
            Data = other.Data;
        }

        /// <summary>
        /// Loads the default ROM segments and regions.
        /// </summary>
        public void LoadDefaultSegmentsAndRegions()
        {
            Segments.Add(new DataSegment("INTERNAL_HEADER", Data.GetHeaderData(), 0xA4000000));
            Regions.Add(new DataRegion("INTERNAL_HEADER", 0xA4000000, 0xA400003F));

            Segments.Add(new DataSegment("IPL3", Data.GetIPL3(), 0xA4000040));

            Segments.Add(new DataSegment("CODE", Data.GetCodeData(), Data.Header.EntryAddress));
        }

        /// <summary>
        /// Loads the default ROM segments and regions.
        /// </summary>
        public async ValueTask LoadDefaultSegmentsAndRegionsAsync()
        {
            Segments.Add(new DataSegment("INTERNAL_HEADER", await Data.GetHeaderDataAsync(), 0xA4000000));
            Regions.Add(new DataRegion("INTERNAL_HEADER", 0xA4000000, 0xA400003F));

            Segments.Add(new DataSegment("IPL3", await Data.GetIPL3Async(), 0xA4000040));

            Segments.Add(new DataSegment("CODE", await Data.GetCodeDataAsync(), Data.Header.EntryAddress));
        }
    }

    public record KnownFunction(string Name, uint Address);
    public record KnownObject(string Name, uint Address, int? Size = null);
    public record KnownVariable(string Name, uint Address, int Size);
}
