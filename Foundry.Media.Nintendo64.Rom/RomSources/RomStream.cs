using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// A ROM data source that has been loaded from a <see cref="System.IO.Stream"/>.
    /// </summary>
    public sealed class RomStream : RomData
    {
        /// <summary>
        /// Whether the ROM file contents can be unloaded from memory and
        /// reliably loaded at a later time.
        /// </summary>
        public override bool CanReload => Stream.CanRead && Stream.CanSeek;

        private readonly Stream Stream;
        private readonly bool DisposeStream;
        private readonly long StreamPosition;

        private RomStream(Stream romStream, IMemoryOwner<byte> data, long streamPosition, bool disposeStream)
            : base(data)
        {
            Stream = romStream;
            StreamPosition = streamPosition;
            DisposeStream = disposeStream;
        }

        protected override void OnDispose()
        {
            if (DisposeStream)
            {
                Stream.Dispose();
            }

            base.OnDispose();
        }

        /// <summary>
        /// Creates an <see cref="IRomData"/> instance using a <see cref="Stream"/> as the
        /// data source.
        /// </summary>
        /// <param name="romStream">The stream containing the ROM data.</param>
        /// <param name="disposeStream">Whether to dispose of the stream when the <see cref="IRomData"/> instance is disposed.</param>
        public static IRomData Create(Stream romStream, bool disposeStream)
        {
            var dump = MemoryPool<byte>.Shared.Rent((int)romStream.Length);
            long streamPosition = romStream.Position;
            romStream.Read(dump.Memory.Span);

            return new RomStream(romStream, dump, streamPosition, disposeStream);
        }

        /// <summary>
        /// Creates an <see cref="IRomData"/> instance using a <see cref="Stream"/> as the
        /// data source.
        /// </summary>
        /// <param name="romStream">The stream containing the ROM data.</param>
        /// <param name="disposeStream">Whether to dispose of the stream when the <see cref="IRomData"/> instance is disposed.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async ValueTask<IRomData> CreateAsync(Stream romStream, bool disposeStream, CancellationToken cancellationToken = default)
        {
            var dump = MemoryPool<byte>.Shared.Rent((int)romStream.Length);
            long streamPosition = romStream.Position;
            await romStream.ReadAsync(dump.Memory, cancellationToken);

            return new RomStream(romStream, dump, streamPosition, disposeStream);
        }

        /// <summary>
        /// Obtains the <see cref="RomHeader"/> for the ROM data in the stream.
        /// </summary>
        /// <param name="romStream">The stream containing the ROM data.</param>
        public static RomHeader GetHeaderFromStream(Stream romStream)
        {
            Span<byte> headerBuffer = stackalloc byte[RomHeader.Length];

            romStream.Read(headerBuffer);

            return RomHeader.Create(headerBuffer);
        }

        /// <summary>
        /// Obtains the <see cref="RomHeader"/> for the ROM data in the stream.
        /// </summary>
        /// <param name="romStream">The stream containing the ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async ValueTask<RomHeader> GetHeaderFromStreamAsync(Stream romStream, CancellationToken cancellationToken)
        {
            byte[] headerBuffer = new byte[RomHeader.Length];

            await romStream.ReadAsync(headerBuffer, cancellationToken);

            return RomHeader.Create(headerBuffer);
        }

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        protected override void LoadRomData()
        {
            var dump = MemoryPool<byte>.Shared.Rent((int)Stream.Length);

            Stream.Seek(StreamPosition, SeekOrigin.Begin);
            Stream.Read(dump.Memory.Span);

            SetData(dump);
        }

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async ValueTask LoadRomDataAsync(CancellationToken cancellationToken)
        {
            var dump = MemoryPool<byte>.Shared.Rent((int)Stream.Length);

            Stream.Seek(StreamPosition, SeekOrigin.Begin);
            await Stream.ReadAsync(dump.Memory, cancellationToken);

            SetData(dump);
        }
    }
}
