using System;

namespace Foundry.Media.Nintendo64.OotDecompiler.Disassembly
{
    internal partial class Disassembler
    {
        private record DisassemblerDataSegment(string Name, ReadOnlyMemory<byte> Data, uint VirtualAddress, string? SaveFileName);
    }
}
