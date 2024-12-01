using Fantasy.Async;
using Fantasy.Entitas.Interface;

namespace Game.Update.Input
{
    public static class GameInputSystem
    {
        public class GameInputAwakeSystem : AwakeSystemAsync<GameInputComponent>
        {
            protected override async FTask Awake(GameInputComponent self)
            {
                await self.Awake();
            }
        }

        public class GameInputDestroySystem : DestroySystem<GameInputComponent>
        {
            protected override void Destroy(GameInputComponent self)
            {
                self.Destroy();
            }
        }
    }
}