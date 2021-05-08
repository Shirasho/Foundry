using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// The base class for <see cref="IRomData"/> implementations.
    /// </summary>
    public abstract class RomData : IRomData
    {
        /// <summary>
        /// The address at which the ROM code data starts.
        /// </summary>
        public const uint CodeStartAddress = 0x1000;

        /// <summary>
        /// Whether this ROM data has been disposed.
        /// </summary>
        public bool Disposed { get; private set; }

        /// <summary>
        /// Whether the ROM file contents can be unloaded from memory and
        /// reliably loaded at a later time.
        /// </summary>
        public abstract bool CanReload { get; }

        /// <summary>
        /// The ROM header.
        /// </summary>
        public RomHeader Header { get; private set; }

        /// <summary>
        /// Information about the size of the ROM data.
        /// </summary>
        public RomSize Size { get; private set; }

        /// <summary>
        /// The ROM data buffer.
        /// </summary>
        protected IMemoryOwner<byte>? Data { get; private set; }

        protected RomData(IMemoryOwner<byte> data)
        {
            SetData(data);
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                try
                {
                    OnDispose();
                }
                finally
                {
                    Disposed = true;
                }
            }
        }

        /// <summary>
        /// Disposes of resources used by this <see cref="RomData"/> instance.
        /// </summary>
        protected virtual void OnDispose()
        {
            UnloadRomData();
        }

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        /// <remarks>
        /// This method will throw an <see cref="InvalidOperationException" /> if <see cref="P:Foundry.Media.Nintendo64.Rom.IRomData.CanReload" />
        /// returns <see langword="false" />.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
        /// <exception cref="InvalidOperationException">The ROM data source does not support reloading.</exception>
        public void Load()
        {
            CheckDisposed();

            if (!CanReload)
            {
                throw new InvalidOperationException("This ROM data source does not support reloading.");
            }

            LoadRomData();
        }

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        protected abstract void LoadRomData();

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        /// <remarks>
        /// This method will throw an <see cref="InvalidOperationException" /> if <see cref="P:Foundry.Media.Nintendo64.Rom.IRomData.CanReload" />
        /// returns <see langword="false" />.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
        /// <exception cref="InvalidOperationException">The ROM data source does not support reloading.</exception>
        public ValueTask LoadAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (!CanReload)
            {
                throw new InvalidOperationException("This ROM data source does not support reloading.");
            }

            return LoadRomDataAsync();
        }

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected abstract ValueTask LoadRomDataAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Unloads the ROM data from memory.
        /// </summary>
        /// <remarks>
        /// To check whether it is safe to call this method and reliably
        /// reload the contents, check <see cref="P:Foundry.Media.Nintendo64.Rom.IRomData.CanReload" /> before calling
        /// this method.
        /// </remarks>
        public void Unload()
        {
            UnloadRomData();
        }

        /// <summary>
        /// Unloads the ROM data from memory.
        /// </summary>
        protected virtual void UnloadRomData()
        {
            Data?.Dispose();
            Data = null;
        }

        /// <summary>
        /// Returns the ROM data in its entirety.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will load the data from the source if possible.
        /// </para>
        /// <para>
        /// This method will throw an <see cref="InvalidOperationException" /> if the data is unloaded
        /// and <see cref="P:Foundry.Media.Nintendo64.Rom.IRomData.CanReload" /> is <see langword="false" />.
        /// </para>
        /// </remarks>
        /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
        /// <exception cref="InvalidOperationException">The ROM data source does not support reloading.</exception>
        public ReadOnlyMemory<byte> GetData()
        {
            CheckDisposed();

            if (Data is null)
            {
                Load();
            }

            return Data!.Memory;
        }

        /// <summary>
        /// Returns the ROM data in its entirety.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will load the data from the source if possible.
        /// </para>
        /// <para>
        /// This method will throw an <see cref="InvalidOperationException" /> if the data is unloaded
        /// and <see cref="P:Foundry.Media.Nintendo64.Rom.IRomData.CanReload" /> is <see langword="false" />.
        /// </para>
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
        /// <exception cref="InvalidOperationException">The ROM data source does not support reloading.</exception>
        public async ValueTask<ReadOnlyMemory<byte>> GetDataAsync(CancellationToken cancellationToken = default)
        {
            CheckDisposed();

            if (Data is null)
            {
                await LoadAsync(cancellationToken);
            }

            return Data!.Memory;
        }

        /// <summary>
        /// Returns the header data.
        /// </summary>
        /// <remarks>
        /// The header is the first 64 bytes (0x40) of the ROM data.
        /// </remarks>
        public ReadOnlyMemory<byte> GetHeaderData()
            => GetData().Slice(0, RomHeader.Length);

        /// <summary>
        /// Returns the header data.
        /// </summary>
        /// <remarks>
        /// The header is the first 64 bytes (0x40) of the ROM data.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async ValueTask<ReadOnlyMemory<byte>> GetHeaderDataAsync(CancellationToken cancellationToken = default)
        {
            var data = await GetDataAsync(cancellationToken);
            return data.Slice(0, RomHeader.Length);
        }

        /// <summary>
        /// Returns the IPL3, also known as the ROM boot code.
        /// </summary>
        /// <remarks>
        /// The IPL3 is the first 4096 bytes (0x1000) of the ROM data,
        /// minus the 64 bytes (0x40) required for header info.
        /// </remarks>
        public ReadOnlyMemory<byte> GetIPL3Data()
            => GetData().Slice(RomHeader.Length, (int)CodeStartAddress - RomHeader.Length);

        /// <summary>
        /// Returns the IPL3, also known as the ROM boot code.
        /// </summary>
        /// <remarks>
        /// The IPL3 is the first 4096 bytes (0x1000) of the ROM data,
        /// minus the 64 bytes (0x40) required for header info.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async ValueTask<ReadOnlyMemory<byte>> GetIPL3DataAsync(CancellationToken cancellationToken = default)
        {
            var data = await GetDataAsync(cancellationToken);
            return data.Slice(RomHeader.Length, (int)CodeStartAddress - RomHeader.Length);
        }

        /// <summary>
        /// Returns the code segment.
        /// </summary>
        /// <remarks>
        /// This data may be compressed.
        /// </remarks>
        public ReadOnlyMemory<byte> GetCodeData(Range region)
            => GetData().Slice(region.Start.Value, region.End.Value - region.Start.Value);

        /// <summary>
        /// Returns the code segment.
        /// </summary>
        /// <remarks>
        /// This data may be compressed.
        /// </remarks>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async ValueTask<ReadOnlyMemory<byte>> GetCodeDataAsync(Range region, CancellationToken cancellationToken = default)
        {
            var data = await GetDataAsync(cancellationToken);
            return data.Slice(region.Start.Value, region.End.Value - region.Start.Value);
        }

        /// <summary>
        /// Returns a filename for the ROM that is constructed using ROM
        /// header.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
        /// <exception cref="InvalidOperationException">The header is not loaded and <see cref="CanReload"/> is <see langword="false"/>.</exception>
        public string GetFilename()
        {
            CheckDisposed();

            if (Header is null)
            {
                Load();
            }

            if (Header is null)
            {
                throw new InvalidOperationException("The header is not loaded, and reloading did not provide valid header.");
            }

            string title = Header.Title;
            string extension = Header.GetFormatExtension();

            return title + extension;
        }

        /// <summary>
        /// Sets <see cref="Data"/> with the specified value and updates the values
        /// of <see cref="Header"/> and <see cref="Size"/>.
        /// </summary>
        /// <param name="data">The new data buffer.</param>
        [MemberNotNull(nameof(Header), nameof(Size))]
        protected void SetData(IMemoryOwner<byte>? data)
        {
            CheckDisposed();

            // This method could be called to recalculate existing data.
            // In this case we do not want to unload the memory.
            if (!ReferenceEquals(Data, data))
            {
                UnloadRomData();
                Data = data;

                if (Data is null)
                {
                    Header = RomHeader.None;
                    Size = RomSize.Empty;
                }
            }

            if (Data is not null)
            {
                Header = RomHeader.Create(Data.Memory.Span);
                Size = new RomSize((ulong)Data.Memory.Span.Length);
            }

            Header ??= RomHeader.None;
            Size ??= RomSize.Empty;
        }

        /// <summary>
        /// Loads a ROM at the specified file location.
        /// </summary>
        /// <param name="romPath">The file path to the ROM.</param>
        public static IRomData LoadRom(string romPath)
            => LoadRom(new FileInfo(romPath));

        /// <summary>
        /// Loads a ROM at the specified file location.
        /// </summary>
        /// <param name="romPath">The file path to the ROM.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<IRomData> LoadRomAsync(string romPath, CancellationToken cancellationToken = default)
            => LoadRomAsync(new FileInfo(romPath), cancellationToken);

        /// <summary>
        /// Loads a ROM at the specified file location.
        /// </summary>
        /// <param name="romFile">The ROM file.</param>
        public static IRomData LoadRom(FileInfo romFile)
        {
            Guard.IsNotNull(romFile, nameof(romFile));

            return RomFile.Create(romFile);
        }

        /// <summary>
        /// Loads a ROM at the specified file location.
        /// </summary>
        /// <param name="romFile">The ROM file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<IRomData> LoadRomAsync(FileInfo romFile, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romFile, nameof(romFile));

            return RomFile.CreateAsync(romFile, cancellationToken);
        }

        /// <summary>
        /// Loads a ROM using the data from the provided stream.
        /// </summary>
        /// <param name="romStream">The ROM data stream.</param>
        /// <param name="disposeStream">Whether to dispose of the stream when the ROM data is disposed.</param>
        public static IRomData LoadRom(Stream romStream, bool disposeStream = false)
        {
            Guard.IsNotNull(romStream, nameof(romStream));

            return RomStream.Create(romStream, disposeStream);
        }

        /// <summary>
        /// Loads a ROM using the data from the provided stream.
        /// </summary>
        /// <param name="romStream">The ROM data stream.</param>
        /// <param name="disposeStream">Whether to dispose of the stream when the ROM data is disposed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<IRomData> LoadRomAsync(Stream romStream, bool disposeStream = false, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romStream, nameof(romStream));

            return RomStream.CreateAsync(romStream, disposeStream, cancellationToken);
        }

        /// <summary>
        /// Loads a ROM using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        public static IRomData LoadRom(Memory<byte> romData)
        {
            return new RomMemory(romData);
        }

        /// <summary>
        /// Loads a ROM using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<IRomData> LoadRomAsync(Memory<byte> romData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<IRomData>(new RomMemory(romData));
        }

        /// <summary>
        /// Loads a ROM using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        public static IRomData LoadRom(ReadOnlyMemory<byte> romData)
        {
            return new RomMemory(romData);
        }

        /// <summary>
        /// Loads a ROM using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<IRomData> LoadRomAsync(ReadOnlyMemory<byte> romData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<IRomData>(new RomMemory(romData));
        }

        /// <summary>
        /// Loads a ROM using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        public static IRomData LoadRom(IMemoryOwner<byte> romData)
        {
            return new RomMemory(romData);
        }

        /// <summary>
        /// Loads a ROM using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<IRomData> LoadRomAsync(IMemoryOwner<byte> romData, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romData, nameof(romData));

            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<IRomData>(new RomMemory(romData));
        }

        /// <summary>
        /// Returns the ROM header from ROM file located at the provided path.
        /// </summary>
        /// <param name="romPath">The file path of the ROM file.</param>
        public static RomHeader GetHeader(string romPath)
            => GetHeader(new FileInfo(romPath));

        /// <summary>
        /// Returns the ROM header from ROM file located at the provided path.
        /// </summary>
        /// <param name="romPath">The file path of the ROM file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<RomHeader> GetHeaderAsync(string romPath, CancellationToken cancellationToken = default)
            => GetHeaderAsync(new FileInfo(romPath), cancellationToken);

        /// <summary>
        /// Returns the ROM header from the provided ROM file.
        /// </summary>
        /// <param name="romFile">The ROM file.</param>
        public static RomHeader GetHeader(FileInfo romFile)
        {
            Guard.IsNotNull(romFile, nameof(romFile));

            return RomFile.GetHeaderFromFile(romFile);
        }

        /// <summary>
        /// Returns the ROM header from the provided ROM file.
        /// </summary>
        /// <param name="romFile">The ROM file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<RomHeader> GetHeaderAsync(FileInfo romFile, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romFile, nameof(romFile));

            return RomFile.GetHeaderFromFileAsync(romFile, cancellationToken);
        }

        /// <summary>
        /// Returns the ROM header using the data from the provided stream.
        /// </summary>
        /// <param name="romStream">The ROM data stream.</param>
        public static RomHeader GetHeader(Stream romStream)
        {
            Guard.IsNotNull(romStream, nameof(romStream));

            return RomStream.GetHeaderFromStream(romStream);
        }

        /// <summary>
        /// Returns the ROM header using the data from the provided stream.
        /// </summary>
        /// <param name="romStream">The ROM data stream.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<RomHeader> GetHeaderAsync(Stream romStream, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romStream, nameof(romStream));

            return RomStream.GetHeaderFromStreamAsync(romStream, cancellationToken);
        }

        /// <summary>
        /// Returns the ROM header using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        public static RomHeader GetHeader(ReadOnlyMemory<byte> romData)
            => GetHeader(romData.Span);

        /// <summary>
        /// Returns the ROM header using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<RomHeader> GetHeaderAsync(ReadOnlyMemory<byte> romData, CancellationToken cancellationToken = default)
            => GetHeaderAsync(romData.Span, cancellationToken);

        /// <summary>
        /// Returns the ROM header using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        public static RomHeader GetHeader(ReadOnlySpan<byte> romData)
        {
            return RomHeader.Create(romData);
        }

        /// <summary>
        /// Returns the ROM header using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<RomHeader> GetHeaderAsync(ReadOnlySpan<byte> romData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<RomHeader>(RomHeader.Create(romData));
        }

        /// <summary>
        /// Returns the ROM header using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        public static RomHeader GetHeader(IMemoryOwner<byte> romData)
        {
            Guard.IsNotNull(romData, nameof(romData));

            return GetHeader(romData.Memory.Span);
        }

        /// <summary>
        /// Returns the ROM header using the data from the provided memory region.
        /// </summary>
        /// <param name="romData">The ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<RomHeader> GetHeaderAsync(IMemoryOwner<byte> romData, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romData, nameof(romData));

            return GetHeaderAsync(romData.Memory.Span, cancellationToken);
        }

        /// <summary>
        /// Throws an <see cref="ObjectDisposedException"/> if this object is disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposed()
        {
            if (Disposed)
            {
                ThrowObjectDisposedException();
            }
        }

        /// <exception cref="ObjectDisposedException">The object is disposed.</exception>
        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(nameof(RomData));
        }
    }
}
