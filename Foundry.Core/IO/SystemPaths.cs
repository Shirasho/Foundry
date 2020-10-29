using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Foundry.Core.IO
{
    /// <summary>
    /// System paths.
    /// </summary>
    public static class SystemPaths
    {
        /// <summary>
        /// The user's roaming AppData directory.
        /// </summary>
        public static DirectoryInfo AppDataRoaming { get; } = CreateDirectory(Environment.SpecialFolder.ApplicationData);

        /// <summary>
        /// The user's local AppData directory.
        /// </summary>
        public static DirectoryInfo AppDataLocal { get; } = CreateDirectory(Environment.SpecialFolder.LocalApplicationData);

        /// <summary>
        /// Program data directory.
        /// </summary>
        public static DirectoryInfo ProgramData { get; } = CreateDirectory(Environment.SpecialFolder.CommonApplicationData);

        /// <summary>
        /// Temp directory.
        /// </summary>
        public static DirectoryInfo Temp { get; } = new DirectoryInfo(Path.GetTempPath());

        /// <summary>
        /// Generates a temporary file on disk.
        /// </summary>
        public static FileInfo GenerateTempFile()
            => Temp.CombineFile(Path.GetTempFileName());

        /// <summary>
        /// Creates a <see cref="DirectoryInfo"/> from a <see cref="Environment.SpecialFolder"/>.
        /// </summary>
        /// <param name="folder">The system folder.</param>
        /// <param name="paths">Additional path segments.</param>
        public static DirectoryInfo CreateDirectory(Environment.SpecialFolder folder, params string?[]? paths)
        {
            var p = paths?.ToList() ?? new List<string?>();
            p.Insert(0, Environment.GetFolderPath(folder));

            return new DirectoryInfo(Path.Combine(p.ToArray()));
        }

        /// <summary>
        /// Creates a <see cref="FileInfo"/> from a <see cref="Environment.SpecialFolder"/>.
        /// </summary>
        /// <param name="folder">The system folder.</param>
        /// <param name="paths">Additional path segments.</param>
        /// <exception cref="UnauthorizedAccessException">Access to the filename is denied.</exception>
        public static FileInfo CreateFile(Environment.SpecialFolder folder, params string?[]? paths)
        {
            var p = paths?.ToList() ?? new List<string?>();
            p.Insert(0, Environment.GetFolderPath(folder));

            return new FileInfo(Path.Combine(p.ToArray()));
        }
    }
}
