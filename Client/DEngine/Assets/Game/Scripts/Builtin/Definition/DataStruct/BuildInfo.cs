// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 21:02:37
// 版 本：1.0
// ========================================================

using System;

namespace Game
{
    [Serializable]
    public class BuildInfo
    {
        /// <summary>
        /// 最新的游戏版本号
        /// </summary>
        public string LatestGameVersion = string.Empty;
        /// <summary>
        /// 请求版本文件接口
        /// </summary>
        public string CheckVersionUrl = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string WindowsAppUrl = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string MacOSAppUrl = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string IOSAppUrl = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string AndroidAppUrl = string.Empty;
    }
}
