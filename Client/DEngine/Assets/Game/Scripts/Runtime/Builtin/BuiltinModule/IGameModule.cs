namespace Game
{
    /// <summary>
    /// 内置游戏模块接口
    /// </summary>
    public interface IGameModule
    {
        /// <summary>
        /// 获取游戏模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int Priority { get; }

        /// <summary>
        /// 关闭并清理游戏模块。
        /// </summary>
        void Shutdown();
    }
}