using System.ComponentModel;
using Foundry.Media.Nintendo64.Rom.Disassembly.Internal;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// Operation codes.
    /// </summary>
    public enum EOperationCode : byte
    {
        /*
         W: word (32bits integer)
         L: long (64bits integer)
         S: Single precision (32bits float)
         D: double precision  (64bits float)
        */

        [Operation("INVALID", "INVALID", null, "Invalid instruction.")]
        [Description("Invalid instruction.")]
        Invalid,

        [Operation("RESERVED", "RESERVED", null, "Reserved instruction.")]
        [Description("Reserved instruction.")]
        Reserved,

        [Operation("abs.d", "Floating-Point Absolute Value", "FD, FS", "FD = abs(FS)")]
        [Description("The absolute value of the 64-bit floating point value stored in FS is placed into FD. The operation is arithmetic; a NaN operand " +
                     "signals invalid operation.")]
        [Restriction("FS and FD must specify registers that are valid for 64-bit floating point values. If they are not valid, the result is undefined.")]
        [Restriction("The operand in FS must be a 64-bit floating point value. If it is not, the result is undefined.")]
        Abs_D,

        [Operation("abs.s", "Floating-Point Absolute Value", "FD, FS", "FD = abs(FS)")]
        [Description("The absolute value of the 32-bit floating point value stored in FS is placed into FD. The operation is arithmetic; a NaN operand " +
                     "signals invalid operation.")]
        [Restriction("FS and FD must specify registers that are valid for 32-bit floating point values. If they are not valid, the result is undefined.")]
        [Restriction("The operand in FS must be a 32-bit floating point value. If it is not, the result is undefined.")]
        Abs_S,

        [Operation("add", "Add Word", "RD, RS, RT", "RD = RS + RT")]
        [Description("The 32-bit value in RT is added to the 32-bit value in RS to produce a 32-bit result. If the addition results " +
                     "in 32-bit 2s complement arithmetic overflow then the destination register is not modified and an integer overflow " +
                     "exception occurs. If it does not overflow, the 32-bit result is placed into RD.")]
        [Restriction("On 64-bit processors, if either RT or RS do not contain sign-extended 32-bit values, the result of the operation is undefined.")]
        Add,

        [Operation("add.d", "Floating-Point Add", "FD, FS, FT", "FD = FS + FT")]
        [Description("The value of the 64-bit floating point value stored in FT is added to the 64-bit floating point value stored in FS, and the 64-bit " +
                     "floating point result is placed into FD.")]
        [Restriction("FS, FT, and FD must specify registers that are valid for 64-bit floating point values. If they are not valid, the result is undefined.")]
        [Restriction("The operands in FS and FT must be a 64-bit floating point values. If it is not, the result is undefined.")]
        Add_D,

        [Operation("add.s", "Floating-Point Add", "FD, FS, FT", "FD = FS + FT")]
        [Description("The value of the 32-bit floating point value stored in FT is added to the 32-bit floating point value stored in FS, and the 32-bit " +
                     "floating point result is placed into FD.")]
        [Restriction("FS, FT, and FD must specify registers that are valid for 32-bit floating point values. If they are not valid, the result is undefined.")]
        [Restriction("The operands in FS and FT must be a 32-bit floating point values. If it is not, the result is undefined.")]
        Add_S,

        [Operation("addi", "Add Immediate Word", "RT, RS, Immediate", "RT = RS + Immediate")]
        [Description("The 16-bit signed immediate value is added to the 32-bit value in RS to produce a 32-bit result. If the addition results " +
                     "in 32-bit 2s complement arithmetic overflow then the destination register is not modified and an integer overflow " +
                     "exception occurs. If it does not overflow, the 32-bit result is placed into RT.")]
        [Restriction("On 64-bit processors, if RS does not contain a sign-extended 32-bit value, the result of the operation is undefined.")]
        Addi,

        [Operation("addiu", "Add Immediate Unsigned Word", "RT, RS, Immediate", "RT = RS + Immediate",
            Misnomer = "The term \"unsigned\" is a misnomer; this operation is 32-bit modulo arithmetic that does not trap on overflow.")]
        [Description("The 16-bit signed immediate value is added to the 32-bit value in RS to produce a 32-bit result.")]
        [Restriction("On 64-bit processors, if RS does not contain a sign-extended 32-bit value, the result of the operation is undefined.")]
        Addiu,

        [Operation("addu", "Add Unsigned Word", "RD, RS, RT", "RD = RS + RT",
            Misnomer = "The term \"unsigned\" is a misnomer; this operation is 32-bit modulo arithmetic that does not trap on overflow.")]
        [Description("The 32-bit value in RT is added to the 32-bit value in RS to produce a 32-bit result.")]
        [Restriction("On 64-bit processors, if either RT or RS do not contain sign-extended 32-bit values, the result of the operation is undefined.")]
        Addu,

        [Operation("and", "And", "RD, RS, RT", "RD = RS & RT")]
        [Description("The value in RT is logically ANDed with the value in RS.")]
        And,

        [Operation("andi", "And", "RT, RS, Immedate", "RT = RS & Immedate")]
        [Description("The value in RT is logically ANDed with the 16-bit, zero extended Immediate value.")]
        Andi,

        Bc1f,
        Bc1fl,
        Bc1t,
        Bc1tl,
        Beq,
        Beql,
        Bgez,
        Bgezal,
        Bgezall,
        Bgezl,
        Bgtz,
        Bgtzl,
        Blez,
        Blezl,
        Bltz,
        Bltzal,
        Bltzall,
        Bltzl,
        Bne,
        Bnel,
        Break,
        Cache,
        C_F_D,
        C_F_S,
        C_UN_D,
        C_UN_S,
        C_EQ_D,
        C_EQ_S,
        C_UEQ_D,
        C_UEQ_S,
        C_OLT_D,
        C_OLT_S,
        C_ULT_D,
        C_ULT_S,
        C_OLE_D,
        C_OLE_S,
        C_ULE_D,
        C_ULE_S,
        C_SF_D,
        C_SF_S,
        C_NGLE_D,
        C_NGLE_S,
        C_SEQ_D,
        C_SEQ_S,
        C_NGL_D,
        C_NGL_S,
        C_LT_D,
        C_LT_S,
        C_NGE_D,
        C_NGE_S,
        C_LE_D,
        C_LE_S,
        C_NGT_D,
        C_NGT_S,
        Ceil_L_D,
        Ceil_L_S,
        Ceil_W_D,
        Ceil_W_S,
        Cfc1,
        Ctc1,
        Cvt_D_L,
        Cvt_D_S,
        Cvt_D_W,
        Cvt_L_D,
        Cvt_L_S,
        Cvt_S_D,
        Cvt_S_L,
        Cvt_S_W,
        Cvt_W_D,
        Cvt_W_S,
        Dadd,
        Daddi,
        Daddiu,
        Daddu,
        Ddiv,
        Ddivu,
        Div,
        Div_D,
        Div_S,
        Divu,
        Dmfc1,
        Dmtc1,
        Dmult,
        Dmultu,
        Dsll,
        Dsll32,
        Dsllv,
        Dsra,
        Dsra32,
        Dsrav,
        Dsrl,
        Dsrl32,
        Dsrlv,
        Dsub,
        Dsubu,
        Eret,
        Floor_L_D,
        Floor_L_S,
        Floor_W_D,
        Floor_W_S,
        J,
        Jal,
        Jalr,
        Jr,
        Lb,
        Lbu,
        Ld,
        Ldc1,
        Ldc2,
        Ldl,
        Ldr,
        Lh,
        Lhu,
        Ll,
        Lld,
        Lui,
        Lw,
        Lwc1,
        Lwc2,
        Lwl,
        Lwr,
        Lwu,
        Mfc0,
        Mfc1,
        Mfhi,
        Mflo,
        Mov_D,
        Mov_S,
        Movn,
        Movz,
        Mtc0,
        Mtc1,
        Mthi,
        Mtlo,
        Mul_D,
        Mul_S,
        Mult,
        Multu,
        Neg_D,
        Neg_S,
        Nop,
        Nor,
        Or,
        Ori,
        Pref,
        Round_L_D,
        Round_L_S,
        Round_W_D,
        Round_W_S,
        Sb,
        Sc,
        Scd,
        Sd,
        Sdc1,
        Sdc2,
        Sdl,
        Sdr,
        Sh,
        Sll,
        Sllv,
        Slt,
        Slti,
        Sltiu,
        Sltu,
        Sqrt_D,
        Sqrt_S,
        Sra,
        Srav,
        Srl,
        Srlv,
        Sub,
        Sub_D,
        Sub_S,
        Subu,
        Sw,
        Swc1,
        Swc2,
        Swl,
        Swr,
        Sync,
        Syscall,
        Teq,
        Teqi,
        Tge,
        Tgei,
        Tgeiu,
        Tgeu,
        Tlbp,
        Tlbr,
        Tlbwi,
        Tlbwr,
        Tlt,
        Tlti,
        Tltiu,
        Tltu,
        Tne,
        Tnei,
        Trunc_L_D,
        Trunc_L_S,
        Trunc_W_D,
        Trunc_W_S,
        Xor,
        Xori
    }

    /// <summary>
    /// Operation codes.
    /// </summary>
    //public static class OperationCode
    //{
    //    [Operation("beq", "Branch on Equal", "RS, RT, offset", "if (RS = RT) then branch", "if ({0} = {1}) jump to {2}", 0b000100_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS and RT are equal, branch to the effective " +
    //                          "target address after the instruction in the delay slot is executed.")]
    //    public const uint Beq = 0b000100_00000_00000_0000000000000000;

    //    [Operation("beql", "Branch on Equal Likely", "RS, RT, offset", "if (RS = RT) then branch (likely)", "if ({0} = {1}) jump to {2}", 0b010100_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS and RT are equal, branch to the effective " +
    //                          "target address after the instruction in the delay slot is executed. If the branch is not taken, the instruction " +
    //                          "in the delay slot is not executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Beql = 0b010100_00000_00000_0000000000000000;

    //    [Operation("bgez", "Branch on Greater Than or Equal to Zero", "RS, offset", "if (RS >= 0) then branch", "if ({0} >= 0) jump to {1}", 0b000001_00000_00001_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS are greater than or than zero, branch to the " +
    //                          "effective target address after the instruction in the delay slot is executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.")]
    //    public const uint Bgez = 0b000001_00000_00001_0000000000000000;

    //    [Operation("bgezal", "Branch on Greater Than or Equal to Zero and Link", "RS, offset", "if (RS >= 0) then procedure call", "if ({0} >= 0) call procedure {1}", 0b000001_00000_10001_0000000000000000,
    //            Description = "Place the return address link in $31. The return link is the address of the second instruction following the " +
    //                          "branch, where execution would continue after a procedure call. An 18-bit signed offset (the 16-bit offset field " +
    //                          "is shifted left 2 bits) is added to the address of the instruction following the branch (not the branch itself), " +
    //                          "in the branch delay slot, to form a PC-relative effective target address. If the contents of RS are greater than " +
    //                          "or than zero, branch to the effective target address after the instruction in the delay slot is executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use JAL or JALR instead. " +
    //                          "$31 must not be used for RS because such an instruction does not have the same effect when re-executed.")]
    //    public const uint Bgezal = 0b000001_00000_10001_0000000000000000;

    //    [Operation("bgezall", "Branch on Greater Than or Equal to Zero and Link Likely", "RS, offset", "if (RS >= 0) then procedure call (likely)", "if ({0} >= 0) call procedure {1}", 0b000001_00000_10011_0000000000000000,
    //            Description = "Place the return address link in $31. The return link is the address of the second instruction following the " +
    //                          "branch, where execution would continue after a procedure call. An 18-bit signed offset (the 16-bit offset field " +
    //                          "is shifted left 2 bits) is added to the address of the instruction following the branch (not the branch itself), " +
    //                          "in the branch delay slot, to form a PC-relative effective target address. If the contents of RS are greater than " +
    //                          "or than zero, branch to the effective target address after the instruction in the delay slot is " +
    //                          "executed. If the branch is not taken, the instruction in the delay slot is not executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use JAL or JALR instead. " +
    //                          "$31 must not be used for RS because such an instruction does not have the same effect when re-executed.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Bgezall = 0b000001_00000_10011_0000000000000000;

    //    [Operation("bgezl", "Branch on Greater Than or Equal to Zero Likely", "RS, offset", "if (RS >= 0) then branch (likely)", "if ({0} >= 0) jump to {1}", 0b000001_00000_00011_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS are greater than or than zero, branch to the " +
    //                          "effective target address after the instruction in the delay slot is executed. If the branch is not taken, the " +
    //                          "instruction in the delay slot is not executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Bgezl = 0b000001_00000_00011_0000000000000000;

    //    [Operation("bgtz", "Branch on Greater Than Zero", "RS, offset", "if (RS > 0) then branch", "if ({0} >= 0) jump to {1}", 0b000111_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS are greater than zero, branch to the " +
    //                          "effective target address after the instruction in the delay slot is executed. If the branch is not taken, the " +
    //                          "instruction in the delay slot is not executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.")]
    //    public const uint Bgtz = 0b000111_00000_00000_0000000000000000;

    //    [Operation("bgtzl", "Branch on Greater Than Zero Likely", "RS, offset", "if (RS > 0) then branch (likely)", "if ({0} >= 0) jump to {1}", 0b010111_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS are greater than zero, branch to the " +
    //                          "effective target address after the instruction in the delay slot is executed. If the branch is not taken, the " +
    //                          "instruction in the delay slot is not executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Bgtzl = 0b010111_00000_00000_0000000000000000;

    //    [Operation("blez", "Branch on Less Than or Equal to Zero", "RS, offset", "if (RS >= 0) then branch", "if ({0} >= 0) jump to {1}", 0b000110_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS are greater than or than zero, branch to the " +
    //                          "effective target address after the instruction in the delay slot is executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.")]
    //    public const uint Blez = 0b000110_00000_00000_0000000000000000;

    //    [Operation("blezl", "Branch on Less Than or Equal to Zero Likely", "RS, offset", "if (RS <= 0) then branch (likely)", "if ({0} <= 0) jump to {1}", 0b010110_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS are less than or equal to zero, branch to the " +
    //                          "effective target address after the instruction in the delay slot is executed. If the branch is not taken, the " +
    //                          "instruction in the delay slot is not executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Blezl = 0b010110_00000_00000_0000000000000000;

    //    [Operation("bltz", "Branch on Less Than Zero", "RS, offset", "if (RS < 0) then branch", "if ({0} < 0) jump to {1}", 0b000001_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS are less than zero, branch to the effective " +
    //                          "target address after the instruction in the delay slot is executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.")]
    //    public const uint Bltz = 0b000001_00000_00000_0000000000000000;

    //    [Operation("bltzal", "Branch on Less Than Zero and Link", "RS, offset", "if (RS < 0) then branch", "if ({0} < 0) call procedure {1}", 0b000001_00000_10000_0000000000000000,
    //            Description = "Place the return address link in $31. The return link is the address of the second instruction following the " +
    //                          "branch, where execution would continue after a procedure call. An 18-bit signed offset (the 16-bit offset field" +
    //                          "is shifted left 2 bits) is added to the address of the instruction following the branch (not the branch itself), " +
    //                          "in the branch delay slot, to form a PC-relative effective target address. If the contents of RS are less than zero, " +
    //                          "branch to the effective target address after the instruction in the delay slot is executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use JAL or JALR instead.")]
    //    public const uint Bltzal = 0b000001_00000_10000_0000000000000000;

    //    [Operation("bltzall", "Branch on Less Than Zero and Link Likely", "RS, offset", "if (RS < 0) then branch (likely)", "if ({0} < 0) call procedure {1}", 0b000001_00000_10010_0000000000000000,
    //            Description = "Place the return address link in $31. The return link is the address of the second instruction following the " +
    //                          "branch, where execution would continue after a procedure call. An 18-bit signed offset (the 16-bit offset field" +
    //                          "is shifted left 2 bits) is added to the address of the instruction following the branch (not the branch itself), " +
    //                          "in the branch delay slot, to form a PC-relative effective target address. If the contents of RS are less than zero, " +
    //                          "branch to the effective target address after the instruction in the delay slot is executed. If the branch is not taken, the " +
    //                          "instruction in the delay slot is not executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use JAL or JALR instead.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Bltzall = 0b000001_00000_10010_0000000000000000;

    //    [Operation("bltzl", "Branch on Less Than Zero Likely", "RS, offset", "if (RS < 0) then branch (likely)", "if ({0} < 0) jump to {1}", 0b000001_00000_00010_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the " +
    //                          "instruction following the branch (not the branch itself), in the branch delay slot, to form a PC-relative " +
    //                          "effective target address. If the contents of RS are less than zero, branch to the effective " +
    //                          "target address after the instruction in the delay slot is executed. If the branch is not taken, the instruction " +
    //                          "in the delay slot is not executed.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Bltzl = 0b000001_00000_00010_0000000000000000;

    //    [Operation("bne", "Branch on Not Equal", "RS, RT, offset", "if (RS != RT) then branch", "if ({0} != {1}) jump to {2}", 0b000101_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the instruction " +
    //                          "following the branch (not the branch itself), in the branch delay slot, to form a PC-relative effective target address. " +
    //                          "If the contents of RS and RT are not equal, branch to the effective target address after the instruction in the delay " +
    //                          "slot is executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.")]
    //    public const uint Bne = 0b000101_00000_00000_0000000000000000;

    //    [Operation("bnel", "Branch on Not Equal Likely", "RS, RT, offset", "if (RS != RT) then branch", "if ({0} != {1}) jump to {2}", 0b010101_00000_00000_0000000000000000,
    //            Description = "An 18-bit signed offset (the 16-bit offset field is shifted left 2 bits) is added to the address of the instruction " +
    //                          "following the branch (not the branch itself), in the branch delay slot, to form a PC-relative effective target address. " +
    //                          "If the contents of RS and RT are not equal, branch to the effective target address after the instruction in the delay " +
    //                          "slot is executed. If the branch is not taken, the instruction in the delay slot is not executed.",
    //            Restriction = "The conditional branch range is plus or minus 128KBytes. If a larger jump is needed, use J or JR instead.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Bnel = 0b010101_00000_00000_0000000000000000;

    //    [Operation("break", "Break", null, "if (RS != RT) then branch", "if ({0} != {1}) jump to {2}", 0b000000_00000000000000000000_001101,
    //            Description = "A breakpoint exception occurs, immediately and unconditionally transferring control to the exception handler. " +
    //                          "The break code is available in the middle 20 bits.")]
    //    public const uint Break = 0b000000_00000000000000000000_001101;

    //    [Operation("dadd", "Doubleword Add", "RD, RS, RT", "RD = RS + RT", "{0} = {1} + {2}", 0b000000_00000_00000_00000_00000_100000,
    //            Description = "The 64-bit value in RT is added to the 64-bit value in RS to produce a 64-bit result. If the addition results " +
    //                          "in 64-bit 2s complement arithmetic overflow then the destination register is not modified and an integer overflow " +
    //                          "exception occurs. If it does not overflow, the 64-bit result is placed into RD.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dadd = 0b000000_00000_00000_00000_00000_101100;

    //    [Operation("daddi", "Doubleword Add Immediate", "RT, RS, Immediate", "RT = RS + Immediate", "{0} = {1} + {2}", 0b011000_00000_00000_0000000000000000,
    //            Description = "The 16-bit signed immediate value is added to the 64-bit value in RS to produce a 64-bit result. If the addition results " +
    //                          "in 64-bit 2s complement arithmetic overflow then the destination register is not modified and an integer overflow " +
    //                          "exception occurs. If it does not overflow, the 64-bit result is placed into RT.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Daddi = 0b011000_00000_00000_0000000000000000;

    //    [Operation("daddiu", "Doublrword Add Immediate Unsigned", "RT, RS, Immediate", "RT = RS + Immediate", "{0} = {1} + {2}", 0b011001_00000_00000_0000000000000000,
    //            Description = "The 16-bit signed immediate value is added to the 64-bit value in RS to produce a 64-bit result.",
    //            Misnomer = "The term \"unsigned\" is a misnomer; this operation is 64-bit modulo arithmetic that does not trap on overflow.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Daddiu = 0b011001_00000_00000_0000000000000000;

    //    [Operation("daddu", "Doubleword Add Unsigned", "RD, RS, RT", "RD = RS + RT", "{0} = {1} + {2}", 0b000000_00000_00000_00000_00000_101101,
    //            Description = "The 64-bit value in RT is added to the 64-bit value in RS to produce a 64-bit result.",
    //            Misnomer = "The term \"unsigned\" is a misnomer; this operation is 64-bit modulo arithmetic that does not trap on overflow.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Daddu = 0b000000_00000_00000_00000_00000_101101;

    //    [Operation("ddiv", "Doubleword Divide", "RS, RT", "(LO, HI) = RS / RT", "(LO, HI) = {0} / {1}", 0b000000_00000_00000_0000000000_011110,
    //            Description = "The 64-bit value in RS is divided by the 64-bit value in RT, treating both operands as signed " +
    //                          "values. The 64-bit quotient is placed into LO and the 64-bit remainder is placed into HI.",
    //            Restriction = "If either of the two preceding instructions is MFHI or MFLO, the result of the MFHI or MFLO is undefined. " +
    //                          "Reads of the HI or LO registers must be separated from subsequent instructions that write to them by two or " +
    //                          "more other instructions. If RT is zero, the result value is undefined.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Ddiv = 0b000000_00000_00000_0000000000_011110;

    //    [Operation("ddivu", "Doubleword Divide Unsigned", "RS, RT", "(LO, HI) = RS / RT", "(LO, HI) = {0} / {1}", 0b000000_00000_00000_0000000000_011111,
    //            Description = "The 64-bit value in RS is divided by the 64-bit value in RT, treating both operands as unsigned " +
    //                          "values. The 64-bit quotient is placed into LO and the 64-bit remainder is placed into HI.",
    //            Restriction = "If either of the two preceding instructions is MFHI or MFLO, the result of the MFHI or MFLO is undefined. " +
    //                          "Reads of the HI or LO registers must be separated from subsequent instructions that write to them by two or " +
    //                          "more other instructions. If RT is zero, the result value is undefined.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Ddivu = 0b000000_00000_00000_0000000000_011111;

    //    [Operation("div", "Divide Word", "RS, RT", "(LO, HI) = RS / RT", "(LO, HI) = {0} / {1}", 0b000000_00000_00000_0000000000_011010,
    //            Description = "The 32-bit value in RS is divided by the 32-bit value in RT, treating both operands as signed " +
    //                          "values. The 32-bit quotient is placed into LO and the 32-bit remainder is placed into HI.",
    //            Restriction = "If either of the two preceding instructions is MFHI or MFLO, the result of the MFHI or MFLO is undefined. " +
    //                          "Reads of the HI or LO registers must be separated from subsequent instructions that write to them by two or " +
    //                          "more other instructions. On 64-bit processors, if either RT or RS do not contain sign-extended 32-bit values, the result of the " +
    //                          "operation is undefined. If RT is zero, the result value is undefined.")]
    //    public const uint Div = 0b000000_00000_00000_0000000000_011010;

    //    [Operation("divu", "Divide Word Unsigned", "RS, RT", "(LO, HI) = RS / RT", "(LO, HI) = {0} / {1}", 0b000000_00000_00000_0000000000_011011,
    //            Description = "The 32-bit value in RS is divided by the 32-bit value in RT, treating both operands as unsigned " +
    //                          "values. The 32-bit quotient is placed into LO and the 32-bit remainder is placed into HI.",
    //            Restriction = "If either of the two preceding instructions is MFHI or MFLO, the result of the MFHI or MFLO is undefined. " +
    //                          "Reads of the HI or LO registers must be separated from subsequent instructions that write to them by two or " +
    //                          "more other instructions. On 64-bit processors, if either RT or RS do not contain sign-extended 32-bit values, the result of the " +
    //                          "operation is undefined. If RT is zero, the result value is undefined.")]
    //    public const uint Divu = 0b000000_00000_00000_0000000000_011011;

    //    [Operation("dmult", "Doubleword Multiply", "RS, RT", "(LO, HI) = RS x RT", "(LO, HI) = {0} x {1}", 0b000000_00000_00000_0000000000_011100,
    //            Description = "The 64-bit value in RS is multiplied by the 64-bit value in RT, treating both operands as signed " +
    //                          "values to produce a 128-bit result. The low-order 64-bits of the result are placed into LO and the " +
    //                          "high-order 64-bits remainder are placed into HI.",
    //            Restriction = "If either of the two preceding instructions is MFHI or MFLO, the result of the MFHI or MFLO is undefined. " +
    //                          "Reads of the HI or LO registers must be separated from subsequent instructions that write to them by two or " +
    //                          "more other instructions. If RT is zero, the result value is undefined.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dmult = 0b000000_00000_00000_0000000000_011110;

    //    [Operation("dmultu", "Doubleword Multiply Unsigned", "RS, RT", "(LO, HI) = RS x RT", "(LO, HI) = {0} x {1}", 0b000000_00000_00000_0000000000_011101,
    //            Description = "The 64-bit value in RS is multiplied by the 64-bit value in RT, treating both operands as unsigned " +
    //                          "values to produce a 128-bit result. The low-order 64-bits of the result are placed into LO and the " +
    //                          "high-order 64-bits remainder are placed into HI.",
    //            Restriction = "If either of the two preceding instructions is MFHI or MFLO, the result of the MFHI or MFLO is undefined. " +
    //                          "Reads of the HI or LO registers must be separated from subsequent instructions that write to them by two or " +
    //                          "more other instructions. If RT is zero, the result value is undefined.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dmultu = 0b000000_00000_00000_0000000000_011101;

    //    [Operation("dsll", "Doubleword Shift Left Logical", "RD, RT, Shift", "RD = RT << Shift", "{0} = {1} << {2}", 0b000000_00000_00000_00000_00000_111000,
    //            Description = "The 64-bit value of RT is shifted left by the amount specified in Shift, inserting zeros into the emptied bits.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsll = 0b000000_00000_00000_00000_00000_111000;

    //    [Operation("dsll32", "Doubleword Shift Left Logical Plus 32", "RD, RT, Shift", "RD = RT << (Shift + 32)", "{0} = {1} << ({2} + 32)", 0b000000_00000_00000_00000_00000_111100,
    //            Description = "The 64-bit value of RT is shifted left by the amount specified in Shift plus 32, inserting zeros into the emptied bits.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsll32 = 0b000000_00000_00000_00000_00000_111100;

    //    [Operation("dsllv", "Doubleword Shift Left Logical Variable", "RD, RT, Shift", "RD = RT << RS", "{0} = {1} << {2}", 0b000000_00000_00000_00000_00000_010100,
    //            Description = "The 64-bit value of RT is shifted left by the amount specified in RT, inserting zeros into the emptied bits." +
    //                          "The bit shift count in the range of 0 to 63 is specified by the low-order six bits in RS.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsllv = 0b000000_00000_00000_00000_00000_010100;

    //    [Operation("dsra", "Doubleword Shift Right Arithmetic", "RD, RT, Shift", "RD = RT >> Shift", "{0} = {1} >> {2}", 0b000000_00000_00000_00000_00000_111011,
    //            Description = "The 64-bit value of RT is shifted right by the amount specified in Shift, inserting zeros into the emptied bits.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsra = 0b000000_00000_00000_00000_00000_111011;

    //    [Operation("dsra32", "Doubleword Shift Right Arithmetic Plus 32", "RD, RT, Shift", "RD = RT >> (Shift + 32)", "{0} = {1} >> ({2} + 32)", 0b000000_00000_00000_00000_00000_111111,
    //            Description = "The 64-bit value of RT is shifted right by the amount specified in Shift plus 32, inserting zeros into the emptied bits.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsra32 = 0b000000_00000_00000_00000_00000_111111;

    //    [Operation("dsrav", "Doubleword Shift Right Arithmetic Variable", "RD, RT, RS", "RD = RT >> RS", "{0} = {1} >> {2}", 0b000000_00000_00000_00000_00000_010111,
    //            Description = "The 64-bit value of RT is shifted right by the amount specified in RT, inserting the sign bit into the emptied bits." +
    //                          "The bit shift count in the range of 0 to 63 is specified by the low-order six bits in RS.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsrav = 0b000000_00000_00000_00000_00000_010111;

    //    [Operation("dsrl", "Doubleword Shift Right Logical", "RD, RT, Shift", "RD = RT >> Shift", "{0} = {1} >> {2}", 0b000000_00000_00000_00000_00000_111010,
    //            Description = "The 64-bit value of RT is shifted right by the amount specified in Shift, inserting zeros into the emptied bits.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsrl = 0b000000_00000_00000_00000_00000_111010;

    //    [Operation("dsrl32", "Doubleword Shift Right Logical Plus 32", "RD, RT, Shift", "RD = RT >> (Shift + 32)", "{0} = {1} >> ({2} + 32)", 0b000000_00000_00000_00000_00000_111110,
    //            Description = "The 64-bit value of RT is shifted right by the amount specified in Shift plus 32, inserting zeros into the emptied bits.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsrl32 = 0b000000_00000_00000_00000_00000_111110;

    //    [Operation("dsrlv", "Doubleword Shift Right Logical Variable", "RD, RT, RS", "RD = RT >> RS", "{0} = {1} >> {2}", 0b000000_00000_00000_00000_00000_010110,
    //            Description = "The 64-bit value of RT is shifted right by the amount specified in RT, inserting the sign bit into the emptied bits." +
    //                          "The bit shift count in the range of 0 to 63 is specified by the low-order six bits in RS.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsrlv = 0b000000_00000_00000_00000_00000_010110;

    //    [Operation("dsub", "Doubleword Subtract", "RD, RS, RT", "RD = RS - RT", "{0} = {1} - {2}", 0b000000_00000_00000_00000_00000_101110,
    //            Description = "The 64-bit value of RT is subtracted from the 64-bit value in RS to produce a 64-bit result. If the subtraction results " +
    //                          "in 64-bit 2s complement arithmetic overflow then the destination register is not modified and an integer overflow exception " +
    //                          "occurs. If it does not overflow, the 64-bit result is placed into RD.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsub = 0b000000_00000_00000_00000_00000_101110;

    //    [Operation("dsubu", "Doubleword Subtract Unsigned", "RD, RS, RT", "RD = RS - RT", "{0} = {1} - {2}", 0b000000_00000_00000_00000_00000_101111,
    //            Description = "The 64-bit value of RT is subtracted from the 64-bit value in RS to produce a 64-bit result.",
    //            Misnomer = "The term \"unsigned\" is a misnomer; this operation is 64-bit modulo arithmetic that does not trap on overflow.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Dsubu = 0b000000_00000_00000_00000_00000_101111;

    //    [Operation("j", "Jump", "target", "Jump to target", "Jump to {0}", 0b000010_00000000000000000000000000,
    //            Description = "Jump to the effective target address comprising of the low 26 bits of the target address, shifted left 2 bits (for a total of 28 bits), " +
    //                          "within the current 256MB aligned region. Execute the instruction following the jump, in the branch delay slot, before jumping." +
    //                          "The remaining upper bits are the corresponding bits of the address of the instruction in the delay slot (not the branch itself).")]
    //    public const uint J = 0b000010_00000000000000000000000000;

    //    [Operation("jal", "Jump and Link", "target", "Jump to procedure", "Execute procedure at {0}", 0b000011_00000000000000000000000000,
    //            Description = "Jump to the effective target address comprising of the low 26 bits of the target address, shifted left 2 bits (for a total of 28 bits), " +
    //                          "within the current 256MB aligned region. Execute the instruction following the jump, in the branch delay slot, before jumping." +
    //                          "The remaining upper bits are the corresponding bits of the address of the instruction in the delay slot (not the branch itself)." +
    //                          "The return address link is placed in register $31. The return address link is the address of the second instruction following the branch, " +
    //                          "where execution would continue after a procedure call.")]
    //    public const uint Jal = 0b000011_00000000000000000000000000;

    //    [Operation("jalr", "Jump and Link Register", "RD, RS", "Jump to procedure", "Execute procedure at {0}, store return addr in {1}", 0b000000_00000_00000_00000_00000_001001,
    //            Description = "Jump to the effective target address in RS. Execute the instruction following the jump, in the branch delay slot, before jumping." +
    //                          "The remaining upper bits are the corresponding bits of the address of the instruction in the delay slot (not the branch itself)." +
    //                          "The return address link is placed in RD. The return address link is the address of the second instruction following the branch, " +
    //                          "where execution would continue after a procedure call.",
    //            Restriction = "RD and RS must not be equal because such an instruction does not have the same effect when re-executed. The result of executing such " +
    //                          "an instruction is undefined. The effective target address in RS must be naturally aligned. If either of the two least-significant bits " +
    //                          "are not zero an Address Exception occurs, not for the jump instruction, but when the branch target is subsequently fetched as an instruction.")]
    //    public const uint Jalr = 0b000000_00000_00000_00000_00000_001001;

    //    [Operation("jr", "Jump Register", "RS", "Jump to target", "Jump to {0}", 0b000000_00000_000000000000000_001000,
    //            Description = "Jump to the effective target address in RS. Execute the instruction following the jump, in the branch delay slot, before jumping.",
    //            Restriction = "The effective target address in RS must be naturally aligned. If either of the two least-significant bits are not zero an Address " +
    //                          "Exception occurs, not for the jump instruction, but when the branch target is subsequently fetched as an instruction.")]
    //    public const uint Jr = 0b000000_00000_000000000000000_001000;

    //    [Operation("lb", "Load Byte", "RT, Offset(Base)", "RT = Memory[Base + Offset]", "{0} = Memory[{1}]", 0b100000_00000_00000_0000000000000000,
    //            Description = "Load the byte at the effective memory address, sign extend it, and place it into RT. The 16-bit signed offset is added to the base " +
    //                          "address to form the effective address.")]
    //    public const uint Lb = 0b100000_00000_00000_0000000000000000;

    //    [Operation("lbu", "Load Byte Unsigned", "RT, Offset(Base)", "RT = Memory[Base + Offset]", "{0} = Memory[{1}]", 0b100100_00000_00000_0000000000000000,
    //            Description = "Load the byte at the effective memory address, zero extend it, and place it into RT. The 16-bit signed offset is added to the base " +
    //                          "address to form the effective address.")]
    //    public const uint Lbu = 0b100100_00000_00000_0000000000000000;

    //    [Operation("ld", "Load Doubleword", "RT, Offset(Base)", "RT = Memory[Base + Offset]", "{0} = Memory[{1}]", 0b110111_00000_00000_0000000000000000,
    //            Description = "Load the 64-bit value at the effective memory address and place it into RT. The 16-bit signed offset is added to the base " +
    //                          "address to form the effective address.",
    //            Restriction = "The effective address must be naturally aligned. If any of the 3 least-significant bits of the address are non-zero, an Address Exception " +
    //                          "occurs. In MIPS IV, the low-order 3 bits of the offset field must be zero. If they are not, the result of the instruction is undefined.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Ld = 0b110111_00000_00000_0000000000000000;

    //    [Operation("ldc1", "Load Doubleword to Coprocessor", "RT, Offset(Base)", "RT = Memory[Base + Offset]", "{0} = Memory[{1}]", 0b110101_00000_00000_0000000000000000,
    //            Description = "Load the 64-bit value at the effective memory address, place it into RT, and make it available to coprocessor unit 1. The 16-bit signed " +
    //                          "offset is added to the base address to form the effective address.",
    //            Restriction = "This instruction is not available for coprocessor 0 or the system control coprocessor, and the opcode may be used for other instructions. " +
    //                          "The effective address must be naturally aligned. If any of the 3 least-significant bits of the address are non-zero, an Address Exception " +
    //                          "occurs. In MIPS IV, the low-order 3 bits of the offset field must be zero. If they are not, the result of the instruction is undefined.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Ldc1 = 0b110101_00000_00000_0000000000000000;

    //    [Operation("ldc2", "Load Doubleword to Coprocessor", "RT, Offset(Base)", "RT = Memory[Base + Offset]", "{0} = Memory[{1}]", 0b110110_00000_00000_0000000000000000,
    //            Description = "Load the 64-bit value at the effective memory address, place it into RT, and make it available to coprocessor unit 2. The 16-bit signed " +
    //                          "offset is added to the base address to form the effective address.",
    //            Restriction = "This instruction is not available for coprocessor 0 or the system control coprocessor, and the opcode may be used for other instructions. " +
    //                          "The effective address must be naturally aligned. If any of the 3 least-significant bits of the address are non-zero, an Address Exception " +
    //                          "occurs. In MIPS IV, the low-order 3 bits of the offset field must be zero. If they are not, the result of the instruction is undefined.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Ldc2 = 0b110110_00000_00000_0000000000000000;

    //    [Operation("ldl", "Load Doubleword Left", "RT, Offset(Base)", "RT = RT MERGE Memory[Base + Offset]", "{0} = {0} MERGE Memory[{1}]", 0b011010_00000_00000_0000000000000000,
    //            Description = "The 16-bit signed offset is added to base to form an effective address. The effective address is the address of the most-significant of eight " +
    //                          "consecutive bytes forming a doubleword in memory starting at an arbitrary byte boundary. The most-significant one to eight bytes are in the aligned " +
    //                          "doubleword containing the effective address. This part of the doubleword is loaded into the most-significant (left) part of RT leaving the " +
    //                          "remainder of RT unchanged.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Ldl = 0b011010_00000_00000_0000000000000000;

    //    [Operation("ldr", "Load Doubleword Right", "RT, Offset(Base)", "RT = RT MERGE Memory[Base + Offset]", "{0} = {0} MERGE Memory[{1}]", 0b011011_00000_00000_0000000000000000,
    //            Description = "The 16-bit signed offset is added to base to form an effective address. The effective address is the address of the least-significant of eight " +
    //                          "consecutive bytes forming a doubleword in memory starting at an arbitrary byte boundary. The least-significant one to eight bytes are in the aligned " +
    //                          "doubleword containing the effective address. This part of the doubleword is loaded into the least-significant (right) part of RT leaving the " +
    //                          "remainder of RT unchanged.",
    //            IntroducedIn = EMipsVersion.Three)]
    //    public const uint Ldr = 0b011011_00000_00000_0000000000000000;

    //    [Operation("lh", "Load Halfword", "RT, Offset(Base)", "RT = Memory[Base + Offset]", "{0} = Memory[{1}]", 0b100001_00000_00000_0000000000000000,
    //            Description = "The contents of the 16-bit value at the memory location specified by the aligned effective address are fetched, sign-extended, and placed into RT. " +
    //                          "The 16-bit signed offset is added to the contents of base to form the effective address.",
    //            Restriction = "The effective address must be naturally aligned. If the least-significant bit of the address is non-zero, an Address Error exception occurs. " +
    //                          "In MIPS IV, the low-order bit of the offset field must be zero. If it is not, the result of the instruction is undefined.")]
    //    public const uint Lh = 0b100001_00000_00000_0000000000000000;

    //    [Operation("lhu", "Load Halfword Unsigned", "RT, Offset(Base)", "RT = Memory[Base + Offset]", "{0} = Memory[{1}]", 0b100101_00000_00000_0000000000000000,
    //            Description = "The contents of the 16-bit value at the memory location specified by the aligned effective address are fetched, zero-extended, and placed into RT. " +
    //                          "The 16-bit signed offset is added to the contents of base to form the effective address.",
    //            Restriction = "The effective address must be naturally aligned. If the least-significant bit of the address is non-zero, an Address Error exception occurs. " +
    //                          "In MIPS IV, the low-order bit of the offset field must be zero. If it is not, the result of the instruction is undefined.")]
    //    public const uint Lhu = 0b100101_00000_00000_0000000000000000;

    //    [Operation("ll", "Load Linked Word", "RT, Offset(Base)", "RT = Memory[Base + Offset]", "{0} = Memory[{1}]", 0b110000_00000_00000_0000000000000000,
    //            Description = "The contents of the 32-bit value at the memory location specified by the aligned effective address are fetched, sign-extended to the register length " +
    //                          "if necessary, and written to RT. This operation begins a Read-Modify-Write (RMW) sequence on the current processor. The 16-bit signed offset is added to the " +
    //                          "contents of base to form the effective address. There is only one active RMW sequence per processor. When an LL is executed it starts the active RMW " +
    //                          "sequence replacing any other sequence that was active. The RMW sequence is completed by a subsequent SC instruction that either completes the RMW sequence " +
    //                          "atomically and succeeds, or does nothing and fails. An execution of LL does not have to be followed by an execution of SC; a program is free to abandon " +
    //                          "the RMW sequence without attempting a write.",
    //            Restriction = "The addressed location must be cached; if it is not, the result is undefined. The effective address must be naturally aligned. If either of the two " +
    //                          "least-significant bits of the effective address are non-zero an Address Error exception occurs. In MIPS IV, the low-order 2 bits of the offset field must " +
    //                          "be zero. If they are not, the result of the instruction is undefined. An LL on one processor must not take an action that, by itself, would cause an SC for the " +
    //                          "same block on another processor to fail.",
    //            IntroducedIn = EMipsVersion.Two)]
    //    public const uint Ll = 0b110000_00000_00000_0000000000000000;

    //    // LLD page 105


    //}
}
