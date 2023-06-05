using System.Collections.Generic;
using GameFramework;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// UnityWebRequestHeader 请求头
    /// </summary>
    public class UnityWebRequestHeader : IReference
    {
        /// <summary>
        /// 设置表头
        /// </summary>
        public Dictionary<string, string> Header
        {
            get;
            private set;
        }

        public static UnityWebRequestHeader Creat(Dictionary<string, string> header)
        {
            UnityWebRequestHeader requestParams = ReferencePool.Acquire<UnityWebRequestHeader>();
            requestParams.Header = header;
            return requestParams;
        }

        public void Clear()
        {
            Header = null;
        }
    }
}
