using System;
using System.Collections.Generic;

namespace Game.Archive
{
    [Serializable]
    public sealed class ArchiveSlot : IComparable<ArchiveSlot>
    {
        /// <summary>
        /// 存档栏名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 存档栏标识符
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 是否加密
        /// </summary>
        public bool UserEncryptor { get; set; }

        /// <summary>
        /// 存档栏索引值
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 存档栏描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 存档时间
        /// </summary>
        public long Timestamp { get; set; }

        /// <summary>
        /// 存档版本
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 缩略图信息
        /// </summary>
        public string ThumbnailIdentifier { get; set; }

        /// <summary>
        /// 是否是自动存档
        /// </summary>
        public bool IsAutoSave { get; set; }

        /// <summary>
        /// 数据目录
        /// </summary>
        public Dictionary<string, string> DataCatalog { get; set; } = new();

        /// <summary>
        /// 通过时间戳来排序
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(ArchiveSlot other)
        {
            return other == null ? 1 : Timestamp.CompareTo(other.Timestamp);
        }
    }
}