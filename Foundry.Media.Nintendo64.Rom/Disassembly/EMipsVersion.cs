using System.ComponentModel;

namespace Foundry.Media.Nintendo64.Rom.Disassembly
{
    /// <summary>
    /// A MIPS version.
    /// </summary>
    public enum EMipsVersion : byte
    {
        [Description("I")]
        One,
        [Description("II")]
        Two,
        [Description("III")]
        Three,
        [Description("IV")]
        Four
    }
}
