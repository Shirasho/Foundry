using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.RomData
{
    /// <summary>
    /// The base class for <see cref="IRomData"/> implementations.
    /// </summary>
    public abstract class RomData : IRomData
    {
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
        /// The ROM metadata.
        /// </summary>
        public RomMetadata Metadata { get; private set; }

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
        /// This method will throw an <see cref="InvalidOperationException" /> if <see cref="P:Foundry.Media.Nintendo64.RomData.IRomData.CanReload" />
        /// returns <see langword="false" />.
        /// </remarks>
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
        /// This method will throw an <see cref="InvalidOperationException" /> if <see cref="P:Foundry.Media.Nintendo64.RomData.IRomData.CanReload" />
        /// returns <see langword="false" />.
        /// </remarks>
        public ValueTask LoadAsync()
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
        protected abstract ValueTask LoadRomDataAsync();

        /// <summary>
        /// Unloads the ROM data from memory.
        /// </summary>
        /// <remarks>
        /// To check whether it is safe to call this method and reliably
        /// reload the contents, check <see cref="P:Foundry.Media.Nintendo64.RomData.IRomData.CanReload" /> before calling
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
        /// and <see cref="P:Foundry.Media.Nintendo64.RomData.IRomData.CanReload" /> is <see langword="false" />.
        /// </para>
        /// </remarks>
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
        /// and <see cref="P:Foundry.Media.Nintendo64.RomData.IRomData.CanReload" /> is <see langword="false" />.
        /// </para>
        /// </remarks>
        public async ValueTask<ReadOnlyMemory<byte>> GetDataAsync()
        {
            CheckDisposed();

            if (Data is null)
            {
                await LoadAsync();
            }

            return Data!.Memory;
        }

        /// <summary>
        /// Returns a filename for the ROM that is constructed using ROM
        /// metadata.
        /// </summary>
        public string GetFilename()
        {
            string title = Metadata.Title;
            string extension = Metadata.GetFormatExtension();

            return $"{title}{extension}";
        }

        /// <summary>
        /// Sets <see cref="Data"/> with the specified value and updates the values
        /// of <see cref="Metadata"/> and <see cref="Size"/>.
        /// </summary>
        /// <param name="data">The new data buffer.</param>
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
                    Metadata = default;
                    Size = default;
                }
            }

            if (Data is not null)
            {
                Metadata = RomMetadata.Create(Data.Memory.Span);
                Size = new RomSize(Data.Memory.Span.Length);
            }
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
        /// <param name="romPath">The file path to the ROM.</param>
        public static IRomData LoadRom(FileInfo romFile)
        {
            Guard.IsNotNull(romFile, nameof(romFile));

            return RomFile.Create(romFile);
        }

        /// <summary>
        /// Loads a ROM at the specified file location.
        /// </summary>
        /// <param name="romPath">The file path to the ROM.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static ValueTask<IRomData> LoadRomAsync(FileInfo romFile, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romFile, nameof(romFile));

            return RomFile.CreateAsync(romFile, cancellationToken);
        }

        public static IRomData LoadRom(Stream romStream, bool disposeStream = false)
        {
            Guard.IsNotNull(romStream, nameof(romStream));

            return RomStream.Create(romStream, disposeStream);
        }

        public static ValueTask<IRomData> LoadRomAsync(Stream romStream, bool disposeStream = false, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romStream, nameof(romStream));

            return RomStream.CreateAsync(romStream, disposeStream, cancellationToken);
        }

        public static IRomData LoadRom(Memory<byte> romData)
        {
            return new RomMemory(romData);
        }

        public static ValueTask<IRomData> LoadRomAsync(Memory<byte> romData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<IRomData>(new RomMemory(romData));
        }

        public static IRomData LoadRom(ReadOnlyMemory<byte> romData)
        {
            return new RomMemory(romData);
        }

        public static ValueTask<IRomData> LoadRomAsync(ReadOnlyMemory<byte> romData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<IRomData>(new RomMemory(romData));
        }

        public static IRomData LoadRom(IMemoryOwner<byte> romData)
        {
            return new RomMemory(romData);
        }

        public static ValueTask<IRomData> LoadRomAsync(IMemoryOwner<byte> romData, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romData, nameof(romData));

            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<IRomData>(new RomMemory(romData));
        }

        public static RomMetadata GetMetadata(string romPath)
            => GetMetadata(new FileInfo(romPath));

        public static ValueTask<RomMetadata> GetMetadataAsync(string romPath, CancellationToken cancellationToken = default)
            => GetMetadataAsync(new FileInfo(romPath), cancellationToken);

        public static RomMetadata GetMetadata(FileInfo romFile)
        {
            Guard.IsNotNull(romFile, nameof(romFile));

            return RomFile.GetMetadata(romFile);
        }

        public static ValueTask<RomMetadata> GetMetadataAsync(FileInfo romFile, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romFile, nameof(romFile));

            return RomFile.GetMetadataAsync(romFile, cancellationToken);
        }

        public static RomMetadata GetMetadata(Stream romStream)
        {
            Guard.IsNotNull(romStream, nameof(romStream));

            return RomStream.GetMetadata(romStream);
        }

        public static ValueTask<RomMetadata> GetMetadataAsync(Stream romStream, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romStream, nameof(romStream));

            return RomStream.GetMetadataAsync(romStream, cancellationToken);
        }

        public static RomMetadata GetMetadata(ReadOnlyMemory<byte> romData)
            => GetMetadata(romData.Span);

        public static ValueTask<RomMetadata> GetMetadataAsync(ReadOnlyMemory<byte> romData, CancellationToken cancellationToken = default)
            => GetMetadataAsync(romData.Span, cancellationToken);

        public static RomMetadata GetMetadata(ReadOnlySpan<byte> romData)
        {
            return RomMetadata.Create(romData);
        }

        public static ValueTask<RomMetadata> GetMetadataAsync(ReadOnlySpan<byte> romData, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return new ValueTask<RomMetadata>(RomMetadata.Create(romData));
        }

        public static RomMetadata GetMetadata(IMemoryOwner<byte> romData)
        {
            Guard.IsNotNull(romData, nameof(romData));

            return GetMetadata(romData.Memory.Span);
        }

        public static ValueTask<RomMetadata> GetMetadataAsync(IMemoryOwner<byte> romData, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romData, nameof(romData));

            return GetMetadataAsync(romData.Memory.Span, cancellationToken);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposed()
        {
            if (Disposed)
            {
                ThrowObjectDisposedException();
            }
        }

        [DoesNotReturn, MethodImpl(MethodImplOptions.NoInlining)]
        private void ThrowObjectDisposedException()
        {
            throw new ObjectDisposedException(nameof(RomData));
        }
    }
}
