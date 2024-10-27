using Cysharp.Threading.Tasks;
using Fantasy.Event;
using Fantasy.Platform.Unity;
using Game.Network;
using Game.Update.DataTable;

namespace Game.Update
{
    /// <summary>
    /// 可更新入口
    /// </summary>
    public static partial class Entry
    {
        public static Fantasy.Scene Scene { get; private set; }

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
            Network = GameEntry.GetModule<INetworkModule>();
            DataTable = GameEntry.GetModule<IDataTableProvider>();
            Luban = GameEntry.GetModule<ILubanDataProvider>();
        }
    }
}