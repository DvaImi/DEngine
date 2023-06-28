﻿namespace DEngine.Resource
{
    /// <summary>
    /// 本地读写区版本资源列表序列化器。
    /// </summary>
    public sealed class ReadWriteVersionListSerializer : DEngineSerializer<LocalVersionList>
    {
        private static readonly byte[] Header = new byte[] { (byte)'D', (byte)'E', (byte)'W' };

        /// <summary>
        /// 初始化本地读写区版本资源列表序列化器的新实例。
        /// </summary>
        public ReadWriteVersionListSerializer()
        {
        }

        /// <summary>
        /// 获取本地读写区版本资源列表头标识。
        /// </summary>
        /// <returns>本地读写区版本资源列表头标识。</returns>
        protected override byte[] GetHeader()
        {
            return Header;
        }
    }
}
