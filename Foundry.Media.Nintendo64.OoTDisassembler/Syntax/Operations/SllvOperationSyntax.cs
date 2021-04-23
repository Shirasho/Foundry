using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class SllvOperationSyntax : OperationSyntax
    {
        internal SllvOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("sllv", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Shift left logical variable - shift the contents of register {Instruction.RT} left by the";
                yield return $"# distance indicated in value stored in register {Instruction.RS} and store the result in";
                yield return $"# register {Instruction.RD}.";
            }
            yield return $"sllv\t{Instruction.RD}, {Instruction.RT}, {Instruction.RS}";
        }
    }
}
