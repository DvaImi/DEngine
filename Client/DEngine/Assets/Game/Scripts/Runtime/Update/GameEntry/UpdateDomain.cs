using Fantasy.Platform.Unity;
using Game.Network;
using Game.Update.DataTable;

namespace Game.Update
{
    /// <summary>
    /// 可更新入口
    /// </summary>
    public static class UpdateDomain
    {
        /// <summary>
        /// 根节点
        /// </summary>
        public static Fantasy.Scene Scene { get; private set; }

        /// <summary>
        /// 获取网络模块。
        /// </summary>
        public static INetworkModule Network { get; private set; }

        /// <summary>
        /// Luban配表模块
        /// </summary>
        public static ILubanDataProvider Luban { get; private set; }

        public static void Initialize(Fantasy.Scene scene)
        {
            Scene = scene;
            Network = GameEntry.GetModule<INetworkModule>();
            Luban = GameEntry.GetModule<ILubanDataProvider>();
        }
    }
}