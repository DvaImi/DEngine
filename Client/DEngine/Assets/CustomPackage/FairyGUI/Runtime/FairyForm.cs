using System;
using System.Collections.Generic;

namespace Game.FairyGUI.Runtime
{
    [Serializable]
    public class FairyForm
    {
        /// <summary>
        /// 获取界面包裹名
        /// </summary>
        public string packageName;

        /// <summary>
        /// 获取界面编号。
        /// </summary>
        public int id;

        /// <summary>
        /// 获取界面描述资源名称。
        /// </summary>
        public string assetName;

        /// <summary>
        /// 获取或者设置预制体资源
        /// </summary>
        public string objectAssetName;

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string uiGroupName;

#if UNITY_EDITOR
        /// <summary>
        /// 分组索引
        /// </summary>
        [NonSerialized] public int GroupIndex;
#endif

        /// <summary>
        /// 获取是否允许多个界面实例。
        /// </summary>
        public bool allowMultiInstance;

        /// <summary>
        /// 获取是否暂停被其覆盖的界面。
        /// </summary>
        public bool pauseCoveredUIForm;

        /// <summary>
        /// 获取本包依赖的包体
        /// </summary>
        public List<string> dependencyPackages = new();

        /// <summary>
        /// 获取本包依赖的资源
        /// </summary>
        public List<string> dependencyAssets = new();
    }
}