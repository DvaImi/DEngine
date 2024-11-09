namespace Game.Network
{
    public struct OnNetworkDisconnectEventArg
    {
        public INetworkChannel NetworkChannel { get; private set; }

        public OnNetworkDisconnectEventArg(INetworkChannel networkChannel)
        {
            NetworkChannel = networkChannel;
        }
    }
}