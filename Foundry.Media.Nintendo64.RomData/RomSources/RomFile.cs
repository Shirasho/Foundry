using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Foundry.Media.Nintendo64.RomData
{
    internal sealed class RomFile : RomData
    {
        public override bool CanReload => true;

        private readonly FileInfo File;

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
