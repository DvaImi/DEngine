using Game.Network;
using Game.Timer;

namespace Game
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        /// <summary>
        /// 获取计时器模块
        /// </summary>
        public static ITimerModule Timer { get; private set; }

        /// <summary>
        /// 获取网络模块。
        /// </summary>
        public static INetworkModule Network { get; private set; }

        private static void InitCustomsModules()
        {
            Timer = GetModule<ITimerModule>();
            Network = GetModule<INetworkModule>();
        }
    }
}