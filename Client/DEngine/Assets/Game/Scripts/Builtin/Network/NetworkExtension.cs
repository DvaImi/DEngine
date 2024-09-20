using Fantasy.Serialize;

namespace Game.Network
{
    public static class NetworkExtension
    {
        public static T Acquire<T>(this NetworkComponent self) where T : AMessage, new()
        {
            return self.Scene.MessagePoolComponent.Rent<T>();
        }
    }
}