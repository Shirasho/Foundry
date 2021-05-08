using System;
using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Foundry.Media.Nintendo64.Rom.Disassembly.Internal;
using Microsoft.Toolkit.HighPerformance.Helpers;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    public sealed partial class Disassembler
    {
        private readonly ISet<DataFile> DataFiles = new SortedSet<DataFile>(Comparer<DataFile>.Create((a, b) => a.VirtualAddress.CompareTo(b.VirtualAddress)));
        private readonly ISet<DataRegion> DataRegions = new SortedSet<DataRegion>(Comparer<DataRegion>.Create((a, b) => a.StartAddress.CompareTo(b.StartAddress)));

        private readonly IDictionary<uint, bool> DataRegionAddressCache = new ConcurrentDictionary<uint, bool>();
        private readonly IDictionary<uint, bool> CodeRegionAddressCache = new ConcurrentDictionary<uint, bool>();
        private readonly IDictionary<uint, (uint, uint)> LoadHighRefCache = new ConcurrentDictionary<uint, (uint, uint)>();
        private readonly IDictionary<uint, (uint, uint)> LoadLowRefCache = new ConcurrentDictionary<uint, (uint, uint)>();

        private readonly IDictionary<uint, RomLabel> Labels = new Dictionary<uint, RomLabel>();
        private readonly IDictionary<uint, RomFunction> Functions = new Dictionary<uint, RomFunction>();
        private readonly IDictionary<uint, RomObject> Objects = new Dictionary<uint, RomObject>();
        private readonly IDictionary<uint, RomVariable> Variables = new SortedDictionary<uint, RomVariable>();

        private readonly ISet<uint> SwitchCases = new HashSet<uint>();

        public DisassemblyResult Disassemble(DisassemblerOptions options)
        {
            SetEnvironment(options);

            try
            {
                // First pass - extract metadata.
                foreach (var file in DataFiles)
                {
                    ParallelHelper.For(0, file.Data.Length / 4, new MetadataExtractor(this, file), 100);
                }
            }
            catch (Exception e)
            {
                ClearEnvironment();
                return new DisassemblyResult(e);
            }


            var outputDir = options.OutputDir?.CombineDirectory("src") ?? new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "src"));
            try
            {
                if (!outputDir.Exists)
                {
                    outputDir.Create();
                }
                else
                {
                    outputDir.DeleteChildren();
                }

                int expectedOperationCount = DataFiles.Sum(f => f.Data.Length / 4);
                var operations = new List<Operation>(expectedOperationCount);

                // Second pass - create instructions.
                foreach (var file in DataFiles)
                {
                    MipsWriter? writer = null;
                    try
                    {
                        writer = new MipsWriter(outputDir.CombineFile($"{GetObjectName(file, options.SplitFiles)}.asm").OpenWrite());
                        writer.WriteHeader();

                        int length = file.Data.Length / 4;
                        for (int index = 0; index < length; ++index)
                        {
                            uint addressOffset = (uint)index;
                            uint rawInstruction = BinaryPrimitives.ReadUInt32BigEndian(file.Data.Slice(index * 4, 4).Span);
                            uint virtualAddress = file.VirtualAddress + addressOffset * 4;
                            var instruction = new Instruction(rawInstruction, virtualAddress, addressOffset);

                            if (Objects.ContainsKey(virtualAddress))
                            {
                                writer.Dispose();
                                writer = new MipsWriter(outputDir.CombineFile($"{GetObjectName(virtualAddress, file.VirtualAddress, options.SplitFiles)}.asm").OpenWrite());
                                writer.WriteHeader();
                            }

                            if (Labels.TryGetValue(virtualAddress, out var label) && !SwitchCases.Contains(virtualAddress))
                            {
                                writer.WriteLabel(virtualAddress, label.Name);

                                // We do not care about label writes in the operation result set.
                            }

                            if (SwitchCases.Contains(virtualAddress))
                            {
                                writer.WriteSwitchCase(virtualAddress);

                                // We do not care about switch case writes in the operation result set (for now).
                            }

                            if (Functions.TryGetValue(virtualAddress, out var function))
                            {
                                writer.WriteFunction(function);

                                // We do not care about function name writes in the operation result set (for now).
                            }

                            if (!IsAddressInDataRegionOrUndefined(virtualAddress))
                            {
                                var operation = DisassembleInstruction(instruction, file);
                                if (operation.Code == EOperationCode.Reserved)
                                {
                                    // TODO: I do not know how to handle reserved values.
                                    // Looking at MUPEN these instructions set the r4300_core stop value to 1,
                                    // but I don't yet understand what this flag does.
                                    // There are quite a few of these and the test ROMs run fine, so more research
                                    // needs to be done to figure out the best path here.
                                }

                                // TODO: Include help text.
                                var result = writer.WriteLine($"/* 0x{addressOffset:X5} 0x{virtualAddress:X8} 0x{instruction.Value:X8} */ {operation.GetMIPSString()}");
                                if (!result.ValidationResult)
                                {
                                    // TODO: Come up with better error text.
                                    operations.Add(new Operation(operation.Code, operation.Instruction, operation.File, operation.MIPSStringDelegate, new Exception("The instruction contains invalid logic.")));
                                }
                                else
                                {
                                    operations.Add(operation);
                                }
                            }
                            else if (Functions.TryGetValue(instruction.Value, out function))
                            {
                                if (Variables.ContainsKey(virtualAddress))
                                {
                                    // We do not care about function name writes in the operation result set (for now).
                                    writer.WriteFunction(GetLoadName(virtualAddress));
                                }

                                writer.WriteLine($"/* 0x{addressOffset:X5} 0x{virtualAddress:X8} 0x{instruction.Value:X8} */ .word\t{function.Name}");
                            }
                            else if (SwitchCases.Contains(instruction.Value))
                            {
                                if (Variables.ContainsKey(virtualAddress))
                                {
                                    // We do not care about function name writes in the operation result set (for now).
                                    writer.WriteFunction(GetLoadName(virtualAddress));
                                }

                                writer.WriteLine($"/* 0x{addressOffset:X5} 0x{virtualAddress:X8} 0x{instruction.Value:X8} */ .word\t.L{instruction.Value:X8}");
                            }
                            else if (Variables.ContainsKey(instruction.Value))
                            {
                                if (Variables.ContainsKey(virtualAddress))
                                {
                                    // We do not care about function name writes in the operation result set (for now).
                                    writer.WriteFunction(GetLoadName(virtualAddress));
                                }

                                writer.WriteLine($"/* 0x{addressOffset:X5} 0x{virtualAddress:X8} 0x{instruction.Value:X8} */ .word\t{GetLoadName(instruction.Value)}");
                            }
                            else if (instruction.Value.IsBetween(0x801F0568, 0x801F0684, EInclusivity.Inclusive, EInclusivity.Exclusive))
                            {
                                if (Variables.ContainsKey(virtualAddress))
                                {
                                    // We do not care about function name writes in the operation result set (for now).
                                    writer.WriteFunction(GetLoadName(virtualAddress));
                                }

                                var offset = GetVariableOffset(instruction.Value);
                                writer.WriteLine($"/* 0x{addressOffset:X5} 0x{virtualAddress:X8} 0x{instruction.Value:X8} */ .word\t({FormatOffset(GetLoadName(offset.Nearest), offset.Offset)})");
                            }
                            else
                            {
                                uint printHead = virtualAddress;
                                uint dataStream = instruction.Value;
                                while (printHead < virtualAddress + 4)
                                {
                                    if (Variables.ContainsKey(printHead))
                                    {
                                        writer.WriteFunction(GetLoadName(printHead));
                                    }

                                    if (Variables.ContainsKey(printHead + 1) || printHead % 2 != 0)
                                    {
                                        writer.WriteLine($"/* 0x{addressOffset:X5} 0x{virtualAddress:X8} 0x{instruction.Value:X8} */ .byte\t0x{(dataStream >> 24) & 0xFF:X2}");
                                        dataStream <<= 8;
                                        printHead += 1;
                                    }
                                    else if (Variables.ContainsKey(printHead + 2) || Variables.ContainsKey(printHead + 3) || printHead % 4 != 0)
                                    {
                                        writer.WriteLine($"/* 0x{addressOffset:X5} 0x{virtualAddress:X8} 0x{instruction.Value:X8} */ .short\t0x{(dataStream >> 16) & 0xFFFF:X4}");
                                        dataStream <<= 16;
                                        printHead += 2;
                                    }
                                    else
                                    {
                                        writer.WriteLine($"/* 0x{addressOffset:X5} 0x{virtualAddress:X8} 0x{instruction.Value:X8} */ .byte\t0x{dataStream & 0xFFFFFFFF:X8}");
                                        dataStream <<= 32;
                                        printHead += 4;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        writer?.Dispose();
                    }
                }

                return new DisassemblyResult(operations);
            }
            catch (Exception e)
            {
                if (outputDir.Exists)
                {
                    outputDir.Delete(true);
                }
                return new DisassemblyResult(e);
            }
            finally
            {
                ClearEnvironment();
            }
        }

        private void SetEnvironment(DisassemblerOptions options)
        {
            ClearEnvironment();

            foreach (var file in options.KnownFiles)
            {
                var dataFile = new DataFile(file.Name, file.GetFileData(), file.VirtualAddress);
                if (!DataFiles.Add(dataFile))
                {
                    throw new InvalidOperationException($"Multiple files share the virtual address 0x{file.VirtualAddress:X}.");
                }

                // Assume every file starts with an object and a function.
                if (!Objects.TryAdd(file.VirtualAddress, new RomObject(file.VirtualAddress)))
                {
                    throw new InvalidOperationException($"Multiple objects share the virtual address 0x{file.VirtualAddress:X}.");
                }
                if (!Functions.TryAdd(file.VirtualAddress, new RomFunction(file.VirtualAddress, options.KnownFunctions.TryGetValue(file.VirtualAddress, out string? name) ? name : null)))
                {
                    throw new InvalidOperationException($"Multiple functions share the virtual address 0x{file.VirtualAddress:X}.");
                }
            }

            foreach (var func in options.KnownFunctions)
            {
                if (!Functions.TryAdd(func.Key, new RomFunction(func.Key, func.Value)))
                {
                    //TODO: Log warning.
                }
            }

            foreach (var obj in options.KnownObjects)
            {
                if (!Objects.TryAdd(obj.Key, new RomObject(obj.Key, obj.Value)))
                {
                    //TODO: Log warning.
                }
                if (IsAddressInCodeRegion(obj.Key))
                {
                    // We assume every object starts with a function.
                    if (!Functions.TryAdd(obj.Key, new RomFunction(obj.Key, obj.Value)))
                    {
                        //TODO: Log warning.
                    }
                }
            }

            foreach (var v in options.KnownVariables)
            {
                if (!Variables.TryAdd(v.Key, new RomVariable(v.Key, v.Value)))
                {
                    //TODO: Log warning.
                }
            }

            DataRegions.AddRange(options.Regions);
        }

        private void ClearEnvironment()
        {
            ClearCaches();

            SwitchCases.Clear();

            Labels.Clear();
            Functions.Clear();
            Objects.Clear();
            Variables.Clear();
            SwitchCases.Clear();
        }

        private string? GetFunctionName(uint addr)
        {
            return Functions.TryGetValue(addr, out var function) && !string.IsNullOrWhiteSpace(function.Name)
                ? function.Name
                : null;
        }

        private string? GetVariableName(uint addr)
        {
            return Variables.TryGetValue(addr, out var variable) && !string.IsNullOrWhiteSpace(variable.Name)
                ? variable.Name
                : null;
        }

        private string GetLoadName(uint addr)
        {
            if (IsAddressInDataRegionOrUndefined(addr))
            {
                return GetVariableName(addr) ?? RomVariable.MakeName(addr);
            }

            return GetFunctionName(addr) ?? RomFunction.MakeName(addr);
        }

        private string GetObjectName(DataFile file, bool splitFiles)
        {
            return GetObjectName(file.Name, file.VirtualAddress, file.VirtualAddress, splitFiles);
        }

        private string GetObjectName(uint addr, uint fileAddr, bool splitFiles)
        {
            string? file = null;
            foreach (var f in DataFiles)
            {
                if (f.VirtualAddress == fileAddr)
                {
                    file = f.Name;
                    break;
                }
            }

            if (file is null)
            {
                throw new InvalidOperationException("Unable to find file for object.");
            }

            return GetObjectName(file, addr, fileAddr, splitFiles);
        }

        private string GetObjectName(string filename, uint addr, uint fileAddr, bool splitFiles)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new InvalidOperationException($"Bad file pointed used to get object name - {fileAddr}");
            }

            if (splitFiles)
            {
                if (Objects.TryGetValue(addr, out var obj) && !string.IsNullOrWhiteSpace(obj.Name))
                {
                    return obj.Name;
                }

                return $"{filename}_{addr}";
            }

            return filename;
        }

        private string FormatOffset(string addrName, uint offset)
        {
            return offset == 0
                ? addrName
                : $"{addrName} + 0x{offset:X8}";
        }

        private void DetermineLoadRef(DataFile segment, uint addressOffset)
        {
            var candidates = DetermineLoadRefImpl(segment, addressOffset, addressOffset, 200);
            if (candidates.Count > 0)
            {
                var first = candidates[0];
                LoadHighRefCache.TryAdd(segment.VirtualAddress + addressOffset * 4, (first.Nearest, first.Offset));
                foreach (var c in candidates)
                {
                    LoadLowRefCache.TryAdd(c.Address, (c.Nearest, c.Offset));
                }
            }
        }

        private IList<(uint Address, uint Nearest, uint Offset)> DetermineLoadRefImpl(DataFile segment, uint addressOffset, uint addressOffsetStart, int depth, bool fromBranch = false, ISet<uint>? visited = null)
        {
            var candidates = new List<(uint, uint, uint)>();
            visited ??= new HashSet<uint>();

            if (!fromBranch)
            {
                visited.Clear();
            }

            if (depth <= 0)
            {
                return candidates;
            }

            uint currentInstance = segment.Read(addressOffset);
            uint addrHigh = Instruction.Mask.GetIMM(currentInstance);

            if (addrHigh < 0x8000 || addrHigh > 0x80C2)
            {
                return candidates;
            }

            if (Instruction.Mask.GetOpCode(currentInstance) != (uint)EOperationCode.Lui)
            {
                return candidates;
            }

            bool prevWasJump;
            bool prevWasRet;
            bool prevWasBranch;
            bool prevWasBranchF;
            bool prevWasBranchFLikely = false;
            bool prevWasBranchLikely = false;
            bool prevWasBranchAlways = false;
            uint prevTarget;
            uint prevInstance;
            uint prevOpCode;

            if (!(addressOffsetStart == 0 || fromBranch))
            {
                prevInstance = segment.Read(addressOffsetStart);
                prevOpCode = Instruction.Mask.GetOpCode(prevInstance);
                //prevWasJump = prevOpCode == EOperationCode.J ||
                //              prevOpCode == EOperationCode.Jal;
                //prevWasRet = prevOpCode == EOperationCode.Spec_RType && ((InstructionR)prevInstance).Funct == 8;
                prevWasBranch = prevOpCode == 4 || prevOpCode == 5 || prevOpCode == 6 || prevOpCode == 7 ||
                                (prevOpCode == 1 && (Instruction.Mask.GetRT(prevInstance) == 0 || Instruction.Mask.GetRT(prevInstance) == 1));
                prevWasBranchF = (prevOpCode == 16 || prevOpCode == 17 || prevOpCode == 18) &&
                                  Instruction.Mask.GetRS(prevInstance) == 8 &&
                                  ((prevInstance & (1 << 17)) == 0);
                prevWasBranchFLikely = (prevOpCode == 16 || prevOpCode == 17 || prevOpCode == 18) &&
                                  Instruction.Mask.GetRS(prevInstance) == 8 &&
                                  ((prevInstance & (1 << 17)) != 0);
                //prevWasBranchAlways = prevOpCode == EOperationCode.Beq &&
                //                      Instruction.Mask.GetRS(prevInstance) == Instruction.Mask.GetRT(prevInstance) &&
                //                      Instruction.Mask.GetRS(prevInstance) == 0;
                prevWasBranchLikely = prevOpCode == 20 || prevOpCode == 21 || prevOpCode == 22 || prevOpCode == 23 ||
                                      (prevOpCode == 1 && (Instruction.Mask.GetRT(prevInstance) == 2 || Instruction.Mask.GetRT(prevInstance) == 3));
                prevTarget = (uint)(Instruction.Mask.GetIMMSigned(prevInstance) * 4 + (segment.VirtualAddress + (addressOffsetStart - 1) * 4) + 4);

                if (prevWasBranch || prevWasBranchF || prevWasBranchLikely || prevWasBranchFLikely)
                {
                    var newCandidates = DetermineLoadRefImpl(segment, addressOffset, (prevTarget - segment.VirtualAddress) / 4, depth, true, visited);
                    candidates.AddRange(newCandidates);
                }
            }

            bool continueBranch = !(prevWasBranchLikely || prevWasBranchFLikely);
            prevWasJump = false;
            prevWasRet = false;
            prevWasBranch = false;
            prevWasBranchF = false;
            prevWasBranchFLikely = false;
            prevWasBranchLikely = false;
            prevTarget = 0;
            uint readAddress = fromBranch ? addressOffsetStart : addressOffset + 1;

            foreach (int i in Enumerable.Range(1, depth + 1))
            {
                if (!continueBranch)
                {
                    break;
                }

                if (visited.Contains(readAddress))
                {
                    break;
                }

                if (IsAddressInDataRegion(segment.VirtualAddress + readAddress * 4))
                {
                    break;
                }

                if (readAddress + 4 >= segment.Length)
                {
                    break;
                }

                uint nextInstance = segment.Read(readAddress);
                uint nextOpCode = Instruction.Mask.GetOpCode(nextInstance);
                uint nextFuncCode = Instruction.Mask.GetFunct(nextInstance);

                if ((nextOpCode == 9 && Instruction.Mask.GetRT(currentInstance) == Instruction.Mask.GetRS(nextInstance)) ||
                    (nextOpCode > 31 && Instruction.Mask.GetRT(nextInstance) == Instruction.Mask.GetRS(nextInstance)))
                {
                    // lui + addiu (move pointer)
                    uint addr = (uint)((Instruction.Mask.GetIMM(currentInstance) << 16) + Instruction.Mask.GetIMMSigned(nextInstance));

                    // TODO: workaround to avoid classifying loading constants as loading pointers
                    // This unfortunately causes it to not detect object addresses
                    if (addr > 0x80000000)
                    {
                        var variableOffset = GetVariableOffset(addr);
                        if (variableOffset.Nearest == 0 && variableOffset.Offset == 0)
                        {
                            foreach (var f in Functions)
                            {
                                if (f.Key == addr)
                                {
                                    variableOffset = (addr, 0);
                                    break;
                                }
                            }
                        }

                        if (variableOffset.Nearest != 0 && variableOffset.Offset != 0)
                        {
                            candidates.Append((segment.VirtualAddress + readAddress * 4, variableOffset.Nearest, variableOffset.Offset));
                        }
                    }

                    if (Instruction.Mask.GetRT(currentInstance) == Instruction.Mask.GetRT(nextInstance))
                    {
                        if (prevWasBranchLikely || prevWasBranchFLikely)
                        {
                            prevWasBranchLikely = false;
                            prevWasBranchFLikely = false;
                        }
                        else
                        {
                            prevWasBranch = false;
                            prevWasBranchF = false;
                            prevWasBranchAlways = false;
                            continueBranch = false;
                        }
                    }
                }

                switch (nextOpCode)
                {
                    case 8 or 9 or 15 or (> 31 and < 48) when Instruction.Mask.GetRT(currentInstance) == Instruction.Mask.GetRS(nextInstance):
                    case 0 when (nextFuncCode == 0 || nextFuncCode == 2 || nextFuncCode == 3 || nextFuncCode == 24 ||
                                 nextFuncCode == 25 || nextFuncCode == 26 || nextFuncCode == 27 || nextFuncCode == 32 ||
                                 nextFuncCode == 33 || nextFuncCode == 34 || nextFuncCode == 35 || nextFuncCode == 36 ||
                                 nextFuncCode == 37 || nextFuncCode == 36 || nextFuncCode == 39) &&
                                 Instruction.Mask.GetRT(currentInstance) == Instruction.Mask.GetRD(nextInstance) &&
                                 Instruction.Mask.GetRT(currentInstance) != Instruction.Mask.GetRT(nextInstance) &&
                                 Instruction.Mask.GetRT(currentInstance) != Instruction.Mask.GetRS(nextInstance):
                    {
                        if (prevWasBranchLikely || prevWasBranchFLikely)
                        {
                            prevWasBranchLikely = false;
                            prevWasBranchFLikely = false;
                        }
                        else
                        {
                            prevWasBranch = false;
                            prevWasBranchF = false;
                            prevWasBranchAlways = false;
                            continueBranch = false;
                        }
                    }
                    break;
                }

                visited.Add(readAddress);

                if (prevWasBranch || prevWasBranchF || prevWasBranchLikely || prevWasBranchFLikely)
                {
                    candidates.AddRange(DetermineLoadRefImpl(segment, addressOffset, (prevTarget - segment.VirtualAddress) / 4, depth - i, true, visited));
                }

                // If this is a jump, mark to return after we evaluate the following instruction.
                if (prevWasJump || prevWasRet || prevWasBranchAlways)
                {
                    continueBranch = false;
                }

                prevWasJump = nextOpCode == 2 || nextOpCode == 3;
                prevWasRet = nextOpCode == 0 && Instruction.Mask.GetFunct(nextInstance) == 8;
                prevWasBranch = nextOpCode == 4 || nextOpCode == 5 || nextOpCode == 6 || nextOpCode == 7 ||
                               (nextOpCode == 1 && (Instruction.Mask.GetRT(nextInstance) == 0 || Instruction.Mask.GetRT(nextInstance) == 1));
                prevWasBranchF = (nextOpCode == 16 || nextOpCode == 17 || nextOpCode == 18) && Instruction.Mask.GetRS(nextInstance) == 8 && (nextInstance & (1 << 17)) != 0;
                prevWasBranchAlways = nextOpCode == 4 && Instruction.Mask.GetRS(nextInstance) == Instruction.Mask.GetRT(nextInstance) && Instruction.Mask.GetRT(nextInstance) == 0;
                prevWasBranchLikely = nextOpCode == 20 || nextOpCode == 21 || nextOpCode == 22 || nextOpCode == 23 || (nextOpCode == 1 && (Instruction.Mask.GetRT(nextInstance) == 2 || Instruction.Mask.GetRT(nextInstance) == 3));
                prevTarget = (uint)(Instruction.Mask.GetIMMSigned(nextInstance) * 4 + (segment.VirtualAddress + readAddress * 4) + 4);

                ++readAddress;
            }

            return candidates;
        }

        /// <summary>
        /// Returns whether <paramref name="addr"/> is within a data region registered
        /// in <see cref="DataRegions"/>.
        /// </summary>
        /// <param name="addr">The address to look up.</param>
        private bool IsAddressInDataRegion(uint addr)
        {
            if (DataRegionAddressCache.TryGetValue(addr, out bool result))
            {
                return result;
            }

            foreach (var region in DataRegions)
            {
                if (region.ContainsAddress(addr))
                {
                    DataRegionAddressCache.Add(addr, true);
                    return true;
                }
            }

            DataRegionAddressCache.Add(addr, false);
            return false;
        }

        /// <summary>
        /// Returns whether <paramref name="addr"/> is within a data region registered
        /// in <see cref="DataFiles"/> and is not in <see cref="DataRegions"/>.
        /// </summary>
        /// <param name="addr">The address to look up.</param>
        private bool IsAddressInCodeRegion(uint addr)
        {
            if (CodeRegionAddressCache.TryGetValue(addr, out bool result))
            {
                return result;
            }

            foreach (var file in DataFiles)
            {
                uint startAddr = file.VirtualAddress;
                if (addr.IsBetween(startAddr, startAddr + (uint)file.Data.Length))
                {
                    result = !IsAddressInDataRegion(addr);
                    CodeRegionAddressCache.Add(addr, result);
                    return result;
                }
            }

            CodeRegionAddressCache.Add(addr, false);
            return false;
        }

        /// <summary>
        /// Returns whether <paramref name="addr"/> is within a data region registered
        /// in <see cref="DataRegions"/> or is undefined (not registered with anything).
        /// </summary>
        /// <param name="addr">The address to look up.</param>
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
            uint[] addresses = Variables.Keys.ToArray();
            int first = 0;
            int last = Variables.Count - 1;
            bool exact = false;
            int mid;

            do
            {
                mid = first + (last - first) / 2;
                if (addr > addresses[mid])
                {
                    first = mid + 1;
                }
                else
                {
                    last = mid - 1;
                }
                if (addresses[mid] == addr)
                {
                    exact = true;
                    break;
                }
            } while (first <= last);

            uint nearest = addresses[mid + (!exact).ToInt()];
            uint offset = addr - nearest;
            if (offset < nearest)
            {
                return (nearest, offset);
            }

            return (0, 0);
        }

        private void ClearCaches()
        {
            CodeRegionAddressCache.Clear();
            DataRegionAddressCache.Clear();
            LoadHighRefCache.Clear();
            LoadLowRefCache.Clear();
        }
    }
}
