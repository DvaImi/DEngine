using DEngine;

namespace Game
{
    /// <summary>
    /// web 访问结果
    /// </summary>
    public class WebRequestResult : IReference
    {
        /// <summary>
        /// web请求 返回数据
        /// </summary>
        public byte[] Bytes { get; private set; }
        /// <summary>
        /// 是否请求成功
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object UserData { get; private set; }

        public static WebRequestResult Create(byte[] bytes, bool success, string errorMessage, object userData)
        {
            WebRequestResult webRequestResult = ReferencePool.Acquire<WebRequestResult>();
            webRequestResult.Bytes = bytes;
            webRequestResult.Success = success;
            webRequestResult.ErrorMessage = errorMessage;
            webRequestResult.UserData = userData;
            return webRequestResult;
        }

        public void Clear()
        {
            Bytes = null;
            Success = false;
            ErrorMessage = string.Empty;
            UserData = null;
        }
    }
}