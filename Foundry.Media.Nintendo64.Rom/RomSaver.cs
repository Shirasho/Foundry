using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry.Media.Nintendo64.Rom
{
    /// <summary>
    /// A utility for saving ROM data to disk.
    /// </summary>
    public static class RomSaver
    {
        /// <summary>
        /// Saves <paramref name="romData"/> to disk.
        /// </summary>
        /// <param name="romData">The data to save.</param>
        /// <param name="path">The output file path.</param>
        public static void Save(this IRomData romData, string path)
            => Save(romData, new FileInfo(path));

        /// <summary>
        /// Saves <paramref name="romData"/> to disk.
        /// </summary>
        /// <param name="romData">The data to save.</param>
        /// <param name="path">The output file path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static Task SaveAsync(this IRomData romData, string path, CancellationToken cancellationToken = default)
            => SaveAsync(romData, new FileInfo(path), cancellationToken);

        /// <summary>
        /// Saves <paramref name="romData"/> to disk.
        /// </summary>
        /// <param name="romData">The data to save.</param>
        /// <param name="file">The output file.</param>
        public static void Save(this IRomData romData, FileInfo file)
        {
            Guard.IsNotNull(romData, nameof(romData));
            Guard.IsNotNull(file, nameof(file));

            if (file.Directory?.Exists == false)
            {
                file.Directory.Create();
            }

            using var fs = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            fs.Write(romData.GetData().Span);
            fs.Flush();
        }

        /// <summary>
        /// Saves <paramref name="romData"/> to disk.
        /// </summary>
        /// <param name="romData">The data to save.</param>
        /// <param name="file">The output file.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task SaveAsync(this IRomData romData, FileInfo file, CancellationToken cancellationToken = default)
        {
            Guard.IsNotNull(romData, nameof(romData));
            Guard.IsNotNull(file, nameof(file));

            cancellationToken.ThrowIfCancellationRequested();

            if (file.Directory?.Exists == false)
            {
                file.Directory.Create();
            }

            using var fs = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            await fs.WriteAsync(await romData.GetDataAsync(), cancellationToken);
            await fs.FlushAsync();
        }

        /// <summary>
        /// Saves <paramref name="romData"/> to disk, using the metadata of the ROM
        /// to generate the output file name.
        /// </summary>
        /// <param name="romData">The data to save.</param>
        /// <param name="outputDirectory">The output directory.</param>
        public static void Save(this IRomData romData, DirectoryInfo outputDirectory)
        {
            Guard.IsNotNull(romData, nameof(romData));
            Guard.IsNotNull(outputDirectory, nameof(outputDirectory));

            if (!outputDirectory.Exists)
            {
                outputDirectory.Create();
            }

            string fileName = romData.GetFilename();
            var file = new FileInfo(Path.Combine(outputDirectory.FullName, fileName));

            using var fs = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            fs.Write(romData.GetData().Span);
            fs.Flush();
        }

        /// <summary>
        /// Saves <paramref name="romData"/> to disk, using the metadata of the ROM
        /// to generate the output file name.
        /// </summary>
        /// <param name="romData">The data to save.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static async Task SaveAsync(this IRomData romData, DirectoryInfo outputDirectory, CancellationToken cancellationToken)
        {
            Guard.IsNotNull(romData, nameof(romData));
            Guard.IsNotNull(outputDirectory, nameof(outputDirectory));

            cancellationToken.ThrowIfCancellationRequested();

            if (!outputDirectory.Exists)
            {
                outputDirectory.Create();
            }

            string fileName = romData.GetFilename();
            var file = new FileInfo(Path.Combine(outputDirectory.FullName, fileName));

            using var fs = file.Open(FileMode.Create, FileAccess.Write, FileShare.None);
            await fs.WriteAsync(await romData.GetDataAsync(), cancellationToken);
            await fs.FlushAsync();
        }
    }
}
