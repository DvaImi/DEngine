namespace Game.Network
{
    public struct OnNetworkConnectedEventArg
    {
        public INetworkChannel NetworkChannel { get; private set; }

        public OnNetworkConnectedEventArg(INetworkChannel networkChannel)
        {
            NetworkChannel = networkChannel;
        }
    }
}