﻿namespace DEngine.Resource
{
    /// <summary>
    /// 资源包版本资源列表序列化器。
    /// </summary>
    public sealed class ResourcePackVersionListSerializer : DEngineSerializer<ResourcePackVersionList>
    {
        private static readonly byte[] Header = new byte[] { (byte)'D', (byte)'E', (byte)'K' };

        /// <summary>
        /// 初始化资源包版本资源列表序列化器的新实例。
        /// </summary>
        public ResourcePackVersionListSerializer()
        {
        }

        /// <summary>
        /// 获取资源包版本资源列表头标识。
        /// </summary>
        /// <returns>资源包版本资源列表头标识。</returns>
        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}
