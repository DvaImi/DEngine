using System;
using System.Collections.Generic;

namespace Game.Editor.ResourceTools
{
    [Serializable]
    public class ResourceGroupCollector
    {
        public string GroupName = "Default";
        public bool EnableGroup = true;
        public string Description;
        public List<ResourceCollector> AssetCollectors = new();

        internal void SetGroupName(string groupName)
        {
            this.GroupName = groupName;
            for (int i = 0; i < AssetCollectors.Count; i++)
            {
                AssetCollectors[i].Groups = groupName;
            }
        }
    }
}