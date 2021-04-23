using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SrlvOperationSyntax : OperationSyntax
    {
        internal SrlvOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("srlv", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Shift right logical variable - shift the contents of register {Instruction.RT} right by the";
                yield return $"# distance indicated in value stored in register {Instruction.RS} and store the result in";
                yield return $"# register {Instruction.RD}.";
            }
            yield return $"srlv\t{Instruction.RD}, {Instruction.RT}, {Instruction.RS}";
        }
    }
}
