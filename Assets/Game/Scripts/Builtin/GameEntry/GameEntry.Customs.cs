using Game.Archive;
using Game.Network;
using Game.Timer;

namespace Game
{
    public partial class GameEntry
    {
        public static TimerComponent Timer
        {
            get;
            private set;
        }

        public static ArchiveComponent Archive
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取网络组件。
        /// </summary>
        public static NetworkComponent Network
        {
            get;
            private set;
        }

        private static void InitCustomsComponents()
        {
            Timer = DEngine.Runtime.GameEntry.GetComponent<TimerComponent>();
            Archive = DEngine.Runtime.GameEntry.GetComponent<ArchiveComponent>();
            Network = DEngine.Runtime.GameEntry.GetComponent<NetworkComponent>();
        }
    }
}