using System;
using System.Collections.Generic;
using DEngine.Runtime;

namespace Game.Timer
{
    public class CancellationToken
    {
        private HashSet<Action> m_Actions = new HashSet<Action>();

        public bool Invalid => m_Actions == null || m_Actions.Count == 0;

        public void Add(Action callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException();
            }

            m_Actions.Add(callback);
        }

        public void Remove(Action callback)
        {
            m_Actions?.Remove(callback);
        }

        public void Cancel()
        {
            if (m_Actions == null)
            {
                return;
            }

            if (m_Actions.Count == 0)
            {
                return;
            }

            Invoke();
        }

        private void Invoke()
        {
            HashSet<Action> runActions = m_Actions;
            m_Actions = null;
            try
            {
                foreach (Action action in runActions)
                {
                    action.Invoke();
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }
    }
}