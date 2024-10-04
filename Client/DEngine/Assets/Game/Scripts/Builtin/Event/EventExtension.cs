using System;
using DEngine.Event;
using DEngine.Runtime;

namespace Game.EventSystem
{
    public static class EventExtension
    {
        /// <summary>
        /// 安全的取消订阅事件处理回调函数。
        /// </summary>
        /// <param name="self"></param>
        /// <param name="id">事件类型编号。</param>
        /// <param name="handler">要取消订阅的事件处理回调函数。</param>
        public static void SafeUnSubscribe(this EventComponent self, int id, EventHandler<GameEventArgs> handler)
        {
            if (self.Check(id, handler))
            {
                self.Unsubscribe(id, handler);
            }
        }
    }
}