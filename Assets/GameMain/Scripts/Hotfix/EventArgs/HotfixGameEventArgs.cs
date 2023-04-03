// ========================================================
// 描述：
// 作者：Dvalmi 
// 创建时间：2023-04-01 17:20:03
// 版 本：1.0
// ========================================================
using GameFramework.Event;

namespace Dvalmi.Hotfix
{
    public abstract class HotfixGameEventArgs : GameEventArgs
    {
        public override int Id { get => throw new System.NotImplementedException(); }

        public override void Clear()
        {

        }
    }

}