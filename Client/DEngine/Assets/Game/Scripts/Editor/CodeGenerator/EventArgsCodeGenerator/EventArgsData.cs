using System;

namespace Game.Editor
{
    /// <summary>
    /// 事件参数数据
    /// </summary>
    [Serializable]
    internal class EventArgsData
    {
        public string Type;
        public string Name;
        public EventArgType TypeEnum;

        public EventArgsData()
        {
        }

        public EventArgsData(string type, string name)
        {
            Type = type;
            Name = name;
        }
    }
}