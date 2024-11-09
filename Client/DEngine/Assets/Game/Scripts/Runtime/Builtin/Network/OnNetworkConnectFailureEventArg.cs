namespace Game.Network
{
    public struct OnNetworkConnectFailureEventArg
    {
        public INetworkChannel NetworkChannel { get; private set; }
        
        public OnNetworkConnectFailureEventArg(INetworkChannel networkChannel)
        {
            NetworkChannel = networkChannel;
        }
    }
}