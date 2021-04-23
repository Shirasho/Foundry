using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class MovfOperationSyntax : OperationSyntax
    {
        private readonly uint CC;

        internal MovfOperationSyntax(ISyntax? owner, in Instruction instruction, uint cc)
            : base("movf", instruction, owner)
        {
            CC = cc;
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            yield return $"movf\t{Instruction.RD}, {Instruction.RS}, {CC}";
        }
    }
}
