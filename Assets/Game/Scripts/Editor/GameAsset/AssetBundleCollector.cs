using System.Collections.Generic;
using UnityEngine;
using DEngine.Editor.ResourceTools;

namespace Game.Editor.ResourceTools
{
    public class AssetBundleCollector : ScriptableObject
    {
        public List<AssetCollector> Collector = new List<AssetCollector>();
    }

    [System.Serializable]
    public class AssetCollector
    {
        public bool valid = true;
        public string name = string.Empty;
        public string variant = null;
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
        /// 指定文件夹
        /// </summary>
        Root,
        /// <summary>
        /// 指定文件夹下的文件分别打成一个Resource。
        /// </summary>
        Children,
        /// <summary>
        /// 指定文件夹下的子文件夹分别打成一个Resource
        /// </summary>
        ChildrenFoldersOnly,
        /// <summary>
        /// 指定文件夹下的子文件夹的文件分别打成一个Resource。
        /// </summary>
        ChildrenFilesOnly,
        /// <summary>
        /// 指定文件
        /// </summary>
        FileOnly,
    }
}