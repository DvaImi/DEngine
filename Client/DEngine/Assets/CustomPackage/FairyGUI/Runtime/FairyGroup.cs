using System;

namespace Game.FairyGUI.Runtime
{
    [Serializable]
    public class FairyGroup
    {
        public string groupName;
        public int depth;

        public FairyGroup(string groupName, int depth)
        {
            this.groupName = groupName;
            this.depth = depth;
        }
    }
}