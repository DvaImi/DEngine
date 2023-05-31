using GameFramework;

namespace Game.Await
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
        /// 是否有错误
        /// </summary>
        public bool IsError { get; private set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object UserData { get; private set; }


        public static WebRequestResult Create(byte[] bytes, bool isError, string errorMessage, object userData)
        {
            WebRequestResult webRequestResult = ReferencePool.Acquire<WebRequestResult>();
            webRequestResult.Bytes = bytes;
            webRequestResult.IsError = isError;
            webRequestResult.ErrorMessage = errorMessage;
            webRequestResult.UserData = userData;
            return webRequestResult;
        }
        
        public void Clear()
        {
            Bytes = null;
            IsError = false;
            ErrorMessage = string.Empty;
            UserData = null;
        }
    }
}