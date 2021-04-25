using System;
using System.Collections.Generic;
using System.IO;
using Foundry.Disassembly.Mips.Codes;
using Foundry.Disassembly.Mips.Utilities;

namespace Foundry.Disassembly.Mips
{
    public sealed class MipsDisassembler
    {
        private readonly ISet<DataSegment> DataSegments = new SortedSet<DataSegment>(Comparer<DataSegment>.Create((a, b) => a.VirtualAddress.CompareTo(b.VirtualAddress)));
        private readonly ISet<DataRegion> DataRegions = new SortedSet<DataRegion>(Comparer<DataRegion>.Create((a, b) => a.StartAddress.CompareTo(b.StartAddress)));

        private readonly IDictionary<uint, bool> DataRegionAddressCache = new Dictionary<uint, bool>();
        private readonly IDictionary<uint, bool> CodeRegionAddressCache = new Dictionary<uint, bool>();

        public void AddSegment(DataSegment segment)
        {
            DataSegments.Add(segment);
        }

        public void AddRegion(DataRegion region)
        {
            DataRegions.Add(region);
        }

        public void Disassemble(TextWriter output)
        {
            using var writer = new MipsWriter(output);
            writer.WriteHeader();

            foreach (var segment in DataSegments)
            {
                // TODO: Replace with known functions.
                writer.WriteLabel(segment.VirtualAddress, $".L_Main");

                var reader = new SpanReader(segment.Data.Span);
                for (uint instructionAddress = 0; instructionAddress < segment.Length / 4; ++instructionAddress)
                {
                    Instruction instruction = reader.ReadUInt32(instructionAddress * 4);
                    HexCode virtualAddress = segment.VirtualAddress + instructionAddress * 4;

                    if (!IsAddressInDataRegionOrUndefined(virtualAddress))
                    {
                        DisassembleInstruction(instruction, virtualAddress, instructionAddress, writer);
                    }
                }

                writer.Flush();
                output.WriteLine();
            }
        }

        private void DisassembleInstruction(in Instruction instruction, in HexCode virtualAddress, uint instructionAddress, MipsWriter writer)
        {
            if (!instruction.OpCode.IsValid)
            {
                writer.WriteError(instruction, virtualAddress, instructionAddress, $"ERROR_OP_CODE {instruction.OpCode.OpCode}");
                return;
            }

            if (instruction.RawInstruction == 0)
            {
                writer.WriteNop(instruction, virtualAddress, instructionAddress);
                return;
            }

            if (instruction.OpCode.OpCode == OperationCode.Code.Spec_RType)
            {
                DisassembleRTypeInstruction((InstructionR)instruction, virtualAddress, instructionAddress, writer);
                return;
            }

            if (instruction.OpCode.OpCode == OperationCode.Code.Spec_Branch)
            {
                DisassembleITypeInstruction((InstructionI)instruction, virtualAddress, instructionAddress, writer);
                return;
            }

            if (instruction.OpCode.OpCode == OperationCode.Code.J ||
                instruction.OpCode.OpCode == OperationCode.Code.Jal)
            {
                writer.WriteInstruction(instruction, virtualAddress, instructionAddress, $"{instruction.OpCode.Instruction}\t{((InstructionJ)instruction).Addr}");
                return;
            }

            if (instruction.OpCode.OpCode == OperationCode.Code.Lui)
            {
                /* https://github.com/N64RET/decomp-framework/blob/e74b13e365deae31dd1233642753af008bd2e1cf/N64RET/Processor/MIPSR/Disassembler/DisasmImpl.py#L910
                if not self.has_done_first_pass:
                    self.determine_load_ref(file, i)
                if addr in loadHighRefs: # lui loading the higher half of a pointer
                    ref = loadHighRefs[addr]
                    dis += "%s, %%hi(%s)" % (regs[get_rt(inst)], format_ref(self.make_load(ref[0]), ref[1]))
                else:
                    dis += "%s, 0x%04X" % (regs[get_rt(inst)], get_imm(inst))
                */
                var inst = (InstructionI)instruction;
                writer.WriteInstruction(instruction, virtualAddress, instructionAddress, $"{instruction.OpCode.Instruction}\t{inst.RT.RegisterName()}, {inst.IMM}");
                return;
            }

            if (instruction.OpCode.OpCode == OperationCode.Code.Spec_Coprocessor0 ||
                instruction.OpCode.OpCode == OperationCode.Code.Spec_Coprocessor1 ||
                instruction.OpCode.OpCode == OperationCode.Code.Spec_Coprocessor2)
            {
                DisassembleCoprocessorInstruction((InstructionR)instruction, virtualAddress, instructionAddress, writer);
                return;
            }

            if (instruction.OpCode.OpCode == OperationCode.Code.Beq ||
                instruction.OpCode.OpCode == OperationCode.Code.Bne ||
                instruction.OpCode.OpCode == OperationCode.Code.Beql ||
                instruction.OpCode.OpCode == OperationCode.Code.Bnel ||
                instruction.OpCode.OpCode == OperationCode.Code.Blez ||
                instruction.OpCode.OpCode == OperationCode.Code.Bgtz ||
                instruction.OpCode.OpCode == OperationCode.Code.Blezl ||
                instruction.OpCode.OpCode == OperationCode.Code.Bgtzl)
            {
                DisassembleJumpInstruction(instruction, virtualAddress, instructionAddress, writer);
                return;
            }

            if (instruction.OpCode.OpCode == OperationCode.Code.Addi ||
                instruction.OpCode.OpCode == OperationCode.Code.Addiu ||
                instruction.OpCode.OpCode == OperationCode.Code.Slti ||
                instruction.OpCode.OpCode == OperationCode.Code.Sltiu ||
                instruction.OpCode.OpCode == OperationCode.Code.Daddi ||
                instruction.OpCode.OpCode == OperationCode.Code.Daddiu)
            {
                var inst = (InstructionI)instruction;
                if (instruction.OpCode.OpCode == OperationCode.Code.Addiu && inst.RS == 0)
                {
                    writer.WriteInstruction(instruction, virtualAddress, instructionAddress, $"li\t{inst.RT.RegisterName()}, {inst.IMMSigned}");
                    return;
                }

                // https://github.com/N64RET/decomp-framework/blob/e74b13e365deae31dd1233642753af008bd2e1cf/N64RET/Processor/MIPSR/Disassembler/DisasmImpl.py#L903
                if (instruction.OpCode.OpCode == OperationCode.Code.Addiu && false/* virtualAddress in loadLowRefs*/)
                {
                    // ref = loadLowRefs[addr]
                    // dis += "%s, %s, %%lo(%s)" % (regs[get_rt(inst)], regs[get_rs(inst)], self.make_load(ref[0]).FormatOffset(ref[1])
                    return;
                }

                writer.WriteInstruction(instruction, virtualAddress, instructionAddress, $"{instruction.OpCode.Instruction}\t{inst.RT.RegisterName()}, {inst.RS.RegisterName()}, {inst.IMMSigned}");
                return;
            }

            // https://github.com/N64RET/decomp-framework/blob/e74b13e365deae31dd1233642753af008bd2e1cf/N64RET/Processor/MIPSR/Disassembler/DisasmImpl.py#L900

            writer.WriteInstruction(instruction, virtualAddress, instructionAddress, "stub\t/* NOT IMPLEMENTED */");
        }

        private void DisassembleJumpInstruction(in Instruction instruction, in HexCode virtualAddress, uint instructionAddress, MipsWriter writer)
        {
            if (instruction.OpCode.OpCode == OperationCode.Code.Beq ||
                instruction.OpCode.OpCode == OperationCode.Code.Bne ||
                instruction.OpCode.OpCode == OperationCode.Code.Beql ||
                instruction.OpCode.OpCode == OperationCode.Code.Bnel)
            {
                var rInstruction = (InstructionR)instruction;

                if (instruction.OpCode.OpCode == OperationCode.Code.Beq &&
                    rInstruction.RS == 0 && rInstruction.RT == 0)
                {
                    CreateJumpAndLabel("b", (InstructionI)instruction, virtualAddress, instructionAddress, writer);
                    return;
                }

                if (rInstruction.RT == 0)
                {
                    string code = (uint)instruction.OpCode.OpCode switch
                    {
                        4 => $"beqz",
                        5 => $"bnez",
                        20 => $"beqzl",
                        _ => $"bnezl"
                    };

                    CreateJumpAndLabelRS(code, (InstructionI)instruction, virtualAddress, instructionAddress, writer);
                }
                else
                {
                    CreateJumpAndLabelRSRT(instruction.OpCode.Instruction, (InstructionI)instruction, virtualAddress, instructionAddress, writer);
                }
                return;
            }

            if (instruction.OpCode.OpCode == OperationCode.Code.Blez ||
                instruction.OpCode.OpCode == OperationCode.Code.Bgtz ||
                instruction.OpCode.OpCode == OperationCode.Code.Blezl ||
                instruction.OpCode.OpCode == OperationCode.Code.Bgtzl)
            {
                CreateJumpAndLabelRS(instruction.OpCode.Instruction, (InstructionI)instruction, virtualAddress, instructionAddress, writer);
                return;
            }

            writer.WriteError(instruction, virtualAddress, instructionAddress, "JUMP CODE ERROR");
        }

        private void DisassembleRTypeInstruction(in InstructionR instruction, in HexCode virtualAddress, uint instructionAddress, MipsWriter writer)
        {
            if (instruction.Funct == 1)
            {
                writer.WriteInstruction(instruction, virtualAddress, instructionAddress, (instruction.RawInstruction & (1 << 16)) == 0
                    ? $"movf\t{instruction.RD.RegisterName()}, {instruction.RS.RegisterName()}, {(instruction.RawInstruction & (7 << 18)) >> 18}"
                    : $"movt\t{instruction.RD.RegisterName()}, {instruction.RS.RegisterName()}, {(instruction.RawInstruction & (7 << 18)) >> 18}");
                return;
            }

            if (!instruction.FunctionCode.IsValid)
            {
                writer.WriteError(instruction, virtualAddress, instructionAddress, $"ERROR_FUNC_CODE {instruction.FunctionCode.FuncCode}");
                return;
            }

            string output = instruction.FunctionCode.FuncCode switch
            {
                FunctionCode.Code.Or when instruction.RT == 0 => $"move\t{instruction.RD.RegisterName()}, {instruction.RS.RegisterName()}",
                FunctionCode.Code.Sll or
                FunctionCode.Code.Srl or
                FunctionCode.Code.Sra or
                FunctionCode.Code.Dsll or
                FunctionCode.Code.Dsra or
                FunctionCode.Code.Dsll32 or
                FunctionCode.Code.Dsra32 or
                FunctionCode.Code.Dsllv or
                FunctionCode.Code.Dsrlv or
                FunctionCode.Code.Dsrav => $"{instruction.FunctionCode.Instruction}\t{instruction.RD.RegisterName()}, {instruction.RT.RegisterName()}, {instruction.Shamd.ToString(true)}",
                FunctionCode.Code.Sllv or
                FunctionCode.Code.Srlv or
                FunctionCode.Code.Srav => $"{instruction.FunctionCode.Instruction}\t{instruction.RD.RegisterName()}, {instruction.RT.RegisterName()}, {instruction.RS.RegisterName()}",
                FunctionCode.Code.Jr or
                FunctionCode.Code.Jalr => $"{instruction.FunctionCode.Instruction}\t{instruction.RS.RegisterName()}",
                FunctionCode.Code.Syscall => instruction.FunctionCode.Instruction,
                FunctionCode.Code.Break => $"{instruction.FunctionCode.Instruction}\t/* Break Code 0x{(instruction.RawInstruction & 0x7FFFFFF) >> 6:X8} */",
                FunctionCode.Code.Mult or
                FunctionCode.Code.Multu or
                FunctionCode.Code.Dmult or
                FunctionCode.Code.Dmultu or
                FunctionCode.Code.Div or
                FunctionCode.Code.Divu or
                FunctionCode.Code.Ddiv or
                FunctionCode.Code.Ddivu => $"{instruction.FunctionCode.Instruction}\t{instruction.RD.RegisterName()}, {instruction.RT.RegisterName()}",
                FunctionCode.Code.Mfhi or
                FunctionCode.Code.Mflo => $"{instruction.FunctionCode.Instruction}\t{instruction.RD.RegisterName()}",
                FunctionCode.Code.Mthi or
                FunctionCode.Code.Mtlo => $"{instruction.FunctionCode.Instruction}\t{instruction.RS.RegisterName()}",
                FunctionCode.Code.Sub when instruction.RS == 0 => $"neg\t{instruction.RD.RegisterName()}, {instruction.RT.RegisterName()}",
                FunctionCode.Code.Sub when instruction.RT == 0 => $"neg\t{instruction.RD.RegisterName()}, {instruction.RT.RegisterName()}",
                FunctionCode.Code.And or
                FunctionCode.Code.Or or
                FunctionCode.Code.Xor or
                FunctionCode.Code.Nor or
                FunctionCode.Code.Movz or
                FunctionCode.Code.Movn or
                FunctionCode.Code.Sync or
                FunctionCode.Code.Add or
                FunctionCode.Code.Addu or
                FunctionCode.Code.Slt or
                FunctionCode.Code.Sltu or
                FunctionCode.Code.Dadd or
                FunctionCode.Code.Daddu or
                FunctionCode.Code.Tge or
                FunctionCode.Code.Tgeu or
                FunctionCode.Code.Tlt or
                FunctionCode.Code.Tltu or
                FunctionCode.Code.Teq or
                FunctionCode.Code.Tne or
                _ => $"{instruction.FunctionCode.Instruction}\t{instruction.RD.RegisterName()}, {instruction.RS.RegisterName()}, {instruction.RT.RegisterName()}"
            };

            writer.WriteInstruction(instruction, virtualAddress, instructionAddress, output);
        }

        private void DisassembleITypeInstruction(in InstructionI instruction, in HexCode virtualAddress, uint instructionAddress, MipsWriter writer)
        {
            var code = new BranchFunctionCode(instruction.RT);
            if (!code.IsValid)
            {
                writer.WriteError(instruction, virtualAddress, instructionAddress, $"ERROR_BRANCH1_CODE {code.BranchCode}");
                return;
            }

            CreateJumpAndLabelRS(code.Instruction, instruction, virtualAddress, instructionAddress, writer);
        }

        private void DisassembleCoprocessorInstruction(in InstructionR instruction, in HexCode virtualAddress, uint instructionAddress, MipsWriter writer)
        {
            uint coprocessorNumber = instruction.OpCode - (uint)OperationCode.Code.Spec_Coprocessor0;

            //TODO: Replace with enum values.
            if (stackalloc uint[] { 0, 1, 2, 4, 5, 6 }.Contains(instruction.RS))
            {
                var functionCode = new CoprocessorFunctionCode(instruction.RS.Value switch
                {
                    0 => (uint)CoprocessorFunctionCode.Code.Mfc0,
                    1 => (uint)CoprocessorFunctionCode.Code.Dmfc0,
                    2 => (uint)CoprocessorFunctionCode.Code.Cfc0,
                    4 => (uint)CoprocessorFunctionCode.Code.Mtc0,
                    5 => (uint)CoprocessorFunctionCode.Code.Dmtc0,
                    _ => (uint)CoprocessorFunctionCode.Code.Ctc0
                } + coprocessorNumber);

                string target = functionCode.Coprocessor != 0
                    ? instruction.RD.FloatRegisterName()
                    : instruction.RD.RegisterNumber();

                writer.WriteInstruction(instruction, virtualAddress, instructionAddress, $"{functionCode.Instruction}\t{instruction.RT.RegisterName()}, {target}");

                return;
            }

            if (instruction.RS == 8)
            {
                bool t = (instruction.RawInstruction & (1 << 16)) != 0;
                bool l = (instruction.RawInstruction & (1 << 17)) == 0;

                var functionCode = new CoprocessorFunctionCode(t switch
                {
                    true when l => (uint)CoprocessorFunctionCode.Code.Bc0tl,
                    true when !l => (uint)CoprocessorFunctionCode.Code.Bc0t,
                    false when l => (uint)CoprocessorFunctionCode.Code.Bc0fl,
                    _ => (uint)CoprocessorFunctionCode.Code.Bc0f
                } + coprocessorNumber);

                CreateJumpAndLabelRS(functionCode.Instruction, new InstructionI(instruction.RawInstruction), virtualAddress, instructionAddress, writer);

                return;
            }

            //TODO: Replace with enum values.
            if (stackalloc uint[] { 16, 17, 20, 21 }.Contains(instruction.RS))
            {
                if (coprocessorNumber == 0)
                {
                    if (stackalloc uint[] { 1, 2, 6, 8, 24 }.Contains(instruction.Funct))
                    {
                        writer.WriteInstruction(instruction, virtualAddress, instructionAddress, instruction.Funct.Value switch
                        {
                            1 => "tlbr",
                            2 => "tlbwi",
                            6 => "tlbwr",
                            8 => "tlbp",
                            24 => "eret",
                            _ => string.Empty
                        });
                        return;
                    }

                    writer.WriteError(instruction, virtualAddress, instructionAddress, $"COPROCESSOR 0 FUNC ERROR (funct {instruction.Funct.Value})");
                    return;
                }

                if (coprocessorNumber != 1)
                {
                    writer.WriteError(instruction, virtualAddress, instructionAddress, $"COPROCESSOR FUNC ERROR (cp{coprocessorNumber}, ffc {instruction.FloatFunctionCode.FuncCode})");
                    return;
                }

                if (!instruction.FloatFunctionCode.IsValid)
                {
                    writer.WriteError(instruction, virtualAddress, instructionAddress, $"FLOAT ERROR (ffc {instruction.FloatFunctionCode.FuncCode})");
                    return;
                }

                char floatFuncSuffix = instruction.RS.Value switch
                {
                    16 => 's',
                    17 => 'd',
                    20 => 'w',
                    21 => 'l',
                    _ => (char)0
                };

                string output = instruction.FloatFunctionCode.FuncCode switch
                {
                    FloatFunctionCode.Code.Add or
                    FloatFunctionCode.Code.Sub or
                    FloatFunctionCode.Code.Mul or
                    FloatFunctionCode.Code.Div or
                    FloatFunctionCode.Code.Movz or
                    FloatFunctionCode.Code.Movn => $"{instruction.FloatFunctionCode.Instruction}.{floatFuncSuffix}\t{instruction.FD.FloatRegisterName()}, {instruction.FS.FloatRegisterName()}, {instruction.FT.FloatRegisterName()}",
                    FloatFunctionCode.Code.Sqrt or
                    FloatFunctionCode.Code.Abs or
                    FloatFunctionCode.Code.Mov or
                    FloatFunctionCode.Code.Neg or
                    FloatFunctionCode.Code.Roundl or
                    FloatFunctionCode.Code.Truncl or
                    FloatFunctionCode.Code.Ceill or
                    FloatFunctionCode.Code.Floorl or
                    FloatFunctionCode.Code.Roundw or
                    FloatFunctionCode.Code.Truncw or
                    FloatFunctionCode.Code.Ceilw or
                    FloatFunctionCode.Code.Floorw or
                    FloatFunctionCode.Code.Cvts or
                    FloatFunctionCode.Code.Cvtd or
                    FloatFunctionCode.Code.Cvtw or
                    FloatFunctionCode.Code.Cvtl => $"{instruction.FloatFunctionCode.Instruction}.{floatFuncSuffix}\t{instruction.FD.FloatRegisterName()}, {instruction.FS.FloatRegisterName()}",
                    FloatFunctionCode.Code.Ceq or
                    FloatFunctionCode.Code.Clt or
                    FloatFunctionCode.Code.Cle => $"{instruction.FloatFunctionCode.Instruction}.{floatFuncSuffix}\t{instruction.FS.FloatRegisterName()}, {instruction.FT.FloatRegisterName()}",
                    FloatFunctionCode.Code.Cf or
                    FloatFunctionCode.Code.Cun or
                    FloatFunctionCode.Code.Cueq or
                    FloatFunctionCode.Code.Cueq or
                    FloatFunctionCode.Code.Colt or
                    FloatFunctionCode.Code.Cult or
                    FloatFunctionCode.Code.Cole or
                    FloatFunctionCode.Code.Cule or
                    FloatFunctionCode.Code.Csf or
                    FloatFunctionCode.Code.Cngle or
                    FloatFunctionCode.Code.Cseq or
                    FloatFunctionCode.Code.Cngl or
                    FloatFunctionCode.Code.Cnge or
                    FloatFunctionCode.Code.Cngt => $"{instruction.FloatFunctionCode.Instruction}.{floatFuncSuffix}\t /* FLOAT_ERR_NOT_IMPL */",
                    _ => string.Empty
                };

                writer.WriteInstruction(instruction, virtualAddress, instructionAddress, output);
                return;
            }

            writer.WriteError(instruction, virtualAddress, instructionAddress, $"COPROCESSOR ERROR (Code {instruction.RS.Value})");
        }
        private HexCode CreateJumpAndLabel(string code, in InstructionI instruction, in HexCode virtualAddress, uint instructionAddress, MipsWriter writer)
        {
            // The jump target is a relative amount. This signed offset is stored in IMMSigned.
            // We need to multiply by 4 (because each segment is 4 bits). We then add our virtual
            // address to get our final jump offset. The instruction jumps to the instruction following
            // the branch, not the branch itself, so we need to add 4.
            // Here we convert it to an absolute jump.
            var jumpTarget = new HexCode((uint)(instruction.IMMSigned * 4 + virtualAddress + 4));

            // Create a label at the actual jump target location.
            string labelName = writer.WriteLabel(jumpTarget);

            // Finally write the actual jump instruction.
            writer.WriteInstruction(instruction, virtualAddress, instructionAddress, $"{code}\t{labelName}");

            return jumpTarget;
        }

        private HexCode CreateJumpAndLabelRS(string code, in InstructionI instruction, in HexCode virtualAddress, uint instructionAddress, MipsWriter writer)
        {
            // The jump target is a relative amount. This signed offset is stored in IMMSigned.
            // We need to multiply by 4 (because each segment is 4 bits). We then add our virtual
            // address to get our final jump offset. The instruction jumps to the instruction following
            // the branch, not the branch itself, so we need to add 4.
            // Here we convert it to an absolute jump.
            var jumpTarget = new HexCode((uint)(instruction.IMMSigned * 4 + virtualAddress + 4));

            // Create a label at the actual jump target location.
            string labelName = writer.WriteLabel(jumpTarget);

            // Finally write the actual jump instruction.
            writer.WriteInstruction(instruction, virtualAddress, instructionAddress, $"{code}\t{instruction.RS.RegisterName()}, {labelName}");

            return jumpTarget;
        }

        private HexCode CreateJumpAndLabelRSRT(string code, in InstructionI instruction, in HexCode virtualAddress, uint instructionAddress, MipsWriter writer)
        {
            // The jump target is a relative amount. This signed offset is stored in IMMSigned.
            // We need to multiply by 4 (because each segment is 4 bits). We then add our virtual
            // address to get our final jump offset. The instruction jumps to the instruction following
            // the branch, not the branch itself, so we need to add 4.
            // Here we convert it to an absolute jump.
            var jumpTarget = new HexCode((uint)(instruction.IMMSigned * 4 + virtualAddress + 4));

            // Create a label at the actual jump target location.
            string labelName = writer.WriteLabel(jumpTarget);

            // Finally write the actual jump instruction.
            writer.WriteInstruction(instruction, virtualAddress, instructionAddress, $"{code}\t{instruction.RS.RegisterName()}, {instruction.RT.RegisterName()}, {labelName}");

            return jumpTarget;
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
    }
}
