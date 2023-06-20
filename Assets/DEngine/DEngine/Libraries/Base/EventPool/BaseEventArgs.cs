namespace DEngine
{
    /// <summary>
    /// 事件基类。
    /// </summary>
    public abstract class BaseEventArgs : DEngineEventArgs
    {
        /// <summary>
        /// 获取类型编号。
        /// </summary>
        public abstract int Id
        {
            get;
        }
    }
}
