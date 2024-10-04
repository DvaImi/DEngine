using System;
using DEngine.Editor.ResourceTools;
using UnityEditor;

namespace Game.Editor.ResourceTools
{
    [Serializable]
    public class ResourceCollector
    {
        /// <summary>
        /// 资源名称
        /// </summary>
        public string Name = null;
        /// <summary>
        /// 是否启用资源
        /// </summary>
        public bool Enable = true;
        /// <summary>
        /// 资源文件系统
        /// </summary>
        public string FileSystem = null;
        /// <summary>
        /// 资源分组
        /// </summary>
        public string Groups = null;
        /// <summary>
        /// 资源变体
        /// </summary>
        public string Variant = null;
        /// <summary>
        /// 资源对象
        /// </summary>
        public UnityEngine.Object Asset = null;
        /// <summary>
        /// 资源加载方式
        /// </summary>
        public LoadType LoadType = LoadType.LoadFromFile;
        /// <summary>
        /// 是否是随包资源
        /// </summary>
        public bool Packed = true;
        /// <summary>
        /// 资源收集类型
        /// </summary>
        public string FilterRule = nameof(CollectAll);

        public string AssetPath
        {
            get
            {
                return Asset == null ? null : AssetDatabase.GetAssetPath(Asset);
            }
        }
    }
}
