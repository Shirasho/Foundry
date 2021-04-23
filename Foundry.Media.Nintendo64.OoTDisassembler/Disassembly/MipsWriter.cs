using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Foundry.Media.Nintendo64.Disassembly.OoT.Syntax;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly
{
    internal class MipsWriter : IAsyncDisposable
    {
        private readonly Stream Destination;
        private readonly Encoding Encoding;

        public MipsWriter(FileInfo file, Encoding? encoding = null)
            : this(file.Open(FileMode.Append, FileAccess.Write, FileShare.Read), encoding)
        {

        }

        public MipsWriter(Stream dest, Encoding? encoding = null)
        {
            Destination = dest;
            Encoding = encoding ?? Encoding.UTF8;
        }

        public async ValueTask DisposeAsync()
        {
            await Destination.FlushAsync();
            await Destination.DisposeAsync();
        }

        public Task FlushAsync(CancellationToken cancellationToken)
            => Destination.FlushAsync(cancellationToken);

        public async Task WriteHeaderAsync(CancellationToken cancellationToken = default)
        {
            using var sw = Destination.GetStreamWriter(Encoding);

            await sw.WriteLineAsync(".set noat # allow use of $at", cancellationToken);
            await sw.WriteLineAsync(".set noreorder # disallow insertion of nop after branches", cancellationToken);
            await sw.WriteLineAsync(".set gp=64 # allow use of 64bit registers", cancellationToken);
            await sw.WriteLineAsync(".macro glabel label", cancellationToken);
            await sw.WriteLineAsync("    .global \\label", cancellationToken);
            await sw.WriteLineAsync("    \\label:", cancellationToken);
            await sw.WriteLineAsync(".endm", cancellationToken);
            await sw.WriteLineAsync();
            await sw.FlushAsync();
        }

        public ValueTask WriteLabelAsync(string label, CancellationToken cancellationToken)
        {
            return WriteLineAsync($".L_{label}:", cancellationToken);
        }

        public ValueTask WriteLabelAsync(RomLabel label, CancellationToken cancellationToken)
            => WriteLabelAsync(label.Name, cancellationToken);

        public ValueTask WriteSwitchCaseAsync(uint addr, CancellationToken cancellationToken)
            => WriteLineAsync($"glabel .L_{addr:X}", cancellationToken);

        public async ValueTask WriteFunctionAsync(RomFunction function, CancellationToken cancellationToken)
        {
            await Destination.WriteAsync(Encoding.GetBytes(Environment.NewLine), cancellationToken);
            await WriteLineAsync($"glabel {function.GetName()}", cancellationToken);
        }

        public ValueTask WriteFunctionCallAsync(int instructionOffset, Instruction addr, RomFunction function, CancellationToken cancellationToken)
            => WriteLineAsync($"/* {instructionOffset} {addr} */ .word\t{function.GetName()}", cancellationToken);

        public ValueTask WriteGoToCallAsync(int instructionOffset, Instruction addr, uint instruction, CancellationToken cancellationToken)
            => WriteLineAsync($"/* {instructionOffset} {addr} */ .word\tL_{instruction:X}", cancellationToken);

        public ValueTask WriteVariableCallAsync(int instructionOffset, Instruction addr, string variableName, CancellationToken cancellationToken)
            => WriteVariableCallWordAsync(instructionOffset, addr, variableName, cancellationToken);

        public ValueTask WriteVariableCallByteAsync(int instructionOffset, Instruction addr, string variableName, CancellationToken cancellationToken)
            => WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .byte\t{variableName}", cancellationToken);

        public ValueTask WriteVariableCallShortAsync(int instructionOffset, Instruction addr, string variableName, CancellationToken cancellationToken)
            => WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .short\t{variableName}", cancellationToken);

        public ValueTask WriteVariableCallWordAsync(int instructionOffset, Instruction addr, string variableName, CancellationToken cancellationToken)
            => WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .word\t{variableName}", cancellationToken);

        public ValueTask WriteVariableCallAsync(int instructionOffset, Instruction addr, string variableName, uint offset, CancellationToken cancellationToken)
            => WriteLineAsync($"/* {instructionOffset} 0x{addr:X} */ .word\t({variableName} + 0x{offset:X})", cancellationToken);

        public async ValueTask WriteSyntaxAsync(ISyntax syntax, Instruction instruction, Instruction addr, int instructionOffset, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            await syntax.WriteToAsync(sb, cancellationToken);
            await WriteLineAsync($"/* 0x{instructionOffset:X} {addr} {instruction} */ {sb}", cancellationToken);
        }

        public ValueTask WriteVariableAsync(string name, CancellationToken cancellationToken)
            => WriteLineAsync($"glabel {name}", cancellationToken);

        private ValueTask WriteLineAsync(string value, CancellationToken cancellationToken)
            => Destination.WriteAsync(Encoding.GetBytes(value).AsMemory(), cancellationToken);
    }
}
