using DEngine.Event;

namespace Game.Network
{
    public class OnNetworkConnectFailureEventArg : GameEventArgs
    {
        public static int EventId = typeof(OnNetworkConnectFailureEventArg).GetHashCode();

        public override int Id
        {
            get => EventId;
        }

        public override void Clear()
        {
        }
    }
}