using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Foundry
{
    /// <summary>
    /// Helpers for interacting with the .NET Runtime or SDK.
    /// </summary>
    public static class DotNetCore
    {
        /// <summary>
        /// The dotnet filename without the extension.
        /// </summary>
        public const string Filename = "dotnet";

        /// <summary>
        /// The full path to the dotnet executable used to run this application.
        /// If on Windows, the extension will be appended to the path.
        /// </summary>
        public static string? FullPath { get; } = TryGetFullPath();

        private static string? TryGetFullPath()
        {
            string filename = Filename;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                filename += ".exe";
            }

            var mainModule = Process.GetCurrentProcess().MainModule;
            if (!string.IsNullOrEmpty(mainModule?.FileName) && Path.GetFileName(mainModule!.FileName).Equals(filename, StringComparison.OrdinalIgnoreCase))
            {
                return mainModule.FileName;
            }

            string dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT");
            if (!string.IsNullOrEmpty(dotnetRoot))
            {
                return Path.Combine(dotnetRoot, filename);
            }

            return null;
        }
    }
}
