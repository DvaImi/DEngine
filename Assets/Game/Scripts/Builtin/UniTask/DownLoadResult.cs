using DEngine;

namespace Game
{
    /// <summary>
    /// DownLoad 结果
    /// </summary>
    public class DownLoadResult : IReference
    {
        /// <summary>
        /// 是否有错误
        /// </summary>
        public bool Success
        {
            get;
            private set;
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取下载后存放路径。
        /// </summary>
        public string DownloadPath
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取下载地址。
        /// </summary>
        public string DownloadUri
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取当前大小。
        /// </summary>
        public long CurrentLength
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        public static DownLoadResult Create(bool success, string errorMessage, string downloadPath, string downloadUri, long currentLength, object userData)
        {
            DownLoadResult downLoadResult = ReferencePool.Acquire<DownLoadResult>();
            downLoadResult.Success = success;
            downLoadResult.ErrorMessage = errorMessage;
            downLoadResult.DownloadPath = downloadPath;
            downLoadResult.DownloadUri = downloadUri;
            downLoadResult.CurrentLength = currentLength;
            downLoadResult.UserData = userData;
            return downLoadResult;
        }

        public void Clear()
        {
            Success = false;
            ErrorMessage = null;
            DownloadPath = null;
            DownloadUri = null;
            CurrentLength = 0;
            UserData = null;
        }
    }
}