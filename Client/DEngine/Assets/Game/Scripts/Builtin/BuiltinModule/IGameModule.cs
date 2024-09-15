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
        /// 游戏模块轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        void Update(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 关闭并清理游戏模块。
        /// </summary>
        void Shutdown();
    }
}