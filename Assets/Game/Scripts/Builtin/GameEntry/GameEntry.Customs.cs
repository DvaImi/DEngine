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


        private static void InitCustomsComponents()
        {
            Timer = DEngine.Runtime.GameEntry.GetComponent<TimerComponent>();
        }
    }
}
