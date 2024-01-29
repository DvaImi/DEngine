using System;
using DEngine.Editor.ResourceTools;

namespace Game.Editor.ResourceTools
{
    [Serializable]
    public class AssetCollector
    {
        public bool Enable = true;
        public string Name = null;
        public string FileSystem = null;
        public string Groups = null;
        public string Variant = null;
        public string AssetPath = string.Empty;
        public LoadType LoadType = LoadType.LoadFromFile;
        public bool Packed = true;
        public string FilterRule = nameof(CollectAll);
    }
}
