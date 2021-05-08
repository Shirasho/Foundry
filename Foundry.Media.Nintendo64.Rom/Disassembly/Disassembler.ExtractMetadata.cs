using System;
using System.Buffers.Binary;
using System.Linq;
using Foundry.Media.Nintendo64.Rom.Disassembly.Internal;
using Microsoft.Toolkit.HighPerformance.Helpers;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    public sealed partial class Disassembler
    {
        private readonly struct MetadataExtractor : IAction
        {
            private readonly Disassembler Disassembler;
            private readonly DataFile File;
            private readonly ReadOnlyMemory<byte> Data;

            public MetadataExtractor(Disassembler disassembler, DataFile file)
            {
                Disassembler = disassembler;
                File = file;
                Data = file.Data;
            }

            public void Invoke(int index)
            {
                uint addressOffset = (uint)index;
                uint rawInstruction = BinaryPrimitives.ReadUInt32BigEndian(Data.Slice(index * 4, 4).Span);
                uint virtualAddress = File.VirtualAddress + addressOffset * 4;
                var instruction = new Instruction(rawInstruction, virtualAddress, addressOffset);

                if (!Disassembler.IsAddressInDataRegionOrUndefined(virtualAddress))
                {
                    // REGIMM
                    if (instruction.OpCode == 1)
                    {
                        // Rely on compiler optimization.
                        ReadOnlySpan<byte> branchCodes = new byte[] { 0, 1, 2, 3, 8, 9, 10, 11, 12, 14, 16, 17, 18, 19 };
                        if (branchCodes.Contains((byte)instruction.RT))
                        {
                            // The jump target is a relative amount. This signed offset is stored in IMMSigned.
                            // We need to multiply by 4 (because each segment is 4 bits). We then add our virtual
                            // address to get our final jump offset. The instruction jumps to the instruction following
                            // the branch, not the branch itself, so we need to add 4.
                            // Here we convert it to an absolute jump.
                            uint jumpTarget = instruction.GetAbsoluteJumpAddress();

                            // If we have a jump target we need to create a label for it. Multiple locations can jump to a single label,
                            // so we only need to add the label to the global list once (using ISet).
                            var label = new RomLabel(jumpTarget);
                            if (Disassembler.Labels.TryGetValue(label.Address, out var existingLabel))
                            {
                                if (!existingLabel.Name.Equals(label.Name))
                                {
                                    throw new InvalidOperationException($"An attempt was made to declare label {label.Name} at pre-claimed address {label.Address} (existing label = {existingLabel.Name})");
                                }

                                // The label already exists. Nothing to see here.
                            }
                            else
                            {
                                lock (Disassembler.Labels)
                                {
                                    if (Disassembler.Labels.TryGetValue(label.Address, out existingLabel))
                                    {
                                        if (!existingLabel.Name.Equals(label.Name))
                                        {
                                            throw new InvalidOperationException($"An attempt was made to declare label {label.Name} at pre-claimed address {label.Address} (existing label = {existingLabel.Name})");
                                        }

                                        // The label already exists. Nothing to see here.
                                    }
                                    else
                                    {
                                        Disassembler.Labels.Add(label.Address, label);
                                    }
                                }
                            }
                        }
                    }
                    // J and JAL
                    else if (instruction.OpCode == 2 || instruction.OpCode == 3)
                    {
                        uint functionAddress = instruction.Address * 4 + (virtualAddress & 0xF0000000);
                        var function = new RomFunction(functionAddress, Disassembler.GetFunctionName(functionAddress));
                        if (Disassembler.Functions.TryGetValue(function.Address, out var existingFunction))
                        {
                            if (!existingFunction.Name.Equals(function.Name))
                            {
                                throw new InvalidOperationException($"An attempt was made to declare function {function.Name} at pre-claimed address {function.Address} (existing func = {existingFunction.Name})");
                            }

                            // The function already exists. Nothing to see here.
                        }
                        else
                        {
                            lock (Disassembler.Functions)
                            {
                                if (Disassembler.Functions.TryGetValue(function.Address, out existingFunction))
                                {
                                    if (!existingFunction.Name.Equals(function.Name))
                                    {
                                        throw new InvalidOperationException($"An attempt was made to declare function {function.Name} at pre-claimed address {function.Address} (existing func = {existingFunction.Name})");
                                    }

                                    // The function already exists. Nothing to see here.
                                }
                                else
                                {
                                    Disassembler.Functions.Add(function.Address, function);
                                }
                            }
                        }
                    }
                    // BEQ, BNE, BLEZ, BGTZ, BEQL, BNEL, BLEZL, BGTZL
                    else if (instruction.OpCode == 4 || instruction.OpCode == 5 || instruction.OpCode == 6 || instruction.OpCode == 7 ||
                             instruction.OpCode == 20 || instruction.OpCode == 21 || instruction.OpCode == 22 || instruction.OpCode == 23)
                    {
                        // The jump target is a relative amount. This signed offset is stored in IMMSigned.
                        // We need to multiply by 4 (because each segment is 4 bits). We then add our virtual
                        // address to get our final jump offset. The instruction jumps to the instruction following
                        // the branch, not the branch itself, so we need to add 4.
                        // Here we convert it to an absolute jump.
                        uint jumpTarget = instruction.GetAbsoluteJumpAddress();

                        // If we have a jump target we need to create a label for it. Multiple locations can jump to a single label,
                        // so we only need to add the label to the global list once (using ISet).
                        var label = new RomLabel(jumpTarget);
                        if (Disassembler.Labels.TryGetValue(label.Address, out var existingLabel))
                        {
                            if (!existingLabel.Name.Equals(label.Name))
                            {
                                throw new InvalidOperationException($"An attempt was made to declare label {label.Name} at pre-claimed address {label.Address} (existing label = {existingLabel.Name})");
                            }

                            // The label already exists. Nothing to see here.
                        }
                        else
                        {
                            lock (Disassembler.Labels)
                            {
                                if (Disassembler.Labels.TryGetValue(label.Address, out existingLabel))
                                {
                                    if (!existingLabel.Name.Equals(label.Name))
                                    {
                                        throw new InvalidOperationException($"An attempt was made to declare label {label.Name} at pre-claimed address {label.Address} (existing label = {existingLabel.Name})");
                                    }

                                    // The label already exists. Nothing to see here.
                                }
                                else
                                {
                                    Disassembler.Labels.Add(label.Address, label);
                                }
                            }
                        }
                    }
                    // LUI
                    else if (instruction.OpCode == 15)
                    {
                        Disassembler.DetermineLoadRef(File, addressOffset);
                    }
                    // COPROCESSOR 1
                    else if (instruction.OpCode == 17)
                    {
                        // BC1F, BC1T, BC1FL, BC1TL
                        if (instruction.RT == 0 || instruction.RT == 1 || instruction.RT == 2 || instruction.RT == 3)
                        {
                            // The jump target is a relative amount. This signed offset is stored in IMMSigned.
                            // We need to multiply by 4 (because each segment is 4 bits). We then add our virtual
                            // address to get our final jump offset. The instruction jumps to the instruction following
                            // the branch, not the branch itself, so we need to add 4.
                            // Here we convert it to an absolute jump.
                            uint jumpTarget = instruction.GetAbsoluteJumpAddress();

                            // If we have a jump target we need to create a label for it. Multiple locations can jump to a single label,
                            // so we only need to add the label to the global list once (using ISet).
                            var label = new RomLabel(jumpTarget);
                            if (Disassembler.Labels.TryGetValue(label.Address, out var existingLabel))
                            {
                                if (!existingLabel.Name.Equals(label.Name))
                                {
                                    throw new InvalidOperationException($"An attempt was made to declare label {label.Name} at pre-claimed address {label.Address} (existing label = {existingLabel.Name})");
                                }

                                // The label already exists. Nothing to see here.
                            }
                            else
                            {
                                lock (Disassembler.Labels)
                                {
                                    if (Disassembler.Labels.TryGetValue(label.Address, out existingLabel))
                                    {
                                        if (!existingLabel.Name.Equals(label.Name))
                                        {
                                            throw new InvalidOperationException($"An attempt was made to declare label {label.Name} at pre-claimed address {label.Address} (existing label = {existingLabel.Name})");
                                        }

                                        // The label already exists. Nothing to see here.
                                    }
                                    else
                                    {
                                        Disassembler.Labels.Add(label.Address, label);
                                    }
                                }
                            }
                        }
                    }
                }

                if (Disassembler.Variables.ContainsKey(virtualAddress))
                {
                    string name = Disassembler.GetLoadName(virtualAddress);
                    if (name.StartsWith("__switch"))
                    {
                        int addr = index;
                        uint caseAddr = BinaryPrimitives.ReadUInt32BigEndian(Data.Slice(addr * 4, 4).Span);
                        while (Disassembler.IsAddressInCodeRegion(caseAddr))
                        {
                            lock (Disassembler.SwitchCases)
                            {
                                Disassembler.SwitchCases.Add(caseAddr);
                            }
                            ++addr;
                            if (addr >= File.Length / 4)
                            {
                                break;
                            }
                            caseAddr = BinaryPrimitives.ReadUInt32BigEndian(Data.Slice(addr * 4, 4).Span);
                        }
                    }
                }
            }
        }
    }
}
