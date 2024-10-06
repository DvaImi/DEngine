using System;
using System.Collections.Generic;

namespace Game.DataTable
{
    [Serializable]
    public class GameDataTableVersion
    {
        /// <summary>
        /// 常驻数据表
        /// </summary>
        public HashSet<string> StaticDataTable { get; set; } = new();

        /// <summary>
        /// 动态数据表
        /// </summary>
        public HashSet<string> DynamicDataTable { get; set; } = new();
    }
}