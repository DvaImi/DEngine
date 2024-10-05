using System;

namespace Game.Archive
{
    public interface IArchiveData
    {
        /// <summary>
        /// 文件标识符
        /// </summary>
        string Identifier { get; }

        /// <summary>
        /// 
        /// </summary>
        string UniqueId { get; }
    }
}