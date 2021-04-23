using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class NorOperationSyntax : OperationSyntax
    {
        internal NorOperationSyntax(ISyntax? owner, in Instruction instruction)
            : base("nor", instruction, owner)
        {
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# Bitwise NORs the values in registers {Instruction.RS} and {Instruction.RT}";
                yield return $"# and stores the result in register {Instruction.RD}.";
            }
            yield return $"nor\t{Instruction.RD}, {Instruction.RS}, {Instruction.RT}";
        }
    }
}
