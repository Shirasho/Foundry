using System;
using System.Threading;
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
        RomHeader Header { get; }

        /// <summary>
        /// Information about the size of the ROM data.
        /// </summary>
        RomSize Size { get; }

        /// <summary>
        /// The length of the ROM data.
        /// </summary>
        ulong Length => Size.Size.Bytes;

        /// <summary>
        /// Returns the header data.
        /// </summary>
        /// <remarks>
        /// The header is the first 64 bytes (0x40) of the ROM data.
        /// </remarks>
        ReadOnlyMemory<byte> GetHeaderData();

        /// <summary>
        /// Returns the header data.
        /// </summary>
        /// <remarks>
        /// The header is the first 64 bytes (0x40) of the ROM data.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask<ReadOnlyMemory<byte>> GetHeaderDataAsync(CancellationToken cancellationToken = default);

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
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask<ReadOnlyMemory<byte>> GetDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the IPL3, also known as the ROM boot code.
        /// </summary>
        /// <remarks>
        /// The IPL3 is the first 4096 bytes (0x1000) of the ROM data,
        /// minus the 64 bytes (0x40) required for header info.
        /// </remarks>
        ReadOnlyMemory<byte> GetIPL3Data();

        /// <summary>
        /// Returns the IPL3, also known as the ROM boot code.
        /// </summary>
        /// <remarks>
        /// The IPL3 is the first 4096 bytes (0x1000) of the ROM data,
        /// minus the 64 bytes (0x40) required for header info.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask<ReadOnlyMemory<byte>> GetIPL3DataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns the code segment.
        /// </summary>
        /// <remarks>
        /// The start address is assumed to be 0x1000. This data may be compressed.
        /// </remarks>
        /// <param name="endAddress">The code end address.</param>
        ReadOnlyMemory<byte> GetCodeData(uint endAddress)
            => GetCodeData(new Range((int)RomData.CodeStartAddress, (int)endAddress));

        /// <summary>
        /// Returns the code segment.
        /// </summary>
        /// <remarks>
        /// This data may be compressed.
        /// </remarks>
        /// <param name="startAddress">The code start address.</param>
        /// <param name="endAddress">The code end address.</param>
        ReadOnlyMemory<byte> GetCodeData(uint startAddress, uint endAddress)
            => GetCodeData(new Range((int)startAddress, (int)endAddress));

        /// <summary>
        /// Returns the code segment.
        /// </summary>
        /// <remarks>
        /// This data may be compressed.
        /// </remarks>
        ReadOnlyMemory<byte> GetCodeData(Range region);

        /// <summary>
        /// Returns the code segment.
        /// </summary>
        /// <remarks>
        /// The start address is assumed to be 0x1000. This data may be compressed.
        /// </remarks>
        /// <param name="endAddress">The code end address.</param>
        ValueTask<ReadOnlyMemory<byte>> GetCodeDataAsync(uint endAddress, CancellationToken cancellationToken = default)
            => GetCodeDataAsync(new Range((int)RomData.CodeStartAddress, (int)endAddress), cancellationToken);

        /// <summary>
        /// Returns the code segment.
        /// </summary>
        /// <remarks>
        /// This data may be compressed.
        /// </remarks>
        /// <param name="startAddress">The code start address.</param>
        /// <param name="endAddress">The code end address.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask<ReadOnlyMemory<byte>> GetCodeDataAsync(uint startAddress, uint endAddress, CancellationToken cancellationToken = default)
            => GetCodeDataAsync(new Range((int)startAddress, (int)endAddress), cancellationToken);

        /// <summary>
        /// Returns the code segment.
        /// </summary>
        /// <remarks>
        /// This data may be compressed.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask<ReadOnlyMemory<byte>> GetCodeDataAsync(Range region, CancellationToken cancellationToken = default);

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
        /// <param name="cancellationToken">The cancellation token.</param>
        ValueTask LoadAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns a filename for the ROM that is constructed using ROM
        /// metadata.
        /// </summary>
        string GetFilename();
    }
}
