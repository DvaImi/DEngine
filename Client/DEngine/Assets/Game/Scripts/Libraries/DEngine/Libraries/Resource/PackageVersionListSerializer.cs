﻿namespace DEngine.Resource
{
    /// <summary>
    /// 单机模式版本资源列表序列化器。
    /// </summary>
    public sealed class PackageVersionListSerializer : DEngineSerializer<PackageVersionList>
    {
        private static readonly byte[] Header = new byte[] { (byte)'D', (byte)'E', (byte)'P' };

        /// <summary>
        /// 初始化单机模式版本资源列表序列化器的新实例。
        /// </summary>
        public PackageVersionListSerializer()
        {
        }

        /// <summary>
        /// 获取单机模式版本资源列表头标识。
        /// </summary>
        /// <returns>单机模式版本资源列表头标识。</returns>
        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}
