using DEngine.Event;

namespace Game.Network
{
    public class OnNetworkConnectedEventArg : GameEventArgs
    {
        public static int EventId = typeof(OnNetworkConnectedEventArg).GetHashCode();

        public override int Id
        {
            get => EventId;
        }

        public override void Clear()
        {
        }
    }
}