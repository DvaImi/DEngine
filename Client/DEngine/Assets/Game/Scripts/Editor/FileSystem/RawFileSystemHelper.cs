using System;
using DEngine.FileSystem;
using DEngine.Runtime;

namespace Game.Editor.ResourceTools
{
    public sealed class RawFileSystemHelper : IFileSystemHelper
    {
        /// <summary>
        /// 创建文件系统流。
        /// </summary>
        /// <param name="fullPath">要加载的文件系统的完整路径。</param>
        /// <param name="access">要加载的文件系统的访问方式。</param>
        /// <param name="createNew">是否创建新的文件系统流。</param>
        /// <returns>创建的文件系统流。</returns>
        public FileSystemStream CreateFileSystemStream(string fullPath, FileSystemAccess access, bool createNew)
        {
            if (fullPath.StartsWith("jar:", StringComparison.Ordinal))
            {
                return new AndroidFileSystemStream(fullPath, access, createNew);
            }

            return new CommonFileSystemStream(fullPath, access, createNew);
        }
    }
}