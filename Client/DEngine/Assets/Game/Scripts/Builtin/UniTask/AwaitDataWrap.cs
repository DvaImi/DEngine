using Cysharp.Threading.Tasks;
using DEngine;

namespace Game
{
    /// <summary>
    /// Await包装类
    /// </summary>
    public class AwaitDataWrap<T> : IReference
    {
        /// <summary>
        /// UniTaskCompletionSource
        /// </summary>
        public UniTaskCompletionSource<T> Source { get; private set; }

        /// <summary>
        /// 自定义数据
        /// </summary>
        public object UserData { get; private set; }

        public static AwaitDataWrap<T> Create(UniTaskCompletionSource<T> source, object userData = null)
        {
            AwaitDataWrap<T> awaitDataWrap = ReferencePool.Acquire<AwaitDataWrap<T>>();
            awaitDataWrap.Source = source;
            awaitDataWrap.UserData = userData;
            return awaitDataWrap;
        }

        public void Clear()
        {
            UserData = null;
            Source = null;
        }
    }
}