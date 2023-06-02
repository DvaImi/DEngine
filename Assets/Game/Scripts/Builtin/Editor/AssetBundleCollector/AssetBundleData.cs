using System.Collections.Generic;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor.ResourceTools
{
    public class AssetBundleData : ScriptableObject
    {
        /// <summary>
        /// 开启寻址加载
        /// </summary>
        public bool EnableAddress { get; set; }

        public List<ResourceRule> rules = new List<ResourceRule>();

    }

    [System.Serializable]
    public class ResourceRule
    {
        public bool valid = true;
        public string name = string.Empty;
        public string variant = null;
        public string fileSystem = string.Empty;
        public string groups = string.Empty;
        public string assetsDirectoryPath = string.Empty;
        public LoadType loadType = LoadType.LoadFromFile;
        public bool packed = true;
        public ResourceFilterType filterType = ResourceFilterType.Root;
        public string searchPatterns = "*.*";
    }

    public enum ResourceFilterType
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
    }
}