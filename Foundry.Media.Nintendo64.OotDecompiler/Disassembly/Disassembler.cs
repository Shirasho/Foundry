using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Rom.Utilities;

namespace Foundry.Media.Nintendo64.OotDecompiler.Disassembly
{
    internal partial class Disassembler
    {
        private static readonly string[] Registers =
            { "$zero", "$at", "$v0", "$v1", "$a0",
              "$a1", "$a2", "$a3", "$t0", "$t1",
              "$t2", "$t3", "$t4", "$t5", "$t6",
              "$t7", "$s0", "$s1", "$s2", "$s3",
              "$s4", "$s5", "$s6", "$s7", "$t8",
              "$t9", "$k0", "$k1", "$gp", "$sp",
              "$fp", "$ra" };

        private readonly SortedList<uint, DisassemblerDataSegment> DataSegments = new SortedList<uint, DisassemblerDataSegment>();
        private readonly SortedList<uint, DisassemblerDataRegion> DataRegions = new SortedList<uint, DisassemblerDataRegion>();

        private readonly IDictionary<uint, bool> DataRegionAddressCache = new Dictionary<uint, bool>();
        private readonly IDictionary<uint, bool> CodeAddressCache = new Dictionary<uint, bool>();

        public IDictionary<uint, RomFunction> KnownFunctions { get; } = new Dictionary<uint, RomFunction>();
        public IDictionary<uint, RomVariable> KnownVariables { get; } = new Dictionary<uint, RomVariable>();
        public IDictionary<uint, RomObject> KnownObjects { get; } = new Dictionary<uint, RomObject>();

        private readonly Dictionary<uint, RomObject> Objects = new Dictionary<uint, RomObject>();
        private readonly Dictionary<uint, RomFunction> Functions = new Dictionary<uint, RomFunction>();
        private readonly SortedDictionary<uint, RomVariable> Variables = new SortedDictionary<uint, RomVariable>();
        private readonly Dictionary<uint, RomLabel> Labels = new Dictionary<uint, RomLabel>();
        private readonly HashSet<uint> SwitchCaseAddresses = new HashSet<uint>();

        public void AddDataSegment(string name, ReadOnlyMemory<byte> data, uint virtualAddress, string? saveFileName = null)
        {
            DataSegments.Add(virtualAddress, new DisassemblerDataSegment(name, data, virtualAddress, saveFileName));
            ClearCaches();
        }

        public void AddDataRegion(string name, uint startAddress, uint endAddress)
        {
            DataRegions.Add(startAddress, new DisassemblerDataRegion(name, startAddress, endAddress));
            ClearCaches();
        }

        public void AddKnownFunction(uint address, string functionName)
        {
            KnownFunctions.TryAdd(address, new RomFunction(address, functionName));
        }

        public void AddKnownObject(uint address, int size, string objectName)
        {
            KnownObjects.TryAdd(address, new RomObject(address, size, objectName));
        }

        public void AddKnownVariable(uint address, int length, string? variableName = null)
        {
            KnownVariables.TryAdd(address, new RomVariable(address, length, variableName));
        }

        public async Task DisassembleAsync(DirectoryInfo outputDir, bool splitFiles, CancellationToken cancellationToken = default)
        {
            Objects.Clear();
            Functions.Clear();
            Variables.Clear();
            Labels.Clear();
            SwitchCaseAddresses.Clear();

            cancellationToken.ThrowIfCancellationRequested();
            foreach (var func in KnownFunctions)
            {
                Functions.Add(func.Key, func.Value);
            }

            cancellationToken.ThrowIfCancellationRequested();
            foreach (var obj in KnownObjects)
            {
                Objects.Add(obj.Key, obj.Value);
                if (IsAddressInCodeRegion(obj.Key))
                {
                    // Assume every object starts with a function.
                    Functions.Add(obj.Key, new RomFunction(obj.Key, null));
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            foreach (var addr in KnownVariables)
            {
                AddVariable(addr.Key, addr.Value);
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (!outputDir.Exists)
            {
                outputDir.Create();
            }

            foreach (var segment in DataSegments)
            {
                if (string.IsNullOrWhiteSpace(segment.Value.SaveFileName))
                {
                    continue;
                }

                using (var fs = outputDir.CombineFile(segment.Value.SaveFileName).Open(FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await fs.WriteAsync(segment.Value.Data, cancellationToken);
                    await fs.FlushAsync(cancellationToken);
                }
            }

            RunFirstPass();

            var codeOutputDir = outputDir.CombineDirectory("src");
            codeOutputDir.Create();

            await RunSecondPassAsync(codeOutputDir, splitFiles, cancellationToken);
        }

        private void AddVariable(uint addr, in RomVariable value)
        {
            // Known special case that is mis-identified as a variable.
            // It is actually in the middle of a pointer.
            if (addr == 0x80AAB3AE)
            {
                return;
            }

            Variables.Add(addr, value);
        }

        private void AddVariable(uint addr, int size)
            => AddVariable(addr, new RomVariable(addr, size, null));

        private void ClearCaches()
        {
            DataRegionAddressCache.Clear();
            CodeAddressCache.Clear();
        }

        private (uint Nearest, uint Offset) GetVariableOffset(uint addr)
        {
            if (Variables.Count == 0)
            {
                return (0, 0);
            }

            if (Variables.ContainsKey(addr))
            {
                return (addr, 0);
            }

            // Binary search for the closest.
            uint[] keys = Variables.Keys.ToArray();
            int first = 0;
            int last = Variables.Count - 1;
            bool exact = false;
            int mid;

            do
            {
                mid = first + (last - first) / 2;
                if (addr > keys[mid])
                {
                    first = mid + 1;
                }
                else
                {
                    last = mid - 1;
                }
                if (keys[mid] == addr)
                {
                    exact = true;
                    break;
                }
            } while (first <= last);

            var nearest = Variables[keys[mid + (!exact).ToInt()]];
            uint offset = addr - nearest.Address;
            if (offset < nearest.Address)
            {
                return (nearest.Address, offset);
            }

            return (0, 0);
        }

        private string GetVariableName(uint addr)
        {
            if (IsAddressInDataRegionOrUndefined(addr))
            {
                return GetSymbolName(addr);
            }

            return GetFunctionName(addr);
        }

        private string GetSymbolName(uint addr)
        {
            return KnownVariables.TryGetValue(addr, out var variable) && !string.IsNullOrWhiteSpace(variable.KnownName)
                ? variable.KnownName
                : $"Symbol_0x{addr:X}";
        }

        private string GetFunctionName(uint addr)
        {
            return KnownFunctions.TryGetValue(addr, out var function)
                ? function.GetName()
                : RomFunction.GenerateName(addr);
        }

        private string GetObjectName(uint addr, uint fileAddr, bool splitFiles)
        {
            string? filename = null;
            foreach (var segment in DataSegments)
            {
                if (fileAddr == segment.Value.VirtualAddress)
                {
                    filename = segment.Value.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException($"{nameof(fileAddr)} [0x{fileAddr:X}] does not point to a valid data segment.");
            }

            if (splitFiles)
            {
                if (KnownObjects.TryGetValue(addr, out var obj) && !string.IsNullOrWhiteSpace(obj.Name))
                {
                    return obj.Name;
                }
                return $"{filename}_0x{addr:X}";
            }

            return filename;
        }

        private bool IsAddressInDataRegion(uint addr)
        {
            if (DataRegionAddressCache.TryGetValue(addr, out bool result))
            {
                return result;
            }

            foreach (var region in DataRegions)
            {
                if (addr.IsBetween(region.Value.StartAddress, region.Value.EndAddress))
                {
                    DataRegionAddressCache.Add(addr, true);
                    return true;
                }
            }

            DataRegionAddressCache.Add(addr, false);
            return false;
        }

        private bool IsAddressInCodeRegion(uint addr)
        {
            if (CodeAddressCache.TryGetValue(addr, out bool result))
            {
                return result;
            }

            foreach (var file in DataSegments)
            {
                uint startAddr = file.Value.VirtualAddress;
                if (addr.IsBetween(startAddr, startAddr + (uint)file.Value.Data.Length))
                {
                    result = !IsAddressInDataRegion(addr);
                    CodeAddressCache.Add(addr, result);
                    return result;
                }
            }

            CodeAddressCache.Add(addr, false);
            return false;
        }

        private bool IsAddressInDataRegionOrUndefined(uint addr)
        {
            if (IsAddressInDataRegion(addr))
            {
                return true;
            }

            if (IsAddressInCodeRegion(addr))
            {
                return false;
            }

            return true;
        }

        private void RunFirstPass()
        {
            foreach (var dataSegment in DataSegments)
            {
                var reader = new SpanReader(dataSegment.Value.Data.Span);
                for (int instructionOffset = 0; instructionOffset < dataSegment.Value.Data.Length / 4; ++instructionOffset)
                {
                    reader.TryReadUInt32(instructionOffset * 4, out uint instruction);
                    uint addr = dataSegment.Value.VirtualAddress + (uint)instructionOffset * 4;
                    if (!IsAddressInDataRegionOrUndefined(addr))
                    {
                        DisassembleInstruction(instruction, addr, instructionOffset, dataSegment.Value);

                        if (instruction == 0x03E00008)
                        {
                            int nextIndex = instructionOffset + 2;
                            if (reader.TryReadUInt32(nextIndex, out uint nop) && nop == 0)
                            {
                                while (reader.TryReadUInt32(nextIndex, out nop) && nop == 0)
                                {
                                    ++nextIndex;
                                }

                                uint newObjectStart = dataSegment.Value.VirtualAddress + (uint)nextIndex * 4 + 15;
                                newObjectStart -= newObjectStart % 16;

                                // Don't split if it's the start of a data section. It may be the same object.
                                if (!IsAddressInDataRegionOrUndefined(newObjectStart))
                                {
                                    Objects.Add(newObjectStart, new RomObject(newObjectStart, null, null));
                                }
                            }
                        }
                    }

                    if (Variables.ContainsKey(addr))
                    {
                        string name = GetVariableName(addr);
                        if (name.StartsWith("__switch"))
                        {
                            uint addr2 = addr;
                            uint caseAddr = reader.ReadUInt32((int)addr2);
                            while (IsAddressInCodeRegion(caseAddr))
                            {
                                SwitchCaseAddresses.Add(caseAddr);
                                ++addr2;

                                if (addr2 >= dataSegment.Value.Data.Length / 4)
                                {
                                    break;
                                }
                                caseAddr = reader.ReadUInt32((int)addr2);
                            }
                        }
                    }

                    // Try to "smartly" get functions and variables from the data.
                    if (IsAddressInDataRegion(addr))
                    {
                        if (IsAddressInCodeRegion(instruction) && instruction % 4 == 0)
                        {
                            // TODO: Functions are disabled for now due to poor behavior with switches.
                            Functions.Add(instruction, new RomFunction(instruction, null));
                        }
                        else if (IsAddressInDataRegion(instruction))
                        {
                            AddVariable(instruction, 1);
                        }
                    }
                }
            }
        }

        private async Task RunSecondPassAsync(DirectoryInfo outputDir, bool splitFiles, CancellationToken cancellationToken)
        {
            foreach (var file in outputDir.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (var dataSegment in DataSegments)
            {
                var outputFile = outputDir.CombineFile($"{GetObjectName(dataSegment.Value.VirtualAddress, dataSegment.Value.VirtualAddress, splitFiles)}.asm");

                FileStream? fs = null;
                try
                {
                    fs = outputFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
                    await WriteHeaderAsync(fs, cancellationToken);

                    var reader = new MemoryReader(dataSegment.Value.Data);
                    for (int instructionOffset = 0; instructionOffset < dataSegment.Value.Data.Length / 4; ++instructionOffset)
                    {
                        reader.TryReadUInt32(instructionOffset * 4, out uint instruction);
                        uint addr = dataSegment.Value.VirtualAddress + (uint)instructionOffset * 4;

                        if (Objects.TryGetValue(addr, out var obj) && splitFiles)
                        {
                            await fs.FlushAsync();
                            await fs.DisposeAsync();
                            fs = null;
                            outputFile = outputDir.CombineFile($"{GetObjectName(addr, dataSegment.Value.VirtualAddress, splitFiles)}");
                            fs = outputFile.Open(FileMode.Append, FileAccess.Write, FileShare.Read);
                            await WriteHeaderAsync(fs, cancellationToken);
                        }

                        await using var writer = fs.GetStreamWriter();
                        if (Labels.TryGetValue(addr, out var label) && !SwitchCaseAddresses.Contains(addr))
                        {
                            await writer.WriteLineAsync($"{label.Name}:", cancellationToken);
                        }
                        if (SwitchCaseAddresses.Contains(addr))
                        {
                            await writer.WriteLineAsync($"glabel .L_{addr:X}", cancellationToken);
                        }
                        if (Functions.TryGetValue(addr, out var function))
                        {
                            await writer.WriteLineAsync();
                            await writer.WriteLineAsync($"glabel {function.GetName()}", cancellationToken);
                        }

                        if (!IsAddressInDataRegionOrUndefined(addr))
                        {
                            await writer.WriteLineAsync($"/* {instructionOffset} 0x{addr:X} {instruction:X} */ {DisassembleInstruction(instruction, addr, instructionOffset, dataSegment.Value)}", cancellationToken);
                        }
                        else if (Functions.ContainsKey(instruction))
                        {
                            if (Variables.ContainsKey(addr))
                            {
                                await writer.WriteLineAsync($"glabel {GetVariableName(addr)}", cancellationToken);
                            }
                            await writer.WriteLineAsync($"/* {instructionOffset} 0x{addr} */ .word\t{GetFunctionName(instruction)}", cancellationToken);
                        }
                        else if (SwitchCaseAddresses.Contains(instruction))
                        {
                            if (Variables.ContainsKey(addr))
                            {
                                await writer.WriteLineAsync($"glabel {GetVariableName(addr)}", cancellationToken);
                            }
                            await writer.WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .word\tL_{instruction:X}", cancellationToken);
                        }
                        else if (Variables.ContainsKey(instruction))
                        {
                            if (Variables.ContainsKey(addr))
                            {
                                await writer.WriteLineAsync($"glabel {GetVariableName(addr)}", cancellationToken);
                            }
                            await writer.WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .word\t{GetVariableName(instruction)}", cancellationToken);
                        }
                        else if (instruction.IsBetween(0x801F0568, 0x801F0684, EInclusivity.Inclusive, EInclusivity.Exclusive))
                        {
                            // Special case gSaveContext.weekEventReg because there are pointers to fields of it in other parts
                            if (Variables.ContainsKey(addr))
                            {
                                await writer.WriteLineAsync($"glabel {GetVariableName(addr)}", cancellationToken);
                            }
                            var offset = GetVariableOffset(instruction);
                            await writer.WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .word\t({GetVariableName(offset.Nearest)} + 0x{offset.Offset})", cancellationToken);
                        }
                        else
                        {
                            uint printHead = addr;
                            uint dataStream = instruction;
                            while (printHead < addr + 4)
                            {
                                if (Variables.ContainsKey(printHead))
                                {
                                    await writer.WriteLineAsync($"glabel {GetVariableName(printHead)}", cancellationToken);
                                }
                                if (Variables.ContainsKey(printHead + 1) || printHead % 2 != 0)
                                {
                                    await writer.WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .byte\t{GetHex((dataStream >> 24) & 0xFF, 2)}", cancellationToken);
                                    dataStream <<= 8;
                                    ++printHead;
                                }
                                else if (Variables.ContainsKey(printHead + 2) || Variables.ContainsKey(printHead + 3) || printHead % 4 != 0)
                                {
                                    await writer.WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .short\t{GetHex((dataStream >> 16) & 0xFFFF, 4)}", cancellationToken);
                                    dataStream <<= 16;
                                    printHead += 2;
                                }
                                else
                                {
                                    await writer.WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .word\t{GetHex(dataStream & 0xFFFFFFFF, 8)}", cancellationToken);
                                    dataStream <<= 16;
                                    printHead += 2;
                                }
                            }
                        }

                        await writer.FlushAsync();
                    }
                }
                finally
                {
                    if (fs is not null)
                    {
                        await fs.FlushAsync(cancellationToken);
                        await fs.DisposeAsync();
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetHex(uint value, int totalLength)
        {
            string result = value.ToString("X");
            return "0x" + result.PadLeft(totalLength, '0');
        }

        private async Task WriteHeaderAsync(Stream stream, CancellationToken cancellationToken)
        {
            using var sw = stream.GetStreamWriter();

            var headerBuilder = new StringBuilder();
            headerBuilder.AppendLine(".set noat # allow use of $at");
            headerBuilder.AppendLine(".set noreorder # disallow insertion of nop after branches");
            headerBuilder.AppendLine(".set gp=64 # allow use of 64bit registers");
            headerBuilder.AppendLine(".macro glabel label");
            headerBuilder.AppendLine("    .global \\label");
            headerBuilder.AppendLine("    \\label:");
            headerBuilder.AppendLine(".endm");
            headerBuilder.AppendLine();

            await sw.WriteAsync(headerBuilder, cancellationToken);
            await sw.FlushAsync();
        }

        private string DisassembleInstruction(uint instruction, uint addr, int instructionOffset, DisassemblerDataSegment value)
        {
            if (instruction == 0)
            {
                return "nop";
            }

            uint instCode = ToInstruction(instruction);
            var output = new StringBuilder();

            if (instCode == 0)
            {
                uint funcCode = ToFunction(instruction);
                if (funcCode == 1)
                {
                    uint cc = (instruction & (7 << 18)) >> 18;
                    if ((instruction & (1 << 16)) == 0)
                    {
                        output.Append("movf\t").Append(Registers[ToRD(instruction)]).Append(", ").Append(Registers[ToRS(instruction)]).Append(", ").Append(cc).AppendLine();
                    }
                    else
                    {
                        output.Append("movf\t").Append(Registers[ToRD(instruction)]).Append(", ").Append(Registers[ToRS(instruction)]).Append(", ").Append(cc).AppendLine();
                    }
                }
                else
                {
                    if (!FunctionCode.TryGetValue(funcCode, out string? funcName))
                    {
                        output.Append("/* func_error: ").Append(funcCode).AppendLine(" */");
                        output.Append("/* 0x").AppendFormat("{0:X}", instructionOffset).Append(" 0x").AppendFormat("{0:X}", addr).Append(" */ .word\t0x").AppendFormat("{0:X}", instruction).AppendLine();
                    }
                    else
                    {
                        if (funcCode == FunctionCode.or && ToRT(instruction) == 0)
                        {
                            // Or with 0 reg is move
                            output.Append("move\t").Append(Registers[ToRD(instruction)]).Append(", ").AppendLine(Registers[ToRS(instruction)]);
                        }

                        output.Append(funcName).Append('\t');
                        switch (funcCode)
                        {
                            case FunctionCode.sll:
                            case FunctionCode.srl:
                            case FunctionCode.sra:
                            case FunctionCode.dsll:
                            case FunctionCode.dsra:
                            case FunctionCode.dsll32:
                            case FunctionCode.dsra32:
                                output.Append(Registers[ToRD(instruction)]).Append(", ").Append(Registers[ToRT(instruction)]).Append(", ").Append(ToShift(instruction)).AppendLine();
                                break;
                            case FunctionCode.sllv:
                            case FunctionCode.srlv:
                            case FunctionCode.srav:
                                output.Append(Registers[ToRD(instruction)]).Append(", ").Append(Registers[ToRT(instruction)]).Append(", ").AppendLine(Registers[ToRS(instruction)]);
                                break;
                            case FunctionCode.jr:
                            case FunctionCode.jalr:
                                output.AppendLine(Registers[ToRS(instruction)]);
                                break;
                            case FunctionCode.@break:
                                output.Append("0x").AppendFormat("{0:X}", (instruction & (0xFFFFF << 6)) >> 16).AppendLine();
                                break;
                            case FunctionCode.mfhi:
                            case FunctionCode.mflo:
                                output.AppendLine(Registers[ToRD(instruction)]);
                                break;
                            case FunctionCode.mthi:
                            case FunctionCode.mtlo:
                                output.Append(Registers[ToRS(instruction)]);
                                break;
                            case FunctionCode.mult:
                            case FunctionCode.multu:
                            case FunctionCode.dmult:
                            case FunctionCode.dmultu:
                            case FunctionCode.div:
                            case FunctionCode.divu:
                            case FunctionCode.ddiv:
                            case FunctionCode.ddivu:
                                output.Append(Registers[ToRS(instruction)]).Append(", ").AppendLine(Registers[ToRT(instruction)]);
                                break;
                            case FunctionCode.sub when ToRS(instruction) == 0:
                            case FunctionCode.subu when ToRS(instruction) == 0:
                                output.Append(Registers[ToRD(instruction)]).Append(", ").AppendLine(Registers[ToRT(instruction)]);
                                break;
                            case FunctionCode.dsllv:
                            case FunctionCode.dsrlv:
                            case FunctionCode.dsrav:
                                output.Append(Registers[ToRD(instruction)]).Append(", ").Append(Registers[ToRT(instruction)]).Append(", ").AppendLine(Registers[ToRS(instruction)]);
                                break;
                            default:
                                output.Append(Registers[ToRD(instruction)]).Append(", ").Append(Registers[ToRS(instruction)]).Append(", ").AppendLine(Registers[ToRT(instruction)]);
                                break;
                        }
                    }
                }
            }
            else if (instCode == 1)
            {
                // https://github.com/N64RET/decomp-framework/blob/e74b13e365deae31dd1233642753af008bd2e1cf/N64RET/Processor/MIPSR/Disassembler/DisasmImpl.py#L810
            }
            else if (instCode == 16 || instCode == 17 || instCode == 18)
            {

            }
            /*
            else if (instCode not in ops)
            {
                ops = {
    2:"j", 3:"jal", 4:"beq", 5:"bne", 6:"blez", 7:"bgtz",
    8:"addi", 9:"addiu", 10:"slti", 11:"sltiu", 12:"andi", 13:"ori", 14:"xori", 15:"lui",
    20:"beql", 21:"bnel", 22:"blezl", 23:"bgtzl",
    24:"daddi", 25:"daddiu",
    32:"lb", 33:"lh", 34:"lwl", 35:"lw", 36:"lbu", 37:"lhu", 38:"lwr",
    40:"sb", 41:"sh", 42:"swl", 43:"sw", 46:"swr", 47:"cache",
    49:"lwc1", 55:"ld", 51:"pref", 48:"ll", 50:"lwc2", 53:"ldc1", 54:"ldc2",
    57:"swc1", 61:"sdc1", 63:"sd", 56:"sc", 58:"swc2", 62:"sdc2"
    }
            }*/
            else
            {

            }

            return output.ToString();
        }

    }
}
