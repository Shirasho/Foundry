using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.OotDecompiler.Extraction;
using Foundry.Media.Nintendo64.Rom;

namespace Foundry.Media.Nintendo64.OotDecompiler
{
    internal static class Program
    {
        static async Task Main()
        {
            using var romFile = await RomData.LoadRomAsync(@"E:\Games\Nintendo64\Legend of Zelda, The - Ocarina of Time (USA).z64");
            //var romBuild = await romFile.GetRomBuildAsync();

            Console.WriteLine($"Name: {romFile.Metadata.Title}");
            //Console.WriteLine($"Build: {romBuild.Version} ({romBuild.BuildNumber})");
            Console.WriteLine($"Size: {romFile.Size.Size.Mebibits}Mib (0x{romFile.Length:X})");
            Console.WriteLine($"Destination Code: {romFile.Metadata.DestinationCode}");
            Console.WriteLine($"Game Code: {romFile.Metadata.GameCode}");
            Console.WriteLine($"Format: {romFile.Metadata.Format}");
            Console.WriteLine($"Mask Rom Version: {romFile.Metadata.MaskRomVersion}");

            Console.WriteLine();
            Console.CursorVisible = false;

            var result = await RequestInputBlockAsync("Enter the number of the operation to perform.",
                ("Extract files (Debug)", () => ExtractFilesAsync(romFile)),
                ("Change format", () => ChangeFormatAsync(romFile)),
                ("Exit", BackTask)
            );

            if (result == EInputBlockResult.Back)
            {
                return;
            }

            Console.WriteLine();
            Console.WriteLine("The operation {0}.", result == EInputBlockResult.Success ? "completed successfully" : "has failed");

            Console.CursorVisible = true;

            Console.WriteLine();
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static async ValueTask<EInputBlockResult> ExtractFilesAsync(IRomData romData)
        {
            Console.WriteLine("Extracting files (Debug)");

            // We need our rom in z64.
            var extractor = new RomExtractorAddressFinder(await romData.ConvertToAsync(ERomFormat.BigEndian));
            var options = new RomExtractionAddressFinderOptions();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                const int startAddress = 0x10000;
                const int endAddress = 0x20000;
                int nextConsoleUpdate = startAddress;

                for (int currentAddress = startAddress; currentAddress <= endAddress; ++currentAddress)
                {

                    if (currentAddress == nextConsoleUpdate)
                    {
                        nextConsoleUpdate = Math.Min(nextConsoleUpdate + 0xF, endAddress);
                        string percent = (Percent(startAddress, endAddress, currentAddress) * 100).ToString("F2").PadLeft(6, '0');
                        Console.Write($"[{stopwatch.Elapsed}][{percent}%] Testing start position {currentAddress:X} - {nextConsoleUpdate:X}");
                    }

                    for (int fileCount = 1000; fileCount <= 2500; ++fileCount)
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
                if (data.Metadata.Format == format)
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
                    ? rf.File.WithExtension(RomMetadata.GetFormatExtension(format))
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
                            ("Yes", () => new ValueTask<EInputBlockResult>(EInputBlockResult.Success)),
                            ("No", () => new ValueTask<EInputBlockResult>(EInputBlockResult.Failed))
                        );
                    }

                    return EInputBlockResult.Success;
                }
            }
        }

        private enum EInputBlockResult
        {
            Success,
            Failed,
            Back
        }

        private static Func<ValueTask<EInputBlockResult>> BackTask { get; } = new Func<ValueTask<EInputBlockResult>>(() => new ValueTask<EInputBlockResult>(EInputBlockResult.Back));
        private static bool BackTracker;

        private static async Task<EInputBlockResult> RequestInputBlockAsync(string title, params (string Title, Func<ValueTask<EInputBlockResult>> Callback)[] callbacks)
        {
            int cursorTop = Console.CursorTop;
            while (true)
            {
                Console.WriteLine(title);
                for (int i = 0; i < callbacks.Length; ++i)
                {
                    Console.WriteLine($"{i + 1}. {callbacks[i].Title}");
                }

                int callbackIndex = (int)Console.ReadKey().Key - (int)ConsoleKey.D1;
                if (callbackIndex < 0 || callbackIndex >= callbacks.Length)
                {
                    EraseLines(cursorTop + 1);
                    continue;
                }

                EraseLine();
                Console.WriteLine($"[{callbackIndex + 1}]");
                Console.WriteLine();

                var callback = callbacks[callbackIndex];
                var result = await callback.Callback();
                switch (result)
                {
                    case EInputBlockResult.Back:
                        EraseLines(cursorTop + 1);
                        if (!BackTracker)
                        {
                            BackTracker = true;
                            return result;
                        }
                        else
                        {
                            BackTracker = false;
                            continue;
                        }
                    case EInputBlockResult.Success:
                    case EInputBlockResult.Failed:
                        return result;

                }
            }
        }

        private static void EraseLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }
        private static void EraseLines(int toCursorTop)
        {
            while (Console.CursorTop >= toCursorTop)
            {
                EraseLine();
                --Console.CursorTop;
            }
        }

        private static double Percent(int a, int b, int value)
        {
            return (value - a) / (double)(b - a);
        }
    }
}
