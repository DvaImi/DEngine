// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-15 21:02:37
// 版 本：1.0
// ========================================================

namespace Dvalmi
{
    public class BuildInfo
    {
        /// <summary>
        /// 游戏版本号
        /// </summary>
        public string GameVersion
        {
            get;
            set;
        }
        /// <summary>
        /// 游戏内部版本号
        /// </summary>
        public int InternalGameVersion
        {
            get;
            set;
        }
        /// <summary>
        /// 请求版本文件接口
        /// </summary>
        public string CheckVersionUrl
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string WindowsAppUrl
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string MacOSAppUrl
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string IOSAppUrl
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public string AndroidAppUrl
        {
            get;
            set;
        }
        /// <summary>
        /// 下载资源接口
        /// </summary>
        public string UpdatePrefixUri
        {
            get;
            set;
        }
    }
}
