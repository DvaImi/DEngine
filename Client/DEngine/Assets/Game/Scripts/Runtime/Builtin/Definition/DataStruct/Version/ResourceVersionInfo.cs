using System;

namespace Game
{
    [Serializable]
    public class ResourceVersionInfo
    {
        /// <summary>
        /// 资源版本列表长度
        /// </summary>
        public int VersionListLength { get; set; } = 0;

        /// <summary>
        /// 资源版本列表哈希值
        /// </summary>
        public int VersionListHashCode { get; set; } = 0;

        /// <summary>
        /// 资源版本列表压缩后长度
        /// </summary>
        public int VersionListCompressedLength { get; set; } = 0;

        /// <summary>
        /// 资源版本列表压缩后哈希值
        /// </summary>
        public int VersionListCompressedHashCode { get; set; } = 0;
    }
}