using System.Collections.Generic;
using DEngine.Editor.ResourceTools;
using UnityEngine;

namespace Game.Editor.ResourceTools
{
    public class AssetBundleCollector : ScriptableObject
    {
        public List<AssetCollector> Collector = new();
    }

    [System.Serializable]
    public class AssetCollector
    {
        public bool valid = true;
        public string name = string.Empty;
        public string fileSystem = string.Empty;
        public string groups = string.Empty;
        public string assetPath = string.Empty;
        public LoadType loadType = LoadType.LoadFromFile;
        public bool packed = true;
        public FilterType filterType = FilterType.Root;
        public string searchPatterns = "*.*";
    }

    public enum FilterType
    {
        /// <summary>
        /// 将指定文件夹打成一个Bundle
        /// </summary>
        Root,
        /// <summary>
        /// 指定文件夹下的文件分别打成一个Bundle,会过滤该文件下的子文件夹
        /// </summary>
        Children,
        /// <summary>
        /// 指定文件夹下的子文件夹分别打成一个Bundle
        /// </summary>
        ChildrenFoldersOnly,
        /// <summary>
        /// 指定文件夹下的子文件夹的文件分别打成一个Bundle
        /// </summary>
        ChildrenFilesOnly,
    }
}