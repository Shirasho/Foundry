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

        public static IRomData Create(Stream romStream, bool disposeStream)
        {
            var dump = MemoryPool<byte>.Shared.Rent((int)romStream.Length);
            long streamPosition = romStream.Position;
            romStream.Read(dump.Memory.Span);

            return new RomStream(romStream, dump, streamPosition, disposeStream);
        }

        public static async ValueTask<IRomData> CreateAsync(Stream romStream, bool disposeStream, CancellationToken cancellationToken = default)
        {
            var dump = MemoryPool<byte>.Shared.Rent((int)romStream.Length);
            long streamPosition = romStream.Position;
            await romStream.ReadAsync(dump.Memory, cancellationToken);

            return new RomStream(romStream, dump, streamPosition, disposeStream);
        }

        public static new RomHeader GetMetadata(Stream romStream)
        {
            Span<byte> headerBuffer = stackalloc byte[RomHeader.Length];

            romStream.Read(headerBuffer);

            return RomHeader.Create(headerBuffer);
        }

        public static new async ValueTask<RomHeader> GetMetadataAsync(Stream romStream, CancellationToken cancellationToken)
        {
            byte[] headerBuffer = new byte[RomHeader.Length];

            await romStream.ReadAsync(headerBuffer, cancellationToken);

            return RomHeader.Create(headerBuffer);
        }

        protected override void LoadRomData()
        {
            var dump = MemoryPool<byte>.Shared.Rent((int)Stream.Length);

            Stream.Seek(StreamPosition, SeekOrigin.Begin);
            Stream.Read(dump.Memory.Span);

            SetData(dump);
        }

        protected override async ValueTask LoadRomDataAsync()
        {
            var dump = MemoryPool<byte>.Shared.Rent((int)Stream.Length);

            Stream.Seek(StreamPosition, SeekOrigin.Begin);
            await Stream.ReadAsync(dump.Memory);

            SetData(dump);
        }
    }
}
