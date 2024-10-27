using Fantasy.Event;
using Fantasy.Platform.Unity;

namespace Game.Update
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class Entry
    {
        private class OnSceneCreateHandler : EventSystem<OnSceneCreate>
        {
            protected override void Handler(OnSceneCreate self)
            {
                Scene = self.Scene;
            }
        }
    }
}