using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SrlOperationSyntax : OperationSyntax
    {
        internal SrlOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("srl", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Shift right logical - shift the contents of register {Instruction.RT} right by the distance";
                yield return $"# indicated in {Instruction.Shift} and store the result in register {Instruction.RD}.";
            }
            yield return $"srl\t{Instruction.RD}, {Instruction.RT}, {Instruction.Shift}";
        }
    }
}
