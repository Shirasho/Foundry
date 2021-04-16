using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// A ROM data source that has been loaded from a file.
    /// </summary>
    public sealed class RomFile : RomData
    {
        /// <summary>
        /// Whether the ROM file contents can be unloaded from memory and
        /// reliably loaded at a later time.
        /// </summary>
        public override bool CanReload => true;

        /// <summary>
        /// The ROM data file.
        /// </summary>
        public FileInfo File { get; }

        private RomFile(FileInfo file, IMemoryOwner<byte> data)
            : base(data)
        {
            File = file;
        }

        public static IRomData Create(FileInfo file)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var dump = MemoryPool<byte>.Shared.Rent((int)fs.Length);
            fs.Read(dump.Memory.Span);

            return new RomFile(file, dump);
        }

        public static async ValueTask<IRomData> CreateAsync(FileInfo file, CancellationToken cancellationToken = default)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var dump = MemoryPool<byte>.Shared.Rent((int)fs.Length);
            await fs.ReadAsync(dump.Memory, cancellationToken);

            return new RomFile(file, dump);
        }

        public static new RomMetadata GetMetadata(FileInfo file)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            Span<byte> headerBuffer = stackalloc byte[RomMetadata.HeaderLength];

            fs.Read(headerBuffer);

            return RomMetadata.Create(headerBuffer);
        }

        public static new async ValueTask<RomMetadata> GetMetadataAsync(FileInfo file, CancellationToken cancellationToken)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] headerBuffer = new byte[RomMetadata.HeaderLength];

            await fs.ReadAsync(headerBuffer, cancellationToken);

            return RomMetadata.Create(headerBuffer);
        }

        protected override void LoadRomData()
        {
            using var fs = File.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var dump = MemoryPool<byte>.Shared.Rent((int)fs.Length);
            fs.Read(dump.Memory.Span);

            SetData(dump);
        }

        protected override async ValueTask LoadRomDataAsync()
        {
            using var fs = File.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var dump = MemoryPool<byte>.Shared.Rent((int)fs.Length);
            await fs.ReadAsync(dump.Memory);

            SetData(dump);
        }
    }
}
