namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// The ROM file destination code provided by Nintendo for the cartridge.
    /// </summary>
    public enum ERomDestinationCode : byte
    {
        Undocumented = 0x41,
        Brazilian = 0x42,
        Chinese = 0x43,
        German = 0x44,
        NorthAmerican = 0x45,
        French = 0x46,
        Gateway64NTSC = 0x47,
        Dutch = 0x48,
        Italian = 0x49,
        Japanese = 0x4A,
        Korean = 0x4B,
        Gateway64PAL = 0x4C,
        Canadian = 0x4E,
        European = 0x50,
        Spanish = 0x53,
        Australian = 0x55,
        Scandinavian = 0x57,
        OthersX = 0x58,
        OtherY = 0x59,
        OthersZ = 0x5A
    }
}
