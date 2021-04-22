using System;
using System.Runtime.CompilerServices;

namespace Foundry.Media.Nintendo64.OotDecompiler.Disassembly
{
    internal partial class Disassembler
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToInstruction(uint instruction)
        {
            return (instruction & 0b11111100000000000000000000000000) >> 26;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToFunction(uint instruction)
        {
            return instruction & 0b00000000000000000000000000111111;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToRS(uint instruction)
        {
            return (instruction & 0b00000011111000000000000000000000) >> 21;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToRT(uint instruction)
        {
            return (instruction & 0b00000000000111110000000000000000) >> 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToRD(uint instruction)
        {
            return (instruction & 0b00000000000000001111100000000000) >> 11;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToShift(uint instruction)
        {
            return (instruction & 0b00000000000000000000011111000000) >> 6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToFT(uint instruction)
        {
            return (instruction & 0b00000000000111110000000000000000) >> 16;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToFS(uint instruction)
        {
            return (instruction & 0b00000000000000001111100000000000) >> 11;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToFD(uint instruction)
        {
            return (instruction & 0b00000000000000000000011111000000) >> 6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint ToIMM(uint instruction)
        {
            return instruction & 0b00000000000000001111111111111111;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static long ToSignedIMM(uint instruction)
        {
            uint imm = ToIMM(instruction);
            if ((imm & (1 << 15)) != 0)
            {
                return (long)Math.Pow(-2, 15) + (imm & 0b00000000000000000111111111111111);
            }

            return imm;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsLoadInstruction(uint addr)
        {
            return ToInstruction(addr) > 31;
        }
    }
}
