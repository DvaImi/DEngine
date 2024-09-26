using Fantasy.Async;
using Fantasy.Entitas;
using Fantasy.Event;
using Fantasy.Platform.Unity;

namespace Game.EventSystem
{
    public static class FantasyEventSystemExtension
    {
        /// <summary>
        /// 发布一个值类型的事件数据。
        /// </summary>
        /// <typeparam name="TEventData">事件数据类型（值类型）。</typeparam>
        /// <param name="eventData">事件数据实例。</param>
        public static void Publish<TEventData>(this EventComponent self, TEventData eventData) where TEventData : struct
        {
            Entry.Scene.EventComponent.Publish(eventData);
        }

        /// <summary>
        /// 发布一个继承自 Entity 的事件数据。
        /// </summary>
        /// <typeparam name="TEventData">事件数据类型（继承自 Entity）。</typeparam>
        /// <param name="eventData">事件数据实例。</param>
        /// <param name="isDisposed">是否释放事件数据。</param>
        public static void Publish<TEventData>(this EventComponent self, TEventData eventData, bool isDisposed = true) where TEventData : Entity
        {
            Entry.Scene.EventComponent.Publish(eventData, isDisposed);
        }

        /// <summary>
        /// 异步发布一个值类型的事件数据。
        /// </summary>
        /// <typeparam name="TEventData">事件数据类型（值类型）。</typeparam>
        /// <param name="eventData">事件数据实例。</param>
        /// <returns>表示异步操作的任务。</returns>
        public static async FTask PublishAsync<TEventData>(this EventComponent self, TEventData eventData) where TEventData : struct
        {
            await Entry.Scene.EventComponent.PublishAsync(eventData);
        }

        /// <summary>
        /// 异步发布一个继承自 Entity 的事件数据。
        /// </summary>
        /// <typeparam name="TEventData">事件数据类型（继承自 Entity）。</typeparam>
        /// <param name="eventData">事件数据实例。</param>
        /// <param name="isDisposed">是否释放事件数据。</param>
        /// <returns>表示异步操作的任务。</returns>
        public static async FTask PublishAsync<TEventData>(this EventComponent self, TEventData eventData, bool isDisposed = true) where TEventData : Entity
        {
            await Entry.Scene.EventComponent.PublishAsync(eventData, isDisposed);
        }
    }
}