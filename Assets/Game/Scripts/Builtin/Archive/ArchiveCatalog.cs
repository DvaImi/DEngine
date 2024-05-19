using System;
using System.Collections.Generic;

namespace Game.Archive
{
    [Serializable]
    public sealed class ArchiveCatalog
    {
        /// <summary>
        /// 所有存档卡槽
        /// </summary>
        public List<string> Slot;

        /// <summary>
        /// 存档版本
        /// </summary>
        public int Version;
    }
}