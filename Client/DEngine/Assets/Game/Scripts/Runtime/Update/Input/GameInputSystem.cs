using Fantasy.Entitas.Interface;

namespace Game.Update.Input
{
    public static class GameInputSystem
    {
        public class GameInputAwakeSystem : AwakeSystem<GameInputComponent>
        {
            protected override void Awake(GameInputComponent self)
            {
                self.Awake().Coroutine();
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