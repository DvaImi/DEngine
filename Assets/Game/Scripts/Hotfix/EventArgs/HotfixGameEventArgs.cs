// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-01 17:20:03
// 版 本：1.0
// ========================================================
using GameFramework.Event;

namespace Game.Hotfix
{
    public abstract class HotfixGameEventArgs : GameEventArgs
    {
        public static int EventId = typeof(HotfixGameEventArgs).GetHashCode();
        public override int Id { get => EventId; }

        public override void Clear()
        {

        }
    }

}