using System.IO;
using Microsoft.Toolkit.Diagnostics;

namespace Foundry
{
    public static partial class FileSystemInfoExtensions
    {
        /// <summary>
        /// Recursively deletes all files and directories in the <see cref="FileSystemInfo"/>
        /// object.
        /// </summary>
        /// <param name="fileSystemInfo">The file system info to delete the children of.</param>
        public static void DeleteChildren(this FileSystemInfo fileSystemInfo)
        {
            Guard.IsNotNull(fileSystemInfo, nameof(fileSystemInfo));

            if (fileSystemInfo is FileInfo fi)
            {
                fi.Delete();
            }
            else if (fileSystemInfo is DirectoryInfo di)
            {
                foreach (var element in di.EnumerateFileSystemInfos())
                {
                    if (element is DirectoryInfo d)
                    {
                        d.Delete(true);
                    }
                    else
                    {
                        element.Delete();
                    }
                }
            }
        }
    }
}
