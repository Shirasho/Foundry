using System;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    public sealed partial class Disassembler
    {
        private delegate Operation ToOperationDelegate(in Instruction instruction, DataFile file);

        private Operation DisassembleInstruction(Instruction instruction, DataFile file)
        {
            ToOperationDelegate toOperation = instruction.OpCode switch
            {
                // Special prefix
                0 => instruction.Funct switch
                {
                    0 => instruction.RD != 0 ? SLL : NOP,
                    2 => instruction.RD != 0 ? SRL : NOP,
                    3 => instruction.RD != 0 ? SRA : NOP,
                    4 => instruction.RD != 0 ? SLLV : NOP,
                    6 => instruction.RD != 0 ? SRLV : NOP,
                    7 => instruction.RD != 0 ? SRAV : NOP,
                    8 => JR,
                    9 => instruction.RD != 0 ? JALR : NOP,
                    10 => MOVZ,
                    11 => MOVN,
                    12 => SYSCALL,
                    13 => BREAK,
                    15 => SYNC,
                    16 => instruction.RD != 0 ? MFHI : NOP,
                    17 => MTHI,
                    18 => instruction.RD != 0 ? MFLO : NOP,
                    19 => MTLO,
                    20 => instruction.RD != 0 ? DSLLV : NOP,
                    22 => instruction.RD != 0 ? DSRLV : NOP,
                    23 => instruction.RD != 0 ? DSRAV : NOP,
                    24 => MULT,
                    25 => MULTU,
                    26 => DIV,
                    27 => DIVU,
                    28 => DMULT,
                    29 => DMULTU,
                    30 => DDIV,
                    31 => DDIVU,
                    32 => instruction.RD != 0 ? ADD : NOP,
                    33 => instruction.RD != 0 ? ADDU : NOP,
                    34 => instruction.RD != 0 ? SUB : NOP,
                    35 => instruction.RD != 0 ? SUBU : NOP,
                    36 => instruction.RD != 0 ? AND : NOP,
                    37 => instruction.RD != 0 ? OR : NOP,
                    38 => instruction.RD != 0 ? XOR : NOP,
                    39 => instruction.RD != 0 ? NOR : NOP,
                    42 => instruction.RD != 0 ? SLT : NOP,
                    43 => instruction.RD != 0 ? SLTU : NOP,
                    44 => instruction.RD != 0 ? DADD : NOP,
                    45 => instruction.RD != 0 ? DADDU : NOP,
                    46 => instruction.RD != 0 ? DSUB : NOP,
                    47 => instruction.RD != 0 ? DSUBU : NOP,
                    48 => TGE,
                    49 => TGEU,
                    50 => TLT,
                    51 => TLTU,
                    52 => TEQ,
                    54 => TNE,
                    56 => instruction.RD != 0 ? DSLL : NOP,
                    58 => instruction.RD != 0 ? DSRL : NOP,
                    59 => instruction.RD != 0 ? DSRA : NOP,
                    60 => instruction.RD != 0 ? DSLL32 : NOP,
                    62 => instruction.RD != 0 ? DSRL32 : NOP,
                    63 => instruction.RD != 0 ? DSRA32 : NOP,
                    1 or 5 or 14 or 21 or 40 or 41 or 53 or 55 or 57 => RESERVED,
                    _ => INVALID
                },
                // REGIMM
                1 => instruction.RT switch
                {
                    0 => BLTZ,
                    1 => BGEZ,
                    2 => BLTZL,
                    3 => BGEZL,
                    8 => TGEI,
                    9 => TGEIU,
                    10 => TLTI,
                    11 => TLTIU,
                    12 => TEQI,
                    14 => TNEI,
                    16 => BLTZAL,
                    17 => BGEZAL,
                    18 => BLTZALL,
                    19 => BGEZALL,
                    (>= 4 and <= 7) or 13 or 15 or (>= 20 and <= 31) => RESERVED,
                    _ => INVALID
                },
                2 => J,
                3 => JAL,
                4 => BEQ,
                5 => BNE,
                6 => BLEZ,
                7 => BGTZ,
                8 => instruction.RT != 0 ? ADDI : NOP,
                9 => instruction.RT != 0 ? ADDIU : NOP,
                10 => instruction.RT != 0 ? SLTI : NOP,
                11 => instruction.RT != 0 ? SLTIU : NOP,
                12 => instruction.RT != 0 ? ANDI : NOP,
                13 => instruction.RT != 0 ? ORI : NOP,
                14 => instruction.RT != 0 ? XORI : NOP,
                15 => instruction.RT != 0 ? LUI : NOP,
                // COPROCESSOR 0
                16 => instruction.RS switch
                {
                    0 => instruction.RT != 0 ? MFC0 : NOP,
                    4 => MTC0,
                    16 => instruction.Funct switch
                    {
                        1 => TLBR,
                        2 => TLBWI,
                        6 => TLBWR,
                        8 => TLBP,
                        24 => ERET,
                        0 or 3 or 4 or 5 or 7 or (>= 9 and <= 23) or (>= 25 and <= 63) => RESERVED,
                        _ => INVALID
                    },
                    1 or 2 or 3 or (>= 5 and <= 15) or (>= 17 and <= 31) => RESERVED,
                    _ => INVALID
                },
                // COPROCESSOR 1
                17 => instruction.RS switch
                {
                    0 => instruction.RT != 0 ? MFC1 : NOP,
                    1 => instruction.RT != 0 ? DMFC1 : NOP,
                    2 => instruction.RT != 0 ? CFC1 : NOP,
                    4 => MTC1,
                    5 => DMTC1,
                    6 => CTC1,
                    8 => instruction.RT switch
                    {
                        0 => BC1F,
                        1 => BC1T,
                        2 => BC1FL,
                        3 => BC1TL,
                        _ => INVALID
                    },
                    16 => instruction.Funct switch
                    {
                        0 => ADD_S,
                        1 => SUB_S,
                        2 => MUL_S,
                        3 => DIV_S,
                        4 => SQRT_S,
                        5 => ABS_S,
                        6 => MOV_S,
                        7 => NEG_S,
                        8 => ROUND_L_S,
                        9 => TRUNC_L_S,
                        10 => CEIL_L_S,
                        11 => FLOOR_L_S,
                        12 => ROUND_W_S,
                        13 => TRUNC_W_S,
                        14 => CEIL_W_S,
                        15 => FLOOR_W_S,
                        33 => CVT_D_S,
                        36 => CVT_W_S,
                        37 => CVT_L_S,
                        48 => C_F_S,
                        49 => C_UN_S,
                        50 => C_EQ_S,
                        51 => C_UEQ_S,
                        52 => C_OLT_S,
                        53 => C_ULT_S,
                        54 => C_OLE_S,
                        55 => C_ULE_S,
                        56 => C_SF_S,
                        57 => C_NGLE_S,
                        58 => C_SEQ_S,
                        59 => C_NGL_S,
                        60 => C_LT_S,
                        61 => C_NGE_S,
                        62 => C_LE_S,
                        63 => C_NGT_S,
                        (>= 16 and <= 32) or 34 or 35 or (>= 38 and <= 47) => RESERVED,
                        _ => INVALID
                    },
                    17 => instruction.Funct switch
                    {
                        0 => ADD_D,
                        1 => SUB_D,
                        2 => MUL_D,
                        3 => DIV_D,
                        4 => SQRT_D,
                        5 => ABS_D,
                        6 => MOV_D,
                        7 => NEG_D,
                        8 => ROUND_L_D,
                        9 => TRUNC_L_D,
                        10 => CEIL_L_D,
                        11 => FLOOR_L_D,
                        12 => ROUND_W_D,
                        13 => TRUNC_W_D,
                        14 => CEIL_W_D,
                        15 => FLOOR_W_D,
                        32 => CVT_S_D,
                        36 => CVT_W_D,
                        37 => CVT_L_D,
                        48 => C_F_D,
                        49 => C_UN_D,
                        50 => C_EQ_D,
                        51 => C_UEQ_D,
                        52 => C_OLT_D,
                        53 => C_ULT_D,
                        54 => C_OLE_D,
                        55 => C_ULE_D,
                        56 => C_SF_D,
                        57 => C_NGLE_D,
                        58 => C_SEQ_D,
                        59 => C_NGL_D,
                        60 => C_LT_D,
                        61 => C_NGE_D,
                        62 => C_LE_D,
                        63 => C_NGT_D,
                        (>= 16 and <= 31) or 33 or 34 or 35 or (>= 38 and <= 47) => RESERVED,
                        _ => INVALID
                    },
                    20 => instruction.Funct switch
                    {
                        32 => CVT_S_W,
                        33 => CVT_D_W,
                        (>= 0 and <= 31) or (>= 34 and <= 63) => RESERVED,
                        _ => INVALID
                    },
                    21 => instruction.Funct switch
                    {
                        32 => CVT_S_L,
                        33 => CVT_D_L,
                        (>= 0 and <= 31) or (>= 34 and <= 63) => RESERVED,
                        _ => INVALID
                    },
                    3 or 7 or (>= 9 and <= 15) or 18 or 19 or (>= 22 and <= 31) => RESERVED,
                    _ => INVALID
                },
                //TODO: 18 - coprocessor 2
                20 => BEQL,
                21 => BNEL,
                22 => BLEZL,
                23 => BGTZL,
                24 => instruction.RT != 0 ? DADDI : NOP,
                25 => instruction.RT != 0 ? DADDIU : NOP,
                26 => instruction.RT != 0 ? LDL : NOP,
                27 => instruction.RT != 0 ? LDR : NOP,
                32 => instruction.RT != 0 ? LB : NOP,
                33 => instruction.RT != 0 ? LH : NOP,
                34 => instruction.RT != 0 ? LWL : NOP,
                35 => instruction.RT != 0 ? LW : NOP,
                36 => instruction.RT != 0 ? LBU : NOP,
                37 => instruction.RT != 0 ? LHU : NOP,
                38 => instruction.RT != 0 ? LWR : NOP,
                39 => instruction.RT != 0 ? LWU : NOP,
                40 => SB,
                41 => SH,
                42 => SWL,
                43 => SW,
                44 => SDL,
                45 => SDR,
                46 => SWR,
                47 => CACHE,
                48 => instruction.RT != 0 ? LL : NOP,
                49 => LWC1,
                50 => LWC2, // 1 1 0 0 z z
                51 => PREF,
                52 => LLD,
                53 => LDC1,
                54 => LDC2,
                55 => instruction.RT != 0 ? LD : NOP,
                56 => instruction.RT != 0 ? SC : NOP,
                57 => SWC1,
                58 => SWC2,
                60 => SCD,
                61 => SDC1,
                62 => SDC2,
                63 => SD,
                18 or 19 or 28 or 29 or 30 or 31 or 59 => RESERVED,
                _ => INVALID
            };

            return toOperation(instruction, file);
        }

        private Operation ABS_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Abs_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation ABS_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Abs_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation ADD(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Add, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation ADD_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Add_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation ADD_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Add_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation ADDI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Addi, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMMSigned:X} /* 0x{i.IMMSigned:X} = {i.IMMSigned} */");
        }

        private Operation ADDIU(in Instruction instruction, DataFile file)
        {
            //if (LoadLowRefCache.ContainsKey(instruction.VirtualAddress))
            //{
            //    return new Operation(EOperationCode.Addiu, instruction, file, (string name, in Instruction i) =>
            //    {
            //        var reference = LoadLowRefCache[i.VirtualAddress];
            //        return $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, {FormatOffset(GetLoadName(reference.Item1), reference.Item2)}";
            //    });
            //}
            //else
            //{
            return new Operation(EOperationCode.Addiu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMMSigned:X} /* 0x{i.IMMSigned:X} = {i.IMMSigned} */");
            //}
        }

        private Operation ADDU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Addu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation AND(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.And, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation ANDI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.And, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMM:X} /* 0x{i.IMM:X} = {Convert.ToString(i.IMMSigned, 2)} */");
        }

        private Operation BC1F(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bc1f, instruction, file, static (string name, in Instruction i)
                => $"{name}\t0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BC1FL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bc1fl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BC1T(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bc1t, instruction, file, static (string name, in Instruction i)
                => $"{name}\t0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BC1TL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bc1tl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BEQ(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Beq, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BEQL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Beql, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BGEZ(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bgez, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BGEZAL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bgezal, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BGEZALL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bgezall, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BGEZL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bgezl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BGTZ(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bgtz, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BGTZL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bgtzl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BLEZ(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Blez, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BLEZL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Blezl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BLTZ(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bltz, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BLTZAL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bltzal, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BLTZALL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bltzall, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BLTZL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bltzl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BNE(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bne, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BNEL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Bnel, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}, 0x{i.IMMSigned:X8} /* Absolute address = 0x{i.GetAbsoluteJumpAddress():X8} */");
        }

        private Operation BREAK(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Break, instruction, file, static (string name, in Instruction i)
                => $"{name} /* Break Code = 0x{i.BreakCode:X}");
        }

        private Operation C_F_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_F_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_F_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_F_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_UN_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_UN_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_UN_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_UN_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_EQ_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_EQ_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_EQ_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_EQ_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_UEQ_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_UEQ_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_UEQ_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_UEQ_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_OLT_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_OLT_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_OLT_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_OLT_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_ULT_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_ULT_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_ULT_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_ULT_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_OLE_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_OLE_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_OLE_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_OLE_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_ULE_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_ULE_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_ULE_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_ULE_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_SF_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_SF_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_SF_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_SF_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_NGLE_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_NGLE_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_NGLE_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_NGLE_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_SEQ_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_SEQ_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_SEQ_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_SEQ_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_NGL_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_NGL_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_NGL_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_NGL_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_LT_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_LT_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_LT_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_LT_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_NGE_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_NGE_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_NGE_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_NGE_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_LE_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_LE_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_LE_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_LE_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_NGT_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_NGT_D, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation C_NGT_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.C_NGT_S, instruction, file, static (string name, in Instruction i)
                => i.FloatConditionCode != 0
                    ? $"{name}\t{i.FloatConditionCode}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}"
                    : $"{name}\t{i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation CACHE(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Cache, instruction, file);
        }

        private Operation CEIL_L_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ceil_L_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CEIL_L_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ceil_L_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CEIL_W_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ceil_W_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CEIL_W_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ceil_W_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CFC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cfc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CTC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ctc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_D_L(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_D_L, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_D_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_D_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_D_W(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_D_W, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_L_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_L_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_L_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_L_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_S_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_S_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_S_L(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_S_L, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_S_W(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_S_W, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_W_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_W_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation CVT_W_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Cvt_W_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation DADD(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Dadd, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation DADDI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Daddi, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* 0x{i.IMMSigned:X} = {i.IMMSigned} */");
        }

        private Operation DADDIU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Daddiu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMMSigned:X8} /* 0x{i.IMMSigned:X} = {i.IMMSigned} */");
        }

        private Operation DADDU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Daddu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation DDIV(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ddiv, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation DDIVU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ddivu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation DIV(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Div, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation DIV_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Div_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation DIV_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Div_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}, {i.FT.FloatRegisterName()}");
        }

        private Operation DIVU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Divu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation DMFC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Dmfc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation DMTC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Dmtc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation DMULT(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dmult, instruction, file);
        }

        private Operation DMULTU(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dmultu, instruction, file);
        }

        private Operation DSLL(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsll, instruction, file);
        }

        private Operation DSLL32(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsll32, instruction, file);
        }

        private Operation DSLLV(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsllv, instruction, file);
        }

        private Operation DSRA(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsra, instruction, file);
        }

        private Operation DSRA32(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsra32, instruction, file);
        }

        private Operation DSRAV(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsrav, instruction, file);
        }

        private Operation DSRL(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsrl, instruction, file);
        }

        private Operation DSRL32(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsrl32, instruction, file);
        }

        private Operation DSRLV(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsrlv, instruction, file);
        }

        private Operation DSUB(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsub, instruction, file);
        }

        private Operation DSUBU(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Dsubu, instruction, file);
        }

        private Operation ERET(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Eret, instruction, file);
        }

        private Operation FLOOR_L_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Floor_L_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation FLOOR_L_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Floor_L_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation FLOOR_W_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Floor_W_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation FLOOR_W_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Floor_W_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation INVALID(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Invalid, instruction, file, static (string _, in Instruction i)
                => $"/* INVALID INSTRUCTION {Convert.ToString(i.Value, 2).PadLeft(32, '0')} */", new Exception($"The instruction {Convert.ToString(instruction.Value, 2).PadLeft(32, '0')} is not supported or is not valid."));
        }

        private Operation J(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.J, instruction, file, static (string name, in Instruction i)
                => $"{name}\t0x{i.Address:X8}");
        }

        private Operation JAL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Jal, instruction, file, static (string name, in Instruction i)
                => $"{name}\t0x{i.Address:X8}");
        }

        private Operation JALR(in Instruction instruction, DataFile file)
        {
            if (instruction.RS == 31)
            {
                return new Operation(EOperationCode.Jalr, instruction, file, static (string name, in Instruction i)
                    => $"{name}\t{i.RS.RegisterName()}");
            }
            else
            {
                return new Operation(EOperationCode.Jalr, instruction, file, static (string name, in Instruction i)
                    => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}");
            }
        }

        private Operation JR(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Jr, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}");
        }

        private Operation LB(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lb, instruction, file);
        }

        private Operation LBU(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lbu, instruction, file);
        }

        private Operation LD(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Ld, instruction, file);
        }

        private Operation LDC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ldc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FT.FloatRegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation LDC2(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Ldc2, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FT.FloatRegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation LDL(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Ldl, instruction, file);
        }

        private Operation LDR(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Ldr, instruction, file);
        }

        private Operation LH(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lh, instruction, file);
        }

        private Operation LHU(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lhu, instruction, file);
        }

        private Operation LL(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Ll, instruction, file);
        }

        private Operation LLD(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lld, instruction, file);
        }

        private Operation LUI(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lui, instruction, file);
        }

        private Operation LW(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Lw, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation LWC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Lwc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FT.FloatRegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation LWC2(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Lwc2, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FT.FloatRegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation LWL(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lwl, instruction, file);
        }

        private Operation LWR(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lwr, instruction, file);
        }

        private Operation LWU(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Lwu, instruction, file);
        }

        private Operation MFC0(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Mfc0, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation MFC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Mfc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation MFHI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Mfhi, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}");
        }

        private Operation MFLO(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Mflo, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}");
        }

        private Operation MOV_D(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Mov_D, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation MOV_S(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Mov_S, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.FD.FloatRegisterName()}, {i.FS.FloatRegisterName()}");
        }

        private Operation MOVN(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Movn, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation MOVZ(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Movz, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation MTC0(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Mtc0, instruction, file);
        }

        private Operation MTC1(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Mtc1, instruction, file);
        }

        private Operation MTHI(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Mthi, instruction, file);
        }

        private Operation MTLO(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Mtlo, instruction, file);
        }

        private Operation MUL_D(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Mul_D, instruction, file);
        }

        private Operation MUL_S(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Mul_S, instruction, file);
        }

        private Operation MULT(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Mult, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation MULTU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Multu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation NEG_D(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Neg_D, instruction, file);
        }

        private Operation NEG_S(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Neg_S, instruction, file);
        }

        private Operation NOP(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Nop, instruction, file, static (string name, in Instruction _)
                => name);
        }

        private Operation NOR(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Nor, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation OR(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Or, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation ORI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Nor, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMM:X}  /* 0x{i.IMM:X} = {Convert.ToString(i.IMM, 2)} */");
        }

        private Operation PREF(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Pref, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation RESERVED(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Invalid, instruction, file, static (string _, in Instruction i)
                => $"/* RESERVED INSTRUCTION {Convert.ToString(i.Value, 2).PadLeft(32, '0')} */", new Exception($"The instruction {Convert.ToString(instruction.Value, 2).PadLeft(32, '0')} is reserved or not implemented."));
        }

        private Operation ROUND_L_D(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Round_L_D, instruction, file);
        }

        private Operation ROUND_L_S(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Round_L_S, instruction, file);
        }

        private Operation ROUND_W_D(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Round_W_D, instruction, file);
        }

        private Operation ROUND_W_S(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Round_W_S, instruction, file);
        }

        private Operation SB(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sb, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SC(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sc, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SCD(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Scd, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SD(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sd, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SDC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sdc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SDC2(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sdc2, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SDL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sdl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SDR(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sdr, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SH(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sh, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SLL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sll, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RT.RegisterName()}, {i.Shift}");
        }

        private Operation SLLV(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sllv, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RT.RegisterName()}, {i.RS.RegisterName()}");
        }

        private Operation SLT(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Slt, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation SLTI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Slti, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMMSigned:X} /* 0x{i.IMMSigned:X} = {i.IMMSigned} */");
        }

        private Operation SLTIU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sltiu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMMSigned:X} /* 0x{i.IMMSigned:X} = {i.IMMSigned} */");
        }

        private Operation SLTU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Slti, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation SQRT_D(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Sqrt_D, instruction, file);
        }

        private Operation SQRT_S(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Sqrt_S, instruction, file);
        }

        private Operation SRA(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sra, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RT.RegisterName()}, {i.Shift}");
        }

        private Operation SRAV(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sra, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RT.RegisterName()}, {i.RS.RegisterName()}");
        }

        private Operation SRL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Srl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RT.RegisterName()}, {i.Shift}");
        }

        private Operation SRLV(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Srlv, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RT.RegisterName()}, {i.RS.RegisterName()}");
        }

        private Operation SUB(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sub, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation SUB_D(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Sub_D, instruction, file);
        }

        private Operation SUB_S(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Sub_S, instruction, file);
        }

        private Operation SUBU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Subu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation SW(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sw, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SWC1(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Swc1, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SWC2(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Swc2, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SWL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Swl, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SWR(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Swr, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, 0x{i.IMMSigned:X}({i.RS.RegisterName()})");
        }

        private Operation SYNC(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Sync, instruction, file, static (string name, in Instruction i)
                => $"{name}\t0x{i.Shamd:X} /* {i.Shamd} */");
        }

        private Operation SYSCALL(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Syscall, instruction, file, static (string name, in Instruction i)
                => $"{name} /* Syscall Code = 0x{i.SyscallCode:X}");
        }

        private Operation TEQ(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Teq, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()} /* Trap code = 0x{i.CompareTrapCode:X} */");
        }

        private Operation TEQI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Teqi, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X} /* 0x{i.IMMSigned} = {i.IMMSigned}");
        }

        private Operation TGE(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tge, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()} /* Trap code = 0x{i.CompareTrapCode:X} */");
        }

        private Operation TGEI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tgei, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X} /* 0x{i.IMMSigned} = {i.IMMSigned}");
        }

        private Operation TGEIU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tgeiu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMM:X} /* 0x{i.IMM} = {i.IMM}");
        }

        private Operation TGEU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tgeu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()} /* Trap code = 0x{i.CompareTrapCode:X} */");
        }

        private Operation TLBP(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Tlbp, instruction, file);
        }

        private Operation TLBR(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Tlbr, instruction, file);
        }

        private Operation TLBWI(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Tlbwi, instruction, file);
        }

        private Operation TLBWR(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Tlbwr, instruction, file);
        }

        private Operation TLT(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tlt, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()} /* Trap code = 0x{i.CompareTrapCode:X} */");
        }

        private Operation TLTI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tlti, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X} /* 0x{i.IMMSigned} = {i.IMMSigned}");
        }

        private Operation TLTIU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tlti, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMM:X} /* 0x{i.IMM} = {i.IMM}");
        }

        private Operation TLTU(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tltu, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()} /* Trap code = 0x{i.CompareTrapCode:X} */");
        }

        private Operation TNE(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tne, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, {i.RT.RegisterName()} /* Trap code = 0x{i.CompareTrapCode:X} */");
        }

        private Operation TNEI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Tlti, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RS.RegisterName()}, 0x{i.IMMSigned:X} /* 0x{i.IMMSigned} = {i.IMMSigned}");
        }

        private Operation TRUNC_L_D(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Trunc_L_D, instruction, file);
        }

        private Operation TRUNC_L_S(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Trunc_L_S, instruction, file);
        }

        private Operation TRUNC_W_D(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Trunc_W_D, instruction, file);
        }

        private Operation TRUNC_W_S(in Instruction instruction, DataFile file)
        {
            return Operation.NotImplemented(EOperationCode.Trunc_W_S, instruction, file);
        }

        private Operation XOR(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Xor, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RD.RegisterName()}, {i.RS.RegisterName()}, {i.RT.RegisterName()}");
        }

        private Operation XORI(in Instruction instruction, DataFile file)
        {
            return new Operation(EOperationCode.Xori, instruction, file, static (string name, in Instruction i)
                => $"{name}\t{i.RT.RegisterName()}, {i.RS.RegisterName()}, 0x{i.IMM} /* 0x{i.IMM:X} = {Convert.ToString(i.IMM, 2)} */");
        }
    }
}
