using System;
using DEngine;
using DEngine.Event;

namespace Game
{
    public class EventSubscriber : IReference
    {
        private readonly DEngineMultiDictionary<int, EventHandler<GameEventArgs>> m_DicEventHandler = new();

        public object Owner { get; private set; }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            if (handler == null)
            {
                throw new Exception("Event handler is invalid.");
            }

            m_DicEventHandler.Add(id, handler);
            GameEntry.Event.Subscribe(id, handler);
        }

        public void UnSubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            if (!m_DicEventHandler.Remove(id, handler))
            {
                throw new Exception(Utility.Text.Format("Event '{0}' not exists specified handler.", id.ToString()));
            }

            GameEntry.Event.Unsubscribe(id, handler);
        }

        public void UnSubscribeAll()
        {
            if (m_DicEventHandler == null)
            {
                return;
            }

            foreach (var item in m_DicEventHandler)
            {
                foreach (var eventHandler in item.Value)
                {
                    GameEntry.Event.Unsubscribe(item.Key, eventHandler);
                }
            }

            m_DicEventHandler.Clear();
        }

        public static EventSubscriber Create(object owner)
        {
            EventSubscriber eventSubscriber = ReferencePool.Acquire<EventSubscriber>();
            eventSubscriber.Owner = owner;
            return eventSubscriber;
        }

        public void Clear()
        {
            m_DicEventHandler.Clear();
            Owner = null;
        }
    }
}