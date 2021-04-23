using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class NeguOperationSyntax : OperationSyntax
    {
        internal NeguOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("negu", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Puts the negated value in register {Instruction.RT} into register {Instruction.RD}.";
            }
            yield return $"negu\t{Instruction.RD}, {Instruction.RT}";
        }
    }
}
