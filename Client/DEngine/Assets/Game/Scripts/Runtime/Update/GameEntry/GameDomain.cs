using Fantasy.Async;
using Game.Network;
using Game.Update.DataTable;
using Game.Update.Input;

namespace Game.Update
{
    /// <summary>
    /// 可更新入口
    /// </summary>
    public static class GameDomain
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

        /// <summary>
        /// 输入模块
        /// </summary>
        public static GameInputComponent Input { get; private set; }

        public static async FTask Initialize()
        {
            if (Scene == null || Scene.IsDisposed)
            {
                Scene = await Fantasy.Scene.Create();
            }

            Network = GameEntry.GetModule<INetworkModule>();
            Luban = GameEntry.GetModule<ILubanDataProvider>();
            Input = await Scene.AddComponentAsync<GameInputComponent>();
        }
    }
}