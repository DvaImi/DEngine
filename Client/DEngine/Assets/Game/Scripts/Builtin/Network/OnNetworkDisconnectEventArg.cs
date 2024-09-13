using DEngine.Event;

namespace Game.Network
{
    public class OnNetworkDisconnectEventArg : GameEventArgs
    {
        public static int EventId = typeof(OnNetworkDisconnectEventArg).GetHashCode();

        public override int Id
        {
            get => EventId;
        }

        public override void Clear()
        {
        }
    }
}