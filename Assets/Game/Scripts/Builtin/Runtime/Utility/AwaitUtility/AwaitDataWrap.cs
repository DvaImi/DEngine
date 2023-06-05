using Cysharp.Threading.Tasks;
using GameFramework;

namespace Game
{
    /// <summary>
    /// Await包装类
    /// </summary>
    public class AwaitDataWrap<T> : IReference
    {
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// UniTaskCompletionSource
        /// </summary>
        public UniTaskCompletionSource<T> Source
        {
            get;
            private set;
        }

        public static AwaitDataWrap<T> Create(object userData, UniTaskCompletionSource<T> source)
        {
            AwaitDataWrap<T> awaitDataWrap = ReferencePool.Acquire<AwaitDataWrap<T>>();
            awaitDataWrap.UserData = userData;
            awaitDataWrap.Source = source;
            return awaitDataWrap;
        }

        public void Clear()
        {
            UserData = null;
            Source = null;
        }
    }
}