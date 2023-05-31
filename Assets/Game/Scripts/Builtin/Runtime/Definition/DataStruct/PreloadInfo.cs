// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-16 20:39:19
// 版 本：1.0
// ========================================================

using System.Collections.Generic;

namespace Game
{
    public class PreloadInfo
    {
        /// <summary>
        /// 数据表
        /// </summary>
        public List<string> DateTable
        {
            get;
            set;
        }

        /// <summary>
        /// 字典
        /// </summary>
        public List<string> Dictionary
        {
            get;
            set;
        }

        /// <summary>
        /// 配置
        /// </summary>
        public List<string> Config
        {
            get;
            set;
        }
    } 
}
