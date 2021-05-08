using System;
using System.Runtime.CompilerServices;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// Represents an unprocessed MIPS instruction.
    /// </summary>
    public readonly struct Instruction
    {
        /// <summary>
        /// The raw MIPS instruction bytes.
        /// </summary>
        public readonly uint Value { get; }

        /// <summary>
        /// The instruction number.
        /// </summary>
        public readonly uint Number { get; }

        /// <summary>
        /// The virtual address of the instruction.
        /// </summary>
        public readonly uint VirtualAddress { get; }

        /// <summary>
        /// The operation code.
        /// </summary>
        public readonly uint OpCode => Mask.GetOpCode(Value);

        /// <summary>
        /// The RS.
        /// </summary>
        public readonly uint RS => Mask.GetRS(Value);

        /// <summary>
        /// The RT.
        /// </summary>
        public readonly uint RT => Mask.GetRT(Value);

        /// <summary>
        /// The RD.
        /// </summary>
        public readonly uint RD => Mask.GetRD(Value);

        /// <summary>
        /// The shamd.
        /// </summary>
        public readonly uint Shamd => Mask.GetShamd(Value);

        /// <summary>
        /// The shift.
        /// </summary>
        public readonly uint Shift => Mask.GetShamd(Value);

        /// <summary>
        /// The function code.
        /// </summary>
        public readonly uint Funct => Mask.GetFunct(Value);

        /// <summary>
        /// The FT.
        /// </summary>
        public readonly uint FT => Mask.GetFT(Value);

        /// <summary>
        /// The FS.
        /// </summary>
        public readonly uint FS => Mask.GetFS(Value);

        /// <summary>
        /// The FD.
        /// </summary>
        public readonly uint FD => Mask.GetFD(Value);

        /// <summary>
        /// The immediate.
        /// </summary>
        public readonly uint IMM => Mask.GetIMM(Value);

        /// <summary>
        /// The immediate as a signed value.
        /// </summary>
        public readonly int IMMSigned => Mask.GetIMMSigned(Value);

        /// <summary>
        /// The address.
        /// </summary>
        public readonly uint Address => Mask.GetAddr(Value);

        public Instruction(uint value, uint virtualAddress, uint number)
        {
            Value = value;
            VirtualAddress = virtualAddress;
            Number = number;
        }

        /// <summary>
        /// Returns the absolute offset address for a jump instruction.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly uint GetAbsoluteJumpAddress()
        {
            return (IMM + 1) * 4 + VirtualAddress;
        }

        public static class Mask
        {
            /// <summary>
            /// The op code bytes that all types share.
            /// </summary>
            public const uint OpCode = 0b11111100000000000000000000000000;
            public const uint RS = 0b00000011111000000000000000000000;
            public const uint RT = 0b00000000000111110000000000000000;
            public const uint RD = 0b00000000000000001111100000000000;
            public const uint Shamd = 0b00000000000000000000011111000000;
            public const uint Funct = 0b00000000000000000000000000111111;

            public const uint FT = 0b00000000000111110000000000000000;
            public const uint FS = 0b00000000000000001111100000000000;
            public const uint FD = 0b00000000000000000000011111000000;

            public const uint IMM = 0b00000000000000001111111111111111;
            public const uint Addr = 0b00000011111111111111111111111111;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetOpCode(uint instruction)
            {
                return (instruction & OpCode) >> 26;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetRS(uint instruction)
            {
                return (instruction & RS) >> 21;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetRT(uint instruction)
            {
                return (instruction & RT) >> 16;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetRD(uint instruction)
            {
                return (instruction & RD) >> 11;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetFT(uint instruction)
            {
                return (instruction & FT) >> 16;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetFS(uint instruction)
            {
                return (instruction & FS) >> 11;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetFD(uint instruction)
            {
                return (instruction & FD) >> 6;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetShamd(uint instruction)
            {
                return (instruction & Shamd) >> 6;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetFunct(uint instruction)
            {
                return instruction & Funct;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetIMM(uint instruction)
            {
                return instruction & IMM;
            }

            public static int GetIMMSigned(uint instruction)
            {
                uint imm = GetIMM(instruction);
                uint mostSignificantBit = imm >> 15;
                if (mostSignificantBit == 0)
                {
                    return (int)imm;
                }
                else
                {
                    int signed = (int)Math.Pow(-2, 16);
                    return signed + (int)((imm << 1) >> 1);
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetAddr(uint instruction)
            {
                return instruction & Addr;
            }
        }
    }
}
