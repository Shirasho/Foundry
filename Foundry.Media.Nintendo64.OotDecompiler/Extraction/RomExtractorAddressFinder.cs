using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDecompiler.Extraction
{
    internal class RomExtractionAddressFinderOptions : RomExtractionOptions
    {
        /// <summary>
        /// The starting address to use.
        /// </summary>
        public int OffsetAddress { get; set; }

        /// <summary>
        /// The number of assumed files.
        /// </summary>
        public int FileCount { get; set; }
    }

    internal sealed class RomExtractorAddressFinder : IRomExtractor
    {
        private static readonly int[] Numbers = Enumerable.Range(0, 2500).ToArray();
        private readonly IRomData Data;

        public RomExtractorAddressFinder(IRomData romData)
        {
            Data = romData;
        }
        public async Task<bool> ExtractAsync(RomExtractionOptions options, CancellationToken cancellationToken = default)
        {
            var opts = (RomExtractionAddressFinderOptions)options;
            int offsetAddress = opts.OffsetAddress;
            var romData = await Data.GetDataAsync();

            var result = Parallel.ForEach(Numbers.Take(opts.FileCount), new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 }, (file, o) =>
            {
                if (!ExtractFile(romData, file, offsetAddress, cancellationToken))
                {
                    o.Stop();
                }
            });

            return result.IsCompleted;
        }

        private bool ExtractFile(ReadOnlyMemory<byte> data, int fileIndex, int offset, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return false;
            }

            try
            {
                int entryOffset = offset + 16 * fileIndex;
                var metadataWindow = data.Slice(entryOffset, 16);
                uint virtStart = ReadUInt(metadataWindow, 0);
                uint virtEnd = ReadUInt(metadataWindow, 4);
                uint physStart = ReadUInt(metadataWindow, 8);
                uint physEnd = ReadUInt(metadataWindow, 12);

                //bool compressed;
                uint size;

                if (physEnd == 0)
                {
                    //compressed = false;
                    size = virtEnd - virtStart;
                }
                else
                {
                    //compressed = true;
                    size = physEnd - physStart;
                }

                if (size > data.Length - physStart)
                {
                    return false;
                }

                //if (compressed)
                //{
                //    data = Decompress(data.Span);
                //}
            }
            catch
            {
                return false;
            }

            return true;

            static uint ReadUInt(ReadOnlyMemory<byte> buffer, int offset)
            {
                Span<byte> localBuffer = stackalloc byte[4];
                buffer.Slice(offset, 4).Span.CopyTo(localBuffer);

                return BitConverter.ToUInt32(localBuffer);
            }

            //static ReadOnlyMemory<byte> Decompress(ReadOnlySpan<byte> romData)
            //{
            //    uint leng = (uint)(romData[4] << 24 | romData[5] << 16 | romData[6] << 8 | romData[7]);
            //    byte[] Result = new byte[leng];
            //    int Offs = 16;
            //    int dstoffs = 0;
            //    while (true)
            //    {
            //        byte header = romData[Offs++];
            //        for (int i = 0; i < 8; i++)
            //        {
            //            if ((header & 0x80) != 0)
            //            {
            //                Result[dstoffs++] = romData[Offs++];
            //            }
            //            else
            //            {
            //                byte b = romData[Offs++];
            //                int offs = ((b & 0xF) << 8 | romData[Offs++]) + 1;
            //                int length = (b >> 4) + 2;
            //                if (length == 2)
            //                {
            //                    length = romData[Offs++] + 0x12;
            //                }

            //                for (int j = 0; j < length; j++)
            //                {
            //                    Result[dstoffs] = Result[dstoffs - offs];
            //                    dstoffs++;
            //                }
            //            }
            //            if (dstoffs >= leng)
            //            {
            //                return Result;
            //            }
            //            header <<= 1;
            //        }
            //    }
            //}
        }
    }
}
