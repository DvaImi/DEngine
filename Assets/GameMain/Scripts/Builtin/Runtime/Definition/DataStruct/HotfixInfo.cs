// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 23:39:33
// 版 本：1.0
// ========================================================

using System.IO;
using GameFramework;

namespace GeminiLion
{
    public class HotfixInfo
    {
        /// <summary>
        /// 热更程序集生成路径
        /// </summary>
        public string HotfixDllPath
        {
            get; set;
        }
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

        /// <summary>
        /// 热更程序集后缀
        /// </summary>
        public string HotfixDllSuffix
        {
            get; set;
        }

        /// <summary>
        /// 热更新启动对象路径
        /// </summary>
        public string HotfixLauncher
        {
            get; set;
        }

        /// <summary>
        /// 获取热更新程序集
        /// </summary>
        /// <returns>返回带后缀的完整路径</returns>
        public string GetHotfixMainDllFullName()
        {
            return Utility.Path.GetRegularPath(Path.Combine(HotfixDllPath, HotfixDllNameMain + HotfixDllSuffix));
        }

        /// <summary>
        /// 获取AOT dll 程序集
        /// </summary>
        /// <returns>返回带后缀的完整路径</returns>
        public string[] GetAOTDllFullName()
        {
            if (AOTDllNames == null)
            {
                return null;
            }
            string[] result = new string[AOTDllNames.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = Utility.Path.GetRegularPath(Path.Combine(HotfixDllPath, AOTDllNames[i] + HotfixDllSuffix));
            }
            return result;
        }
    } 
}
