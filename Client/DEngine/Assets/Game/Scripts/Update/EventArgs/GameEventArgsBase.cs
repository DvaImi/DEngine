// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-01 17:20:03
// 版 本：1.0
// ========================================================

using DEngine.Event;

namespace Game.Update.Event
{
    public abstract class GameEventArgsBase<T> : GameEventArgs
    {
        public static int EventId = typeof(T).GetHashCode();

        public override int Id => EventId;
    }
}