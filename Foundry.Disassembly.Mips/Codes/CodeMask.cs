using System;
using System.Runtime.CompilerServices;

namespace Foundry.Disassembly.Mips.Codes
{
    /// <summary>
    /// Instruction code masks.
    /// </summary>
    public static class CodeMask
    {
        /// <summary>
        /// The op code bytes that all types share.
        /// </summary>
        public const uint OpCode = 0b11111100000000000000000000000000;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint GetOpCode(uint instruction)
        {
            return (instruction & OpCode) >> 26;
        }

        /// <summary>
        /// R-Type masks.
        /// </summary>
        public static class RType
        {
            public const uint RS     = 0b00000011111000000000000000000000;
            public const uint RT     = 0b00000000000111110000000000000000;
            public const uint RD     = 0b00000000000000001111100000000000;
            public const uint Shamd  = 0b00000000000000000000011111000000;
            public const uint Funct  = 0b00000000000000000000000000111111;

            public const uint FT     = RT;
            public const uint FS     = RD;
            public const uint FD     = Shamd;

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
        }

        /// <summary>
        /// I-Type masks.
        /// </summary>
        public static class IType
        {
            public const uint RS     = 0b00000011111000000000000000000000;
            public const uint RT     = 0b00000000000111110000000000000000;
            public const uint IMM    = 0b00000000000000001111111111111111;

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
        }

        /// <summary>
        /// J-Type masks.
        /// </summary>
        public static class JType
        {
            public const uint Addr   = 0b00000011111111111111111111111111;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint GetAddr(uint instruction)
            {
                return instruction & Addr;
            }
        }
    }
}
