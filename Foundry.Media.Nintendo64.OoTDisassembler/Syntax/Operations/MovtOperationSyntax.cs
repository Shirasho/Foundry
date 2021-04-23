using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MovtOperationSyntax : OperationSyntax
    {
        private readonly uint CC;

        internal MovtOperationSyntax(ISyntax? owner, in Instruction instruction, uint cc)
            : base("movt", instruction, owner)
        {
            CC = cc;
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"movt\t{Instruction.RD}, {Instruction.RS}, {CC}";
        }
    }
}
