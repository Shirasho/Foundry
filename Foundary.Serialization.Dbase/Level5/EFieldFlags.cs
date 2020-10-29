using System;

namespace Foundry.Serialization.Dbase.Level5
{
    /// <summary>
    /// Level 5 field flags.
    /// </summary>
    [Flags]
    public enum EFieldFlags : byte
    {
        None = 0x00,
        System = 0x01,
        AllowNullValues = 0x02,
        Binary = 0x04,
        AutoIncrementing = 0x0C
    }
}
