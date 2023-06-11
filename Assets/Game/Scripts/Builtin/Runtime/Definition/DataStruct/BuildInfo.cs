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
        /// 请求版本文件接口
        /// </summary>
        public string CheckVersionUrl;
        /// <summary>
        /// 下载资源接口
        /// </summary>
        public string UpdatePrefixUri;
        /// <summary>
        /// 
        /// </summary>
        public string WindowsAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public string MacOSAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public string IOSAppUrl;
        /// <summary>
        /// 
        /// </summary>
        public string AndroidAppUrl;
    }
}
