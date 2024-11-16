using System;

namespace Game
{
    /// <summary>
    /// 资源压缩包信息
    /// </summary>
    [Serializable]
    public class CompressedBundleInfo
    {
        /// <summary>
        /// 压缩包文件名
        /// </summary>
        public string CompressedPackName { get; set; } = string.Empty;

        /// <summary>
        /// 压缩包文件大小（字节）
        /// </summary>
        public long CompressedPackLength { get; set; } = 0;

        /// <summary>
        /// 压缩包版本号
        /// </summary>
        public int Version { get; set; } = 0;
    }
}