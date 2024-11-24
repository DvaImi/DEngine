using Game.AssetObject;

namespace Game
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        public static AssetObjectComponent Item { get; private set; }

        private static void InitCustomComponents()
        {
            Item = DEngine.Runtime.GameEntry.GetComponent<AssetObjectComponent>();
        }
    }
}