using Fantasy.Async;
using Fantasy.Entitas.Interface;
using Game.Debugger;

namespace Game.Network
{
    public partial class NetworkModule
    {
        private class NetworkModuleAwakeSystem : AwakeSystem<NetworkModule>
        {
            protected override void Awake(NetworkModule self)
            {
                GameEntry.Debugger.RegisterDebuggerWindow("Profiler/Network", new NetworkDebuggerWindow(), self);
            }
        }

        private class NetworkModuleDestroySystem : DestroySystem<NetworkModule>
        {
            protected override void Destroy(NetworkModule self)
            {
                self.Shutdown();
            }
        }
    }
}