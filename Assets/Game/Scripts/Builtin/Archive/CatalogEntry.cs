namespace Game.Archive
{
    public class CatalogEntry
    {
        /// <summary>
        /// 标识符
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// 完整路径
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long CreationTime { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public long LastModifiedTime { get; set; }
    }
}