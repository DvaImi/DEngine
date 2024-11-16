using Game.AssetItemObject;

namespace Game
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public partial class GameEntry
    {
        public static AssetItemObjectComponent Item { get; private set; }

        private static void InitCustomComponents()
        {
            Item = DEngine.Runtime.GameEntry.GetComponent<AssetItemObjectComponent>();
        }
    }
}