using System;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// Represents a ROM file.
    /// </summary>
    public interface IRomData : IDisposable
    {
        /// <summary>
        /// Whether this ROM data has been disposed.
        /// </summary>
        bool Disposed { get; }

        /// <summary>
        /// Whether the ROM data can be unloaded from memory and
        /// reliably loaded at a later time.
        /// </summary>
        bool CanReload { get; }

        /// <summary>
        /// The ROM metadata.
        /// </summary>
        RomMetadata Metadata { get; }

        /// <summary>
        /// Information about the size of the ROM data.
        /// </summary>
        RomSize Size { get; }

        /// <summary>
        /// The length of the ROM data.
        /// </summary>
        long Length => Size.Size.Bytes;

        /// <summary>
        /// Returns the ROM data in its entirety.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will load the data from the source if possible.
        /// </para>
        /// <para>
        /// This method will throw an <see cref="InvalidOperationException"/> if the data is unloaded
        /// and <see cref="CanReload"/> is <see langword="false"/>.
        /// </para>
        /// </remarks>
        ReadOnlyMemory<byte> GetData();

        /// <summary>
        /// Returns the ROM data in its entirety.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will load the data from the source if possible.
        /// </para>
        /// <para>
        /// This method will throw an <see cref="InvalidOperationException"/> if the data is unloaded
        /// and <see cref="CanReload"/> is <see langword="false"/>.
        /// </para>
        /// </remarks>
        ValueTask<ReadOnlyMemory<byte>> GetDataAsync();

        /// <summary>
        /// Returns the IPL3, also known as the ROM boot code.
        /// </summary>
        /// <remarks>
        /// The IPL3 is the first 4096 bytes (0x1000) of the ROM data,
        /// minus the 64 bytes (0x40) required for header info.
        /// </remarks>
        ReadOnlyMemory<byte> GetIPL3();

        /// <summary>
        /// Returns the IPL3, also known as the ROM boot code.
        /// </summary>
        /// <remarks>
        /// The IPL3 is the first 4096 bytes (0x1000) of the ROM data,
        /// minus the 64 bytes (0x40) required for header info.
        /// </remarks>
        ValueTask<ReadOnlyMemory<byte>> GetIPL3Async();

        /// <summary>
        /// Unloads the ROM data from memory.
        /// </summary>
        /// <remarks>
        /// To check whether it is safe to call this method and reliably
        /// reload the contents, check <see cref="CanReload"/> before calling
        /// this method.
        /// </remarks>
        void Unload();

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        /// <remarks>
        /// This method will throw an <see cref="InvalidOperationException"/> if <see cref="CanReload"/>
        /// returns <see langword="false"/>.
        /// </remarks>
        void Load();

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        /// <remarks>
        /// This method will throw an <see cref="InvalidOperationException"/> if <see cref="CanReload"/>
        /// returns <see langword="false"/>.
        /// </remarks>
        ValueTask LoadAsync();

        /// <summary>
        /// Returns a filename for the ROM that is constructed using ROM
        /// metadata.
        /// </summary>
        string GetFilename();
    }
}
