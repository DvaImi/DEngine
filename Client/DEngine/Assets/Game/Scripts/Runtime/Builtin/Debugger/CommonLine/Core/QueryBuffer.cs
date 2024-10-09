using System;
using System.Collections.Generic;

namespace Game.CommandLine
{
    public class QueryBuffer
    {
        private readonly Dictionary<string, Command[]> m_Buffer = new();
        private readonly List<string> m_QueryHistory = new();
        private readonly int m_Capacity;
        public QueryBuffer(int capacity)
        {
            this.m_Capacity = Math.Max(1, capacity);
        }

        public void Cache(string query, Command[] result)
        {

            if (m_Buffer.ContainsKey(query)) return;
            if (m_QueryHistory.Count + 1 > m_Capacity)
            {
                m_Buffer.Remove(m_QueryHistory[0]);
                m_QueryHistory.RemoveAt(0);
            }
            m_Buffer.Add(query, result);
            m_QueryHistory.Add(query);
        }

        public bool GetCache(string query, out Command[] info)
        {
            return m_Buffer.TryGetValue(query, out info);
        }
    }
}
