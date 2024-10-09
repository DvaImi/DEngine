using System;
using DEngine;
using DEngine.Event;

namespace Game
{
    public class EventSubscriber : IReference
    {
        private DEngineMultiDictionary<int, EventHandler<GameEventArgs>> dicEventHandler = new DEngineMultiDictionary<int, EventHandler<GameEventArgs>>();

        public object Owner
        {
            get;
            private set;
        }

        public EventSubscriber()
        {
            dicEventHandler = new DEngineMultiDictionary<int, EventHandler<GameEventArgs>>();
            Owner = null;
        }

        public void Subscribe(int id, EventHandler<GameEventArgs> handler)
        {
            if (handler == null)
            {
                throw new Exception("Event handler is invalid.");
            }

            dicEventHandler.Add(id, handler);
            GameEntry.Event.Subscribe(id, handler);
        }

        public void UnSubscribe(int id, EventHandler<GameEventArgs> handler)
        {
            if (!dicEventHandler.Remove(id, handler))
            {
                throw new Exception(Utility.Text.Format("Event '{0}' not exists specified handler.", id.ToString()));
            }

            GameEntry.Event.Unsubscribe(id, handler);
        }

        public void UnSubscribeAll()
        {
            if (dicEventHandler == null)
            {
                return;
            }

            foreach (var item in dicEventHandler)
            {
                foreach (var eventHandler in item.Value)
                {
                    GameEntry.Event.Unsubscribe(item.Key, eventHandler);
                }
            }

            dicEventHandler.Clear();
        }

        public static EventSubscriber Create(object owner)
        {
            EventSubscriber eventSubscriber = ReferencePool.Acquire<EventSubscriber>();
            eventSubscriber.Owner = owner;
            return eventSubscriber;
        }

        public void Clear()
        {
            dicEventHandler.Clear();
            Owner = null;
        }
    }
}