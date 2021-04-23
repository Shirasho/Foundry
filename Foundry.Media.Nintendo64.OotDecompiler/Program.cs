using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.OotDecompiler.Disassembly;
using Foundry.Media.Nintendo64.OotDecompiler.Extraction;
using Foundry.Media.Nintendo64.Rom;
using static Foundry.Media.Nintendo64.OotDecompiler.ConsoleEditor;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            using var romFile = await RomData.LoadRomAsync(args.Length > 0 ? args[0] : throw new ArgumentException("First argument must be the ROM path."));

            Console.WriteLine($"Name: {romFile.Header.Title}");
            Console.WriteLine($"Format: {romFile.Header.Format}");

            Console.CursorVisible = false;
            int cursorTop = Console.CursorTop;

            while (true)
            {
                Console.WriteLine();
                var result = await RequestInputBlockAsync("Enter the number of the operation to perform.",
                    //("Extract files (Debug)", () => ExtractFilesAsync(romFile)),
                    ("Disassemble ROM", () => DisassembleRomAsync(romFile)),
                    ("Verify ROM", () => VerifyRomAsync(romFile)),
                    ("Change ROM Format", () => ChangeFormatAsync(romFile)),
                    ("Exit", BackTask)
                );

                if (result == EInputBlockResult.Back)
                {
                    // The way the Back erasing logic works makes it not erase
                    // correctly here, so we need to manually erase some lines.
                    ErasePreviousLine();
                    return;
                }

                Console.WriteLine();
                Console.WriteLine("The operation {0}.", result == EInputBlockResult.Success ? "completed successfully" : "has failed");
                Console.WriteLine();
                Console.WriteLine("To run another operation press Space, otherwise the application will exit.");

                if (Console.ReadKey().Key != ConsoleKey.Spacebar)
                {
                    break;
                }

                EraseLines(cursorTop);
                Console.WriteLine();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Redundancy", "RCS1213:Remove unused member declaration.", Justification = "<Pending>")]
        private static async ValueTask<EInputBlockResult> ExtractFilesAsync(IRomData romData)
        {
            Console.WriteLine("Extracting files (Debug)");

            // We need our rom in z64.
            var extractor = new RomExtractorAddressFinder(await romData.ConvertToAsync(ERomFormat.BigEndian));
            var options = new RomExtractionAddressFinderOptions { Concurrent = false };
            var stopwatch = Stopwatch.StartNew();

            try
            {
                const int startAddress = 0x40080;
                const int endAddress = 0x40080;
                // 1532
                const int startFileCount = 1532;
                const int endFileCount = 1532;
                int nextConsoleUpdate = startAddress;

                for (int currentAddress = startAddress; currentAddress <= endAddress; ++currentAddress)
                {
                    if (currentAddress == nextConsoleUpdate)
                    {
                        nextConsoleUpdate = Math.Min(nextConsoleUpdate + 0xF, endAddress);
                        string percent = (Percent(startAddress, endAddress, currentAddress) * 100).ToString("F2").PadLeft(6, '0');
                        Console.Write($"[{stopwatch.Elapsed}][{percent}%] Testing start position {currentAddress:X} - {nextConsoleUpdate:X}");
                    }

                    for (int fileCount = startFileCount; fileCount <= endFileCount; ++fileCount)
                    {
                        options.OffsetAddress = currentAddress;
                        options.FileCount = fileCount;

                        try
                        {
                            if (await extractor.ExtractAsync(options))
                            {
                                Console.WriteLine($"Detected successful extract at {currentAddress:X} with {fileCount} files.");
                                return EInputBlockResult.Success;
                            }

                            Console.CursorLeft = 0;
                        }
                        catch
                        {
                            Console.CursorLeft = 0;
                        }
                    }
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            return EInputBlockResult.Failed;
        }

        private static async ValueTask<EInputBlockResult> DisassembleRomAsync(IRomData romData)
        {
            Console.WriteLine("Disassembling ROM.");
            Console.WriteLine();
            Console.WriteLine("HEADER");
            var romBuild = romData.GetRomBuild();
            Console.WriteLine($"{romData.Header.Title}");
            Console.WriteLine($"Build: {romBuild.Version} ({romBuild.BuildNumber})");
            Console.WriteLine($"Size: {romData.Size.Size.Mebibits}Mib (0x{romData.Length:X})");
            Console.WriteLine($"Destination Code: {romData.Header.DestinationCode}");
            Console.WriteLine($"Game Code: {romData.Header.GameCode}");
            Console.WriteLine($"Format: {romData.Header.Format}");
            Console.WriteLine($"Mask Rom Version: {romData.Header.Version}");
            Console.WriteLine($"Entry Address: 0x{romData.Header.EntryAddress:X}");
            Console.WriteLine($"Return Address: 0x{romData.Header.ReturnAddress:X}");
            Console.WriteLine();

            var disassembler = new Disassembler();

            Console.WriteLine();

            if (!await PerformChecklistOperationAsync("First pass disassembly.", async () => await disassembler.DisassembleAsync(new DirectoryInfo(@"C:\Users\shira\Desktop\OoTDecomp"), true)))
            {
                return EInputBlockResult.Failed;
            }

            return EInputBlockResult.Success;
        }

        private static async ValueTask<EInputBlockResult> ChangeFormatAsync(IRomData romData)
        {
            return await RequestInputBlockAsync("Enter the number of the format to convert to.",
                (".z64 (Big Endian)", () => ConvertAsync(romData, ERomFormat.BigEndian)),
                (".v64 (Byte Swapped)", () => ConvertAsync(romData, ERomFormat.ByteSwapped)),
                (".n64 (Little Endian)", () => ConvertAsync(romData, ERomFormat.LittleEndian)),
                ("Back", BackTask)
            );

            static async ValueTask<EInputBlockResult> ConvertAsync(IRomData data, ERomFormat format)
            {
                FileInfo output;

                EraseLine();
                if (data.Header.Format == format)
                {
                    Console.WriteLine();
                    Console.WriteLine("The ROM data is already in the correct format.");
                    if (data is not RomFile)
                    {
                        output = new FileInfo(data.GetFilename());
                        await data.SaveAsync(output);
                        Console.WriteLine($"File saved to {output.FullName}");
                    }

                    // If it is a RomFile, the file already exists. Do nothing.
                    return EInputBlockResult.Success;
                }

                output = data is RomFile rf
                    ? rf.File.WithExtension(RomHeader.GetFormatExtension(format))
                    : new FileInfo(data.GetFilename());
                var converted = await data.ConvertToAsync(format);

                if (await PromptOverwriteIfExistsAsync(output) != EInputBlockResult.Success)
                {
                    return EInputBlockResult.Failed;
                }

                await converted.SaveAsync(output);
                Console.WriteLine($"File saved to {output.FullName}");
                return EInputBlockResult.Success;

                static async Task<EInputBlockResult> PromptOverwriteIfExistsAsync(FileInfo file)
                {
                    if (file.Exists)
                    {
                        return await RequestInputBlockAsync($"The file {file.FullName} already exists. Overwrite?",
                            ("Yes", SuccessTask),
                            ("No", FailedTask)
                        );
                    }

                    return EInputBlockResult.Success;
                }
            }
        }

        private static async ValueTask<EInputBlockResult> VerifyRomAsync(IRomData romData)
        {
            Console.WriteLine("Running diagnostics.");
            Console.WriteLine($"Entry Address: 0x{romData.Header.EntryAddress:X}");
            Console.WriteLine($"Return Address: 0x{romData.Header.ReturnAddress:X}");
            Console.WriteLine();

            await PerformChecklistOperationAsync("Verifying code entry address", () =>
            {
                if (romData.AssertValidEntryPoint())
                {
                    return new ValueTask<OperationResult>(new OperationResult($"Entry address 0x{romData.Header.EntryAddress:X} is not within the expected address range."));
                }

                return new ValueTask<OperationResult>(new OperationResult());
            });

            return EInputBlockResult.Success;
        }

        private static double Percent(int a, int b, int value)
        {
            double divisor = b - a;
            if (divisor == 0)
            {
                return 1;
            }
            return (value - a) / (double)(b - a);
        }
    }
}
