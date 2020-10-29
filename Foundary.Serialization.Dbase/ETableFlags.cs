using System;

namespace Foundry.Serialization.Dbase
{
    /// <summary>
    /// Table flags.
    /// </summary>
    [Flags]
    public enum ETableFlags : byte
    {
        None = 0x00,
        HasStructuralCDX = 0x01,
        HasMemoField = 0x02,
        IsDBC = 0x04
    }
}
