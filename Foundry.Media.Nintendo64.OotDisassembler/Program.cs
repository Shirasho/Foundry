using System;
using System.IO;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.OotDisassembler.Tools;
using Foundry.Media.Nintendo64.Rom;
using Foundry.Media.Nintendo64.Rom.Disassembly;
using static Foundry.Media.Nintendo64.OotDisassembler.ConsoleEditor;

namespace Foundry.Media.Nintendo64.OotDisassembler
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
                    ("Disassemble ROM", () => DisassembleRomAsync(romFile)),
                    ("Verify ROM CRC", () => VerifyRomCRCAsync(romFile)),
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

        private static ValueTask<EInputBlockResult> DisassembleRomAsync(IRomData romData)
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

            using var decompressedRomData = romData.Decompress();

            var disassembler = new Disassembler();
            var options = new DisassemblerOptions
            {
                OutputDir = new DirectoryInfo(@"C:\Users\shira\Desktop\OoTDecomp"),
                SplitFiles = false,
                KnownFiles =
                {
                    new FileEntry("INTERNAL_HEADER", 0xA4000000, romData.GetHeaderData()),
                    new FileEntry("IPL3", 0xA4000040, romData.GetIPL3Data()),
                    // We don't know what this is, but we can figure this out
                    // when we start disassembling correctly and getting a bunch of opcode errors.
                    // For reference, we know that Mario ends at 0x0B6A40. We'll go a bit higher then
                    // work our way down.
                    new FileEntry("CODE", romData.Header.EntryAddress, decompressedRomData.GetCodeData(0x0C3500))
                },
                Regions =
                {
                    new DataRegion("INTERNAL_HEADER", 0xA4000000, 0xA400003F)
                },
                KnownFunctions =
                {
                    { 0x80000400, "Main" }
                }
            };

            //disassembler.AddRegion(new DataRegion("INTERNAL_HEADER", 0xA4000000, 0xA400003F));

            Console.WriteLine();

            if (!PerformChecklistOperation("Disassembling ROM to MIPS.", () => disassembler.Disassemble(options), r => r.Exception))
            {
                return new ValueTask<EInputBlockResult>(EInputBlockResult.Failed);
            }

            return new ValueTask<EInputBlockResult>(EInputBlockResult.Success);
        }

        private static async ValueTask<EInputBlockResult> VerifyRomCRCAsync(IRomData romData)
        {
            Console.WriteLine("Disassembling ROM.");
            Console.WriteLine();

            var data = await romData.GetDataAsync();
            switch (Crc.HasCorrectCrc(data.Span, out var crc))
            {
                case false:
                    Console.WriteLine("ROM has invalid CRC values.");
                    Console.WriteLine($"Expected CRC1: 0x{crc.Crc1:X8}, Actual CRC1: 0x{romData.Header.Crc1:X8}");
                    Console.WriteLine($"Expected CRC2: 0x{crc.Crc1:X8}, Actual CRC2: 0x{romData.Header.Crc2:X8}");
                    break;
                case true:
                    Console.WriteLine("CRC OK.");
                    break;
                case null:
                default:
                    Console.WriteLine("Unable to calculate CRC - unknown boot code.");
                    break;
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
    }
}
