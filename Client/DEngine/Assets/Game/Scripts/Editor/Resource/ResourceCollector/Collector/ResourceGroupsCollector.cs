using System;
using System.Collections.Generic;

namespace Game.Editor.ResourceTools
{
    [Serializable]
    public class ResourceGroupsCollector
    {
        public string PackageName = "Default";
        public string Description;
        public List<ResourceGroupCollector> Groups = new();
    }
}