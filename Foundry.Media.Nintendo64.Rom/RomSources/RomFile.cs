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

        /// <summary>
        /// Creates an <see cref="IRomData"/> instance using a <see cref="FileInfo"/> as the
        /// data source.
        /// </summary>
        /// <param name="file">The file containing the ROM data.</param>
        public static IRomData Create(FileInfo file)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var dump = MemoryPool<byte>.Shared.Rent((int)fs.Length);
            fs.Read(dump.Memory.Span);

            return new RomFile(file, dump);
        }

        /// <summary>
        /// Creates an <see cref="IRomData"/> instance using a <see cref="FileInfo"/> as the
        /// data source.
        /// </summary>
        /// <param name="file">The file containing the ROM data.</param>
        public static async ValueTask<IRomData> CreateAsync(FileInfo file, CancellationToken cancellationToken = default)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var dump = MemoryPool<byte>.Shared.Rent((int)fs.Length);
            await fs.ReadAsync(dump.Memory, cancellationToken);

            return new RomFile(file, dump);
        }

        /// <summary>
        /// Obtains the <see cref="RomHeader"/> for the ROM data in the file.
        /// </summary>
        /// <param name="file">The file containing the ROM data.</param>
        public static RomHeader GetHeaderFromFile(FileInfo file)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            Span<byte> headerBuffer = stackalloc byte[RomHeader.Length];

            fs.Read(headerBuffer);

            return RomHeader.Create(headerBuffer);
        }

        /// <summary>
        /// Obtains the <see cref="RomHeader"/> for the ROM data in the file.
        /// </summary>
        /// <param name="file">The file containing the ROM data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async ValueTask<RomHeader> GetHeaderFromFileAsync(FileInfo file, CancellationToken cancellationToken)
        {
            using var fs = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] headerBuffer = new byte[RomHeader.Length];

            await fs.ReadAsync(headerBuffer, cancellationToken);

            return RomHeader.Create(headerBuffer);
        }

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        protected override void LoadRomData()
        {
            using var fs = File.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var dump = MemoryPool<byte>.Shared.Rent((int)fs.Length);
            fs.Read(dump.Memory.Span);

            SetData(dump);
        }

        /// <summary>
        /// Reloads the ROM data into memory.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        protected override async ValueTask LoadRomDataAsync(CancellationToken cancellationToken)
        {
            using var fs = File.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            var dump = MemoryPool<byte>.Shared.Rent((int)fs.Length);
            await fs.ReadAsync(dump.Memory, cancellationToken);

            SetData(dump);
        }
    }
}
