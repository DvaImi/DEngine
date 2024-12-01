using System;

namespace Game
{
    /// <summary>
    /// 资源包信息
    /// </summary>
    [Serializable]
    public class ResourcePackInfo
    {
        /// <summary>
        /// 资源包名
        /// </summary>
        public string ResourcePackName { get; set; } = string.Empty;

        /// <summary>
        /// 资源包文件大小（字节）
        /// </summary>
        public long ResourcePackLength { get; set; } = 0;

        /// <summary>
        /// 资源包版本号
        /// </summary>
        public int Version { get; set; } = 0;
    }
}