using System;
using System.Collections.Generic;

namespace Game.FileSystem
{
    [Serializable]
    public class FileSystemDataVersion
    {
        public FileSystemDataVersion(string fileSystem, string fullPath)
        {
            FileSystem = fileSystem;
            FullPath = fullPath;
        }

        /// <summary>
        /// 资源文件系统
        /// </summary>
        public string FileSystem { get; set; }

        /// <summary>
        /// 获取文件系统完整路径。
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 获取文件数量。
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public List<string> AssetNames { get; set; } = new();

        /// <summary>
        /// 文件完整路径
        /// </summary>
        public List<string> AssetFullNames { get; set; } = new();
    }
}