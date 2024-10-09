using Fantasy.Event;
using Fantasy.Platform.Unity;
using Fantasy.Serialize;

namespace Game.Network
{
    public static class NetworkExtension
    {
        public static T Acquire<T>(this INetworkModule self) where T : AMessage, new()
        {
            return Entry.Scene.MessagePoolComponent.Rent<T>();
        }
    }
}