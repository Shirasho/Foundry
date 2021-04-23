using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Disassembly.OoT.Syntax;
using Foundry.Media.Nintendo64.Rom.Utilities;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    internal sealed class MipsDisassembler : IAsyncDisposable
    {
        private readonly DisassemblerOptions Options;
        private readonly ISourceBuilder SourceBuilder;

        private readonly ISet<DataSegment> DataSegments = new SortedSet<DataSegment>(Comparer<DataSegment>.Create((a, b) => a.VirtualAddress.CompareTo(b.VirtualAddress)));
        private readonly ISet<DataRegion> DataRegions = new SortedSet<DataRegion>(Comparer<DataRegion>.Create((a, b) => a.StartAddress.CompareTo(b.StartAddress)));

        private readonly IDictionary<uint, bool> DataRegionAddressCache = new Dictionary<uint, bool>();
        private readonly IDictionary<uint, bool> CodeRegionAddressCache = new Dictionary<uint, bool>();

        private readonly IDictionary<uint, RomObject> Objects = new Dictionary<uint, RomObject>();
        private readonly IDictionary<uint, RomFunction> Functions = new Dictionary<uint, RomFunction>();
        private readonly IDictionary<uint, RomVariable> Variables = new SortedDictionary<uint, RomVariable>();
        private readonly IDictionary<uint, RomLabel> Labels = new Dictionary<uint, RomLabel>();

        private readonly ISet<uint> SwitchCaseAddresses = new HashSet<uint>();

        internal MipsDisassembler(DisassemblerOptions options, ISourceBuilder sourceBuilder)
        {
            Options = new DisassemblerOptions(options);
            SourceBuilder = sourceBuilder;

            DataSegments.AddRange(Options.Segments);
            DataRegions.AddRange(Options.Regions);
        }

        public ValueTask DisposeAsync()
        {
            return default;
        }

        public async Task DisassembleAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await AddKnownElementsAsync();

            if (!Options.TempDir.Exists)
            {
                await LogAsync($"Creating temp directory: {Options.TempDir.FullName}");
                Options.TempDir.Create();
            }

            foreach (var segment in DataSegments)
            {
                await LogAsync($"Saving {segment.Name} segment binary.");
                using var fs = Options.TempDir.CombineFile($"{segment.Name.ToLower()}.bin").Open(FileMode.Create, FileAccess.Write, FileShare.None);
                await fs.WriteAsync(segment.Data, cancellationToken);
                await fs.FlushAsync(cancellationToken);
            }

            await FirstPassAsync(cancellationToken);

            var codeOutputDir = Options.TempDir.CombineDirectory("src");
            if (!codeOutputDir.Exists)
            {
                codeOutputDir.Create();
            }
            else
            {
                codeOutputDir.DeleteChildren();
            }

            await SecondPassAsync(codeOutputDir, cancellationToken);
        }

        private async Task FirstPassAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await LogAsync("Starting first pass.");

            foreach (var segment in DataSegments)
            {
                await LogAsync($"Parsing data segment {segment.Name}.");
                var reader = new MemoryReader(segment.Data);
                int maxInstructionOffset = segment.Length / 4;
                for (int instructionOffset = 0; instructionOffset < maxInstructionOffset; ++instructionOffset)
                {
                    reader.TryReadUInt32(instructionOffset * 4, out uint instruction);
                    uint addr = segment.VirtualAddress + (uint)instructionOffset * 4;
                    if (!IsAddressInDataRegionOrUndefined(addr))
                    {
                        // We don't care about storing the result right now. We just need cache values to be generated.
                        DisassembleInstruction(instruction, addr, instructionOffset, null, segment);

                        //TODO: Figure out what "flag" this sets and check that instead.
                        if (instruction == 0x03E00008)
                        {
                            int nextIndex = instructionOffset + 2;
                            if (reader.TryReadUInt32(nextIndex, out uint nop) && nop == 0)
                            {
                                while (reader.TryReadUInt32(nextIndex, out nop) && nop == 0)
                                {
                                    ++nextIndex;
                                }

                                uint newObjectStart = segment.VirtualAddress + (uint)nextIndex * 4 + 15;
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

                                if (addr2 >= segment.Length / 4)
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
                            TryAddVariable(instruction, 1);
                        }
                    }
                }
            }
        }

        private async Task SecondPassAsync(DirectoryInfo outputDir, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await LogAsync("Starting second pass.");

            foreach (var segment in DataSegments)
            {
                await LogAsync($"Parsing data segment {segment.Name}.");

                var writer = new MipsWriter(outputDir.CombineFile($"{GetObjectName(segment.VirtualAddress, segment.VirtualAddress)}.asm"));

                try
                {
                    await writer.WriteHeaderAsync(cancellationToken);
                    var reader = new MemoryReader(segment.Data);
                    int maxInstructionOffset = segment.Length / 4;
                    for (int instructionOffset = 0; instructionOffset < maxInstructionOffset; ++instructionOffset)
                    {
                        reader.TryReadUInt32(instructionOffset * 4, out uint instruction);
                        uint addr = segment.VirtualAddress + (uint)instructionOffset * 4;

                        // We have a known object, so give it its own file.
                        if (Objects.TryGetValue(addr, out var obj))
                        {
                            await writer.DisposeAsync();
                            writer = new MipsWriter(outputDir.CombineFile($"{GetObjectName(obj, segment.VirtualAddress)}.asm"));
                            await writer.WriteHeaderAsync(cancellationToken);
                        }

                        bool isSwitchCase = SwitchCaseAddresses.Contains(addr);

                        // Goto Label
                        if (Labels.TryGetValue(addr, out var label) && !isSwitchCase)
                        {
                            await writer.WriteLabelAsync(label, cancellationToken);
                        }
                        // Switch cases
                        if (isSwitchCase)
                        {
                            await writer.WriteSwitchCaseAsync(addr, cancellationToken);
                        }
                        // Function declaration
                        if (Functions.TryGetValue(addr, out var function))
                        {
                            await writer.WriteFunctionAsync(function, cancellationToken);
                        }

                        // Instruction operation
                        if (!IsAddressInDataRegionOrUndefined(addr))
                        {
                            var syntax = DisassembleInstruction(instruction, addr, instructionOffset, null, segment);
                            await writer.WriteSyntaxAsync(syntax, instruction, addr, instructionOffset, cancellationToken);
                        }
                        // Function call
                        else if (Functions.TryGetValue(instruction, out function))
                        {
                            if (Variables.TryGetValue(addr, out var variable))
                            {
                                await writer.WriteVariableAsync(GetVariableName(variable), cancellationToken);
                            }
                            await writer.WriteFunctionCallAsync(instructionOffset, addr, function, cancellationToken);
                        }
                        // Goto call
                        else if (SwitchCaseAddresses.Contains(instruction))
                        {
                            if (Variables.TryGetValue(addr, out var variable))
                            {
                                await writer.WriteVariableAsync(GetVariableName(variable), cancellationToken);
                            }
                            await writer.WriteGoToCallAsync(instructionOffset, addr, instruction, cancellationToken);
                        }
                        // Variable to variable call?
                        else if (Variables.TryGetValue(instruction, out var instVariable))
                        {
                            if (Variables.TryGetValue(addr, out var variable))
                            {
                                await writer.WriteVariableAsync(GetVariableName(variable), cancellationToken);
                            }
                            await writer.WriteVariableCallAsync(instructionOffset, addr, GetVariableName(instVariable), cancellationToken);
                        }
                        // Special case gSaveContext.weekEventReg because there are pointers to fields of it in other parts
                        else if (instruction.IsBetween(0x801F0568, 0x801F0684, EInclusivity.Inclusive, EInclusivity.Exclusive))
                        {
                            if (Variables.TryGetValue(addr, out var variable))
                            {
                                await writer.WriteVariableAsync(GetVariableName(variable), cancellationToken);
                            }
                            var offset = GetVariableOffset(instruction);
                            await writer.WriteVariableCallAsync(instructionOffset, addr, GetVariableName(offset.Nearest), offset.Offset, cancellationToken);
                        }
                        else
                        {
                            uint printHead = addr;
                            uint dataStream = instruction;
                            while (printHead < addr + 4)
                            {
                                if (Variables.ContainsKey(printHead))
                                {
                                    await writer.WriteVariableAsync(GetVariableName(printHead), cancellationToken);
                                }
                                if (Variables.ContainsKey(printHead + 1) || printHead % 2 != 0)
                                {
                                    await writer.WriteVariableCallByteAsync(instructionOffset, addr, GetPaddedHex((dataStream >> 24) & 0xFF, 2), cancellationToken);
                                    dataStream <<= 8;
                                    ++printHead;
                                }
                                else if (Variables.ContainsKey(printHead + 2) || Variables.ContainsKey(printHead + 3) || printHead % 4 != 0)
                                {
                                    await writer.WriteVariableCallShortAsync(instructionOffset, addr, GetPaddedHex((dataStream >> 16) & 0xFFFF, 4), cancellationToken);
                                    dataStream <<= 16;
                                    printHead += 2;
                                }
                                else
                                {
                                    await writer.WriteVariableCallWordAsync(instructionOffset, addr, GetPaddedHex(dataStream & 0xFFFFFFFF, 8), cancellationToken);
                                    dataStream <<= 16;
                                    printHead += 2;
                                }
                            }
                        }

                        await writer.FlushAsync(cancellationToken);
                    }
                }
                finally
                {
                    await writer.DisposeAsync();
                }
            }
        }

        private ISyntax DisassembleInstruction(in Instruction instruction, in Instruction addr, int instructionOffset, ISyntax? owner, DataSegment segment)
        {
            if (instruction == 0)
            {
                return new NopOperationSyntax(owner, instruction);
            }

            if (instruction.OperationCode == 0)
            {
                return DisassembleFunction(instruction, addr, instructionOffset, owner);
            }

            if (instruction.OperationCode == 1)
            {
                if (!BranchInstruction.TryGetValue(instruction.RT, out var branchInstruction))
                {
                    return new InvalidBranchOperationSyntax(owner, instruction, addr, instructionOffset);
                }

                var label = new RomLabel((uint)(instruction.ImmediateSigned * 4 + addr + 4));
                Labels.Add(label.Address, label);
                return new BranchOperationSyntax(owner, instruction, branchInstruction, label);
            }

            if (instruction.OperationCode == 16 ||
                instruction.OperationCode == 17 ||
                instruction.OperationCode == 18)
            {
                // https://github.com/N64RET/decomp-framework/blob/e74b13e365deae31dd1233642753af008bd2e1cf/N64RET/Processor/MIPSR/Disassembler/DisasmImpl.py#L819
            //     z = op_num - 16
            //rs = get_rs(inst)
            //if rs == 0:
            //    dis += "mfc%d\t%s, %s" % (z, regs[get_rt(inst)], float_reg(get_rd(inst)) if z != 0 else "$%d" % get_rd(inst))
            //elif rs == 1:
            //    dis += "dmfc%d\t%s, %s" % (z, regs[get_rt(inst)], float_reg(get_rd(inst)) if z != 0 else "$%d" % get_rd(inst))
            //elif rs == 2:
            //    dis += "cfc%d\t%s, %s" % (z, regs[get_rt(inst)], float_reg(get_rd(inst)) if z != 0 else "$%d" % get_rd(inst))
            //elif rs == 4:
            //    dis += "mtc%d\t%s, %s" % (z, regs[get_rt(inst)], float_reg(get_rd(inst)) if z != 0 else "$%d" % get_rd(inst))
            //elif rs == 5:
            //    dis += "dmtc%d\t%s, %s" % (z, regs[get_rt(inst)], float_reg(get_rd(inst)) if z != 0 else "$%d" % get_rd(inst))
            //elif rs == 6:
            //    dis += "ctc%d\t%s, %s" % (z, regs[get_rt(inst)], float_reg(get_rd(inst)) if z != 0 else "$%d" % get_rd(inst))
            //elif rs == 8:
            //    dis += "bc%d%s%s %s" % (z, "f" if ((inst & (1 << 16)) == 0) else "t", "" if ((inst & (1 << 17)) == 0) else "l", self.make_label(get_signed_imm(inst), addr))
            //elif rs == 16 or rs == 17 or rs == 20 or rs == 21:
            //    if z == 0:
            //        func = get_func(inst)
            //        if func == 1:
            //            dis += "tlbr"
            //        elif func == 2:
            //            dis += "tlbwi"
            //        elif func == 6:
            //            dis += "tlbwr"
            //        elif func == 8:
            //            dis += "tlbp"
            //        elif func == 24:
            //            dis += "eret"
            //        else:
            //            # TODO deret?
            //            dis += "/* cop0_error: %d */\n" % func
            //            dis += "/* %06d 0x%08X */ .word\t%s\n" % (i, addr, hex(inst).strip("L"))
            //    elif z != 1:
            //        dis += "/* cop_error: %d */\n" % z
            //        dis += "/* %06d 0x%08X */ .word\t%s\n" % (i, addr, hex(inst).strip("L"))
            //    else:
            //        if rs == 16:
            //            f = "s"
            //        elif rs == 17:
            //            f = "d"
            //        elif rs == 20:
            //            f = "w"
            //        elif rs == 21:
            //            f = "l"
            //        func = get_func(inst)
            //        if func not in floats:
            //            dis += "float_error: %d" % func
            //        else:
            //            dis += "%s.%s\t" % (floats[func], f)
            //            if func == 0 or func == 1 or func == 2 or func == 3 or func == 18 or func == 19: # 3 op
            //                dis += "%s, %s, %s" % (float_reg(get_fd(inst)), float_reg(get_fs(inst)), float_reg(get_ft(inst)))
            //            elif (func == 4 or func == 5 or func == 6 or func == 7 or func == 8 or func == 9  or func == 10 or func == 11 or func == 12
            //                  or func == 13 or func == 14 or func == 15 or func == 32 or func == 33 or func == 36 or func == 37): # 2 op
            //                dis += "%s, %s" % (float_reg(get_fd(inst)), float_reg(get_fs(inst)))
            //            elif func == 50 or func == 60 or func == 62: # c.eq, c.lt, c.le
            //                dis += "%s, %s" % (float_reg(get_fs(inst)), float_reg(get_ft(inst)))
            //else:
            //    dis += "/* coproc_error: %d */\n" % rs
            //    dis += "/* %06d 0x%08X */ .word\t%s\n" % (i, addr, hex(inst).strip("L"))
            }
            //else if (instruction.OperationCode not in ops)
            {
                //2:"j", 3:"jal", 4:"beq", 5:"bne", 6:"blez", 7:"bgtz",
                //8:"addi", 9:"addiu", 10:"slti", 11:"sltiu", 12:"andi", 13:"ori", 14:"xori", 15:"lui",
                //20:"beql", 21:"bnel", 22:"blezl", 23:"bgtzl",
                //24:"daddi", 25:"daddiu",
                //32:"lb", 33:"lh", 34:"lwl", 35:"lw", 36:"lbu", 37:"lhu", 38:"lwr",
                //40:"sb", 41:"sh", 42:"swl", 43:"sw", 46:"swr", 47:"cache",
                //49:"lwc1", 55:"ld", 51:"pref", 48:"ll", 50:"lwc2", 53:"ldc1", 54:"ldc2",
                //57:"swc1", 61:"sdc1", 63:"sd", 56:"sc", 58:"swc2", 62:"sdc2",

                return new InvalidOperationSyntax(owner, instruction, addr, instructionOffset);
            }
            else
            {
                // https://github.com/N64RET/decomp-framework/blob/e74b13e365deae31dd1233642753af008bd2e1cf/N64RET/Processor/MIPSR/Disassembler/DisasmImpl.py#L886
            //    dis += "%s\t" % ops[op_num]
            //if op_num == 2 or op_num == 3: # j, jal
            //    dis += "%s" % self.make_func(inst & 0x3FFFFFF, addr)
            //elif op_num == 4 or op_num == 5 or op_num == 20 or op_num == 21: # beq, bne, beql, bnel
            //    if op_num == 4 and get_rs(inst) == get_rt(inst) == 0: # beq with both zero regs is a branch always (b %label)
            //        dis = "b\t%s" % self.make_label(get_signed_imm(inst), addr)
            //    else:
            //        if get_rt(inst) == 0: # branchs comparing to 0 have a shorthand
            //            dis = "%s\t" % ("beqz" if op_num == 4 else "bnez" if op_num == 5 else "beqzl" if op_num == 20 else "bnezl")
            //            dis += "%s, %s" % (regs[get_rs(inst)], self.make_label(get_signed_imm(inst), addr))
            //        else:
            //            dis += "%s, %s, %s" % (regs[get_rs(inst)], regs[get_rt(inst)], self.make_label(get_signed_imm(inst), addr))
            //elif op_num == 6 or op_num == 7 or op_num == 22 or op_num == 23: # blez, bgtz, blezl, bgtzl
            //    dis += "%s, %s" % (regs[get_rs(inst)], self.make_label(get_signed_imm(inst), addr))
            //elif op_num == 8 or op_num == 9 or op_num == 10 or op_num == 11 or op_num == 24 or op_num == 25: # addi, addiu, slti, sltiu, daddi, daddiu
            //    if op_num == 9 and get_rs(inst) == 0: # addiu with reg 0 is load immediate (li)
            //        dis = "li\t%s, %d" % (regs[get_rt(inst)], get_signed_imm(inst))
            //    elif op_num == 9 and addr in loadLowRefs: # addiu loading the lower half of a pointer
            //        ref = loadLowRefs[addr]
            //        dis += "%s, %s, %%lo(%s)" % (regs[get_rt(inst)], regs[get_rs(inst)], format_ref(self.make_load(ref[0]), ref[1]))
            //    else:
            //        dis += "%s, %s, %d" % (regs[get_rt(inst)], regs[get_rs(inst)], get_signed_imm(inst))
            //elif op_num == 12 or op_num == 13 or op_num == 14: # andi, ori, xori
            //    dis += "%s, %s, %#X" % (regs[get_rt(inst)], regs[get_rs(inst)], get_imm(inst))
            //elif op_num == 15: # lui
            //    if not self.has_done_first_pass:
            //        self.determine_load_ref(file, i)
            //    if addr in loadHighRefs: # lui loading the higher half of a pointer
            //        ref = loadHighRefs[addr]
            //        dis += "%s, %%hi(%s)" % (regs[get_rt(inst)], format_ref(self.make_load(ref[0]), ref[1]))
            //    else:
            //        dis += "%s, 0x%04X" % (regs[get_rt(inst)], get_imm(inst))
            //elif (op_num == 32 or op_num == 33 or op_num == 34 or op_num == 35 or op_num == 38 or op_num == 40 or op_num == 41 or
            //     op_num == 42 or op_num == 42 or op_num == 43 or op_num == 46 or op_num == 55 or op_num == 63): # load/stores
            //    if addr in loadLowRefs: # loading with immediate forming lower half of pointer
            //        ref = loadLowRefs[addr]
            //        dis += "%s, %%lo(%s)(%s)" % (regs[get_rt(inst)], format_ref(self.make_load(ref[0]), ref[1]), regs[get_rs(inst)])
            //    else:
            //        dis += "%s, %#X(%s)" % (regs[get_rt(inst)], get_signed_imm(inst), regs[get_rs(inst)])
            //elif op_num == 36 or op_num == 37: # lbu, lhu
            //    if addr in loadLowRefs: # loading with immediate forming lower half of pointer
            //        ref = loadLowRefs[addr]
            //        dis += "%s, %%lo(%s)(%s)" % (regs[get_rt(inst)], format_ref(self.make_load(ref[0]), ref[1]), regs[get_rs(inst)])
            //    else:
            //        dis += "%s, %#X(%s)" % (regs[get_rt(inst)], get_signed_imm(inst), regs[get_rs(inst)])
            //elif (op_num == 49 or op_num == 50 or op_num == 53 or op_num == 54 or op_num == 57 or op_num == 58 or
            //      op_num == 61 or op_num == 62): # load/store between co-processors
            //    if addr in loadLowRefs: # loading with immediate forming lower half of pointer
            //        ref = loadLowRefs[addr]
            //        dis += "%s, %%lo(%s)(%s)" % (float_reg(get_rt(inst)), format_ref(self.make_load(ref[0]), ref[1]), regs[get_rs(inst)])
            //    else:
            //        dis += "%s, %#X(%s)" % (float_reg(get_rt(inst)), get_signed_imm(inst), regs[get_rs(inst)])
            //elif op_num == 47: # cache
            //    if addr in loadLowRefs: # cache op with immediate forming lower half of pointer
            //        ref = loadLowRefs[addr]
            //        dis += "0x%02X, %%lo(%s)(%s)" % (get_rt(inst), format_ref(self.make_load(ref[0]), ref[1]), regs[get_rs(inst)])
            //    else:
            //        dis += "0x%02X, %#X(%s)" % (get_rt(inst), get_signed_imm(inst), regs[get_rs(inst)])
            }

            return new NopOperationSyntax(owner, instruction);
        }

        private ISyntax DisassembleFunction(in Instruction instruction, in Instruction addr, int instructionOffset, ISyntax? owner)
        {
            if (instruction.FunctionCode == 1)
            {
                uint cc = (instruction & (7 << 18)) >> 18;
                if ((instruction & (1 << 16)) == 0)
                {
                    return new MovfOperationSyntax(owner, instruction, cc);
                }
                else
                {
                    return new MovtOperationSyntax(owner, instruction, cc);
                }
            }

            var functionCode = instruction.FunctionCode;

            if (!functionCode.IsValid)
            {
                return new InvalidFunctionOperationSyntax(owner, instruction, addr, instructionOffset);
            }

            // Or with 0 registry is move operation.
            if (functionCode == FunctionCode.or && instruction.RT == Register.zero)
            {
                return new MoveOperationSyntax(owner, instruction);
            }

            if (functionCode == FunctionCode.sll)
            {
                return new SllOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.srl)
            {
                return new SrlOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.sra)
            {
                return new SraOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.dsll)
            {
                return new DsllOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.dsra)
            {
                return new DsraOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.dsll32)
            {
                return new Dsll32OperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.dsra32)
            {
                return new Dsra32OperationSyntax(owner, instruction);
            }

            if (functionCode == FunctionCode.sllv)
            {
                return new SllvOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.srlv)
            {
                return new SrlvOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.srav)
            {
                return new SravOperationSyntax(owner, instruction);
            }

            if (functionCode == FunctionCode.jr)
            {
                return new JrOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.jalr)
            {
                return new JalrOperationSyntax(owner, instruction);
            }

            if (functionCode == FunctionCode.@break)
            {
                return new BreakOperationSyntax(owner, instruction);
            }

            if (functionCode == FunctionCode.mfhi)
            {
                return new MfhiOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.mflo)
            {
                return new MfloOperationSyntax(owner, instruction);
            }

            if (functionCode == FunctionCode.mthi)
            {
                return new MthiOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.mtlo)
            {
                return new MtloOperationSyntax(owner, instruction);
            }

            if (functionCode == FunctionCode.mult)
            {
                return new MultOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.multu)
            {
                return new MultuOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.dmult)
            {
                return new DmultOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.dmultu)
            {
                return new DmultuOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.div)
            {
                return new DivOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.divu)
            {
                return new DivuOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.ddiv)
            {
                return new DdivOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.ddivu)
            {
                return new DdivuOperationSyntax(owner, instruction);
            }

            if (instruction.RS == Register.zero)
            {
                if (functionCode == FunctionCode.sub)
                {
                    return new NegOperationSyntax(owner, instruction);
                }

                if (functionCode == FunctionCode.subu)
                {
                    return new NeguOperationSyntax(owner, instruction);
                }
            }

            if (functionCode == FunctionCode.dsllv)
            {
                return new DsllvOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.dsrlv)
            {
                return new DsrlvOperationSyntax(owner, instruction);
            }
            if (functionCode == FunctionCode.dsrav)
            {
                return new DsravOperationSyntax(owner, instruction);
            }

            // We should never hit this point. If we do we need to know.
            Debug.Assert(false, "All functions should be accounted for.");
            return new InvalidFunctionOperationSyntax(owner, instruction, addr, instructionOffset);
        }

        private async Task AddKnownElementsAsync()
        {
            await LogAsync("Registering known functions.");
            foreach (var func in Options.KnownFunctions)
            {
                if (!Functions.TryAdd(func.Address, new RomFunction(func.Address, func.Name)))
                {
                    var existingFunction = Functions[func.Address];
                    throw new InvalidOperationException($"Unable to add known function 0x{func.Address:X} ({RomFunction.GenerateName(func.Address)}) - function {existingFunction.GetName()} is already registered with that address.");
                }
            }

            await LogAsync("Registering known objects.");
            foreach (var obj in Options.KnownObjects)
            {
                if (!Objects.TryAdd(obj.Address, new RomObject(obj.Address, obj.Size, obj.Name)))
                {
                    var existingObject = Objects[obj.Address];
                    throw new InvalidOperationException($"Unable to add known object 0x{obj.Address:X} ({ obj.Name ?? RomObject.GenerateName(obj.Address)}) - object {existingObject.GetName()} is already registered with that address.");
                }

                if (AddressIsInCodeRegion(obj.Address))
                {
                    // Assume every object starts with a function.
                    if (!Functions.TryAdd(obj.Address, new RomFunction(obj.Address, null)))
                    {
                        var existingFunction = Functions[obj.Address];
                        await Options.ErrorWriter.WriteLineAsync($"Unable to add implicit object function 0x{obj.Address:X} ({RomFunction.GenerateName(obj.Address)}) - function {existingFunction.GetName()} is already registered with that address.");
                    }
                }
            }

            await LogAsync("Registering known variables.");
            foreach (var variable in Options.KnownVariables)
            {
                if (!TryAddVariable(new RomVariable(variable.Address, variable.Size, variable.Name)))
                {
                    if (Variables.TryGetValue(variable.Address, out var existingVariable))
                    {
                        throw new InvalidOperationException($"Unable to add known variable 0x{variable.Address:X} ({variable.Name}) - variable {existingVariable.KnownName ?? $"{variable.Address:X}"} is already registered with that address.");
                    }

                    throw new InvalidOperationException($"Unable to add known variable 0x{variable.Address:X} ({variable.Name}) - the specified address is not valid or known to be bad.");
                }
            }
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

        /// <summary>
        /// Gets the name of the symbol or function at the address specified in <paramref name="addr"/>.
        /// If no known symbol or function exists at that address, a name generated using the specified
        /// address is returned.
        /// </summary>
        /// <param name="addr">The address to get the name of.</param>
        private string GetVariableName(uint addr)
        {
            if (IsAddressInDataRegionOrUndefined(addr))
            {
                return GetSymbolName(addr);
            }

            return GetFunctionName(addr);
        }

        /// <summary>
        /// Gets the name of the symbol or function at the address specified in <paramref name="addr"/>.
        /// If no known symbol or function exists at that address, a name generated using the specified
        /// address is returned.
        /// </summary>
        /// <param name="variable">The variable to get the name of.</param>
        private string GetVariableName(in RomVariable variable)
        {
            if (IsAddressInDataRegionOrUndefined(variable.Address))
            {
                return variable.GetName();
            }

            return GetFunctionName(variable.Address);
        }

        /// <summary>
        /// Gets the name of the symbol at the address specified in <paramref name="addr"/>.
        /// If no known variable exists at that address, a name generated using the specified
        /// address is returned.
        /// </summary>
        /// <param name="addr">The address to get the name of.</param>
        private string GetSymbolName(uint addr)
        {
            return Variables.TryGetValue(addr, out var variable) && !string.IsNullOrWhiteSpace(variable.KnownName)
                ? variable.KnownName
                : RomVariable.GenerateName(addr);
        }

        /// <summary>
        /// Gets the name of the function at the address specified in <paramref name="addr"/>.
        /// If no known function exists at that address, a name generated using the specified
        /// address is returned.
        /// </summary>
        /// <param name="addr">The address to get the name of.</param>
        private string GetFunctionName(uint addr)
        {
            return Functions.TryGetValue(addr, out var function)
                ? function.GetName()
                : RomFunction.GenerateName(addr);
        }

        /// <summary>
        /// Gets the name of the object at the address specified in <paramref name="addr"/>.
        /// If no known object exists at that address, a name generated using the specified
        /// address and data segment is returned.
        /// </summary>
        /// <param name="obj">The object to get the name of.</param>
        /// <exception cref="ArgumentException"><paramref name="fileAddr"/> does not point to a valid data segment.</exception>
        private string GetObjectName(in RomObject obj, uint fileAddr)
        {
            if (!string.IsNullOrWhiteSpace(obj.KnownName))
            {
                return obj.KnownName;
            }

            string? filename = null;
            foreach (var segment in DataSegments)
            {
                if (fileAddr == segment.VirtualAddress)
                {
                    filename = segment.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException($"{nameof(fileAddr)} [0x{fileAddr:X}] does not point to a valid data segment.");
            }
            return $"{filename}_0x{obj.Address:X}";
        }

        /// <summary>
        /// Gets the name of the object at the address specified in <paramref name="addr"/>.
        /// If no known object exists at that address, a name generated using the specified
        /// address and data segment is returned.
        /// </summary>
        /// <param name="addr">The address to get the name of.</param>
        /// <exception cref="ArgumentException"><paramref name="fileAddr"/> does not point to a valid data segment.</exception>
        private string GetObjectName(uint addr, uint fileAddr)
        {
            if (Objects.TryGetValue(addr, out var obj) && !string.IsNullOrWhiteSpace(obj.KnownName))
            {
                return obj.KnownName;
            }

            string? filename = null;
            foreach (var segment in DataSegments)
            {
                if (fileAddr == segment.VirtualAddress)
                {
                    filename = segment.Name;
                }
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException($"{nameof(fileAddr)} [0x{fileAddr:X}] does not point to a valid data segment.");
            }
            return $"{filename}_0x{addr:X}";
        }

        /// <summary>
        /// Converts <paramref name="value"/> into hex format.
        /// </summary>
        /// <param name="value">The value to convert to hex format.</param>
        /// <param name="desiredLength">The desired length of the hex, excluding the leading '0x'.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string GetPaddedHex(uint value, int desiredLength)
        {
            string result = value.ToString("X");
            return "0x" + result.PadLeft(desiredLength, '0');
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
                if (addr.IsBetween(region.StartAddress, region.EndAddress))
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
        /// in <see cref="DataSegments"/> and is not in <see cref="DataRegions"/>.
        /// </summary>
        /// <param name="addr">The address to look up.</param>
        private bool IsAddressInCodeRegion(uint addr)
        {
            if (CodeRegionAddressCache.TryGetValue(addr, out bool result))
            {
                return result;
            }

            foreach (var file in DataSegments)
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

        private bool TryAddVariable(in RomVariable variable)
        {
            // Known special case that is mis-identified as a variable.
            // It is actually in the middle of a pointer.
            if (variable.Address == 0x80AAB3AE)
            {
                return false;
            }

            return Variables.TryAdd(variable.Address, variable);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryAddVariable(uint address, int size, string? name = null)
        {
            return TryAddVariable(new RomVariable(address, size, name));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task LogAsync(string message, CancellationToken cancellationToken = default)
        {
            return Options.LogWriter.WriteLineAsync(message, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Task LogErrorAsync(string message, CancellationToken cancellationToken = default)
        {
            return Options.ErrorWriter.WriteLineAsync(message, cancellationToken);
        }
    }
}
