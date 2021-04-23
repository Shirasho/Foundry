using System.Collections.Generic;
using Foundry.Media.Nintendo64.Disassembly.OoT.Disassembly;

namespace Foundry.Media.Nintendo64.Disassembly.OoT.Syntax
{
    public sealed class BranchOperationSyntax : OperationSyntax
    {
        private readonly BranchInstruction BranchInstruction;
        private readonly RomLabel Label;

        internal BranchOperationSyntax(ISyntax? owner, in Instruction instruction, BranchInstruction branchInstruction, in RomLabel label)
            : base(branchInstruction.Name, instruction, owner)
        {
            BranchInstruction = branchInstruction;
            Label = label;
        }

        protected override IEnumerable<string> GetMIPSLines(bool includeComments)
        {
            if (includeComments)
            {
                yield return $"# ";
            }
            //TODO: BEQ and BNE are RS, RT, Label.
            yield return $"{BranchInstruction.Name}\t{Instruction.RS}, {Label.Name}";
        }
    }
}
