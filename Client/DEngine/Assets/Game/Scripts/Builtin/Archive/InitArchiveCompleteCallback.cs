using System.Collections.Generic;

namespace Game.Archive
{
    /// <summary>
    /// 初始化存档系统完成时回调函数
    /// </summary>
    public delegate void InitArchiveCompleteCallback(SortedDictionary<string, ArchiveSlot> slots);
}