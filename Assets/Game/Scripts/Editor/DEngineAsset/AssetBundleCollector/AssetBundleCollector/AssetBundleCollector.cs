using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    [Serializable]
    public class AssetBundleCollector
    {
        public string PackageName = "Default";
        public string Description;
        public List<AssetBundleGroupCollector> Groups = new();
    }
}