using Game.Network;
using Game.Timer;
using Game.Update.DataTable;

namespace Game.Update
{
    /// <summary>
    /// 可更新入口
    /// </summary>
    public static class Entry
    {
        /// <summary>
        /// 获取计时器模块
        /// </summary>
        public static ITimerModule Timer { get; private set; }

        /// <summary>
        /// 获取网络模块。
        /// </summary>
        public static INetworkModule Network { get; private set; }

        /// <summary>
        /// 数据表模块
        /// </summary>
        public static IDataTableProvider DataTable { get; private set; }

        /// <summary>
        /// Luban配表模块
        /// </summary>
        public static ILubanDataProvider Luban { get; private set; }

        public static void Initialize()
        {
            Timer = GameEntry.GetModule<ITimerModule>();
            Network = GameEntry.GetModule<INetworkModule>();
            DataTable = GameEntry.GetModule<IDataTableProvider>();
            Luban = GameEntry.GetModule<ILubanDataProvider>();
        }
    }
}