// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 23:39:33
// 版 本：1.0
// ========================================================

namespace Game
{
    public class HotfixInfo
    {
        /// <summary>
        /// 主热更程序集
        /// </summary>
        public string HotfixDllNameMain
        {
            get; set;
        }

        /// <summary>
        /// AOT 程序集
        /// </summary>
        public string[] AOTDllNames
        {
            get; set;
        }

        /// <summary>
        /// 其他预留热更新程序集
        /// </summary>
        public string[] PreserveHotfixDllNames
        {
            get; set;
        }
    }
}
