using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    internal enum ERomVersion
    {
        Unsupported,
        NTSC1_0,
        NTSC1_1,
        NTSC1_2,
        Europe1_0,
        Europe1_1,
        Japan_MasterQuest,
        USA_MasterQuest,
        Europe_MasterQuest,
        Europe_MasterQuestDebug
    };

    internal sealed class RomBuild
    {
        private static readonly Dictionary<string, ERomVersion> VersionMap = new Dictionary<string, ERomVersion>
        {
            { "zelda@srd44 98-10-21 04:56:31", ERomVersion.NTSC1_0 },
            { "zelda@srd44 98-10-26 10:58:45", ERomVersion.NTSC1_1 },
            { "zelda@srd44 98-11-10 14:34:22", ERomVersion.Europe1_0 },
            { "zelda@srd44 98-11-12 18:17:03", ERomVersion.NTSC1_2 },
            { "zelda@srd44 98-11-18 17:36:49", ERomVersion.Europe1_1 },
            { "zelda@srd022j 02-10-30 00:15:15", ERomVersion.Japan_MasterQuest },
            { "zelda@srd022j 02-12-19 14:05:42", ERomVersion.USA_MasterQuest },
            { "zelda@srd022j 03-02-21 20:37:19", ERomVersion.Europe_MasterQuest },
            { "zelda@srd022j 03-02-21 00:16:31", ERomVersion.Europe_MasterQuestDebug }
        };

        public string BuildNumber { get; }

        public ERomVersion Version { get; }

        private RomBuild(string buildNumber)
        {
            BuildNumber = buildNumber;
            Version = VersionMap.TryGetValue(BuildNumber, out var version) ? version : ERomVersion.Unsupported;
        }

        /// <summary>
        /// Attempts to create ROM build information based on a
        /// Big Endian data region.
        /// </summary>
        /// <param name="data">The data to parse.</param>
        /// <exception cref="FormatException">The build information could not be found in the ROM data.</exception>
        public static RomBuild Create(in ReadOnlySpan<byte> data)
        {
            if (TryCreate(data, out var result))
            {
                return result;
            }

            throw new FormatException("Could not find build info in ROM data.");
        }

        /// <summary>
        /// Attempts to create ROM build information using the specified
        /// ROM data.
        /// </summary>
        /// <param name="data">The ROM data.</param>
        /// <exception cref="FormatException">The build information could not be found in the ROM data.</exception>
        public static RomBuild Create(IRomData data)
        {
            if (TryCreate(data, out var result))
            {
                return result;
            }

            throw new FormatException("Could not find build info in ROM data.");
        }

        /// <summary>
        /// Attempts to create ROM build information using the specified
        /// ROM data.
        /// </summary>
        /// <param name="data">The ROM data.</param>
        /// <param name="result">On successful parse, contains the ROM build information.</param>
        /// <exception cref="FormatException">The build information could not be found in the ROM data.</exception>
        public static bool TryCreate(IRomData data, [NotNullWhen(true)] out RomBuild? result)
        {
            result = null;

            // zelda@ - Some builds (the chinese build for example) has a different prefix,
            // but we are only supporting decompilation of the NTSC 1.0 ROM.
            Span<byte> buildSearchBytes = data.Metadata.Format switch
            {
                // 00007400
                ERomFormat.BigEndian => stackalloc byte[] { 0x7A, 0x65, 0x6C, 0x64, 0x61, 0x40 },
                ERomFormat.ByteSwapped => stackalloc byte[] { 0x65, 0x7A, 0x64, 0x6C, 0x40, 0x61 },
                ERomFormat.LittleEndian => stackalloc byte[] { 0, 0x65, 0x6C, 0x64, 0x61, 0x40 },
                _ => Span<byte>.Empty
            };

            if (buildSearchBytes.IsEmpty)
            {
                return false;
            }

            Debug.Assert(Encoding.ASCII.GetString(buildSearchBytes) == "zelda@");

            ReadOnlySpan<byte> d = stackalloc byte[1];
            int buildStartAddress = d.IndexOf(buildSearchBytes);
            if (buildStartAddress < 0)
            {
                return false;
            }

            int buildEndAddress = d.Slice(buildStartAddress).IndexOf(0x00);
            if (buildEndAddress < 0)
            {
                buildEndAddress = d.Length;
            }

            int buildInfoLength = buildEndAddress - buildStartAddress;
            string buildInfo = Encoding.ASCII.GetString(d.Slice(buildStartAddress, buildInfoLength));

            result = new RomBuild(buildInfo);
            return true;
        }

        /// <summary>
        /// Attempts to create ROM build information based on a
        /// Big Endian data region.
        /// </summary>
        /// <param name="data">The data to parse.</param>
        /// <param name="result">On successful parse, contains the ROM build information.</param>
        public static bool TryCreate(in ReadOnlySpan<byte> data, [NotNullWhen(true)] out RomBuild? result)
        {
            result = null;

            // zelda@ - Some builds (the chinese build for example) has a different prefix,
            // but we are only supporting decompilation of the NTSC 1.0 ROM.
            Span<byte> buildSearchBytes = stackalloc byte[] { 0x7A, 0x65, 0x6C, 0x64, 0x61, 0x40 };

            Debug.Assert(Encoding.ASCII.GetString(buildSearchBytes) == "zelda@");

            int buildStartAddress = data.IndexOf(buildSearchBytes);
            if (buildStartAddress < 0)
            {
                return false;
            }

            int buildEndAddress = data.Slice(buildStartAddress).IndexOf(0x00);
            if (buildEndAddress < 0)
            {
                buildEndAddress = data.Length;
            }

            int buildInfoLength = buildEndAddress - buildStartAddress;
            string buildInfo = Encoding.ASCII.GetString(data.Slice(buildStartAddress, buildInfoLength));

            result = new RomBuild(buildInfo);
            return true;
        }
    }
}
