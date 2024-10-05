using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FairyGUI.Runtime
{
    public class FairyGUIFormRuntimeData : ScriptableObject
    {
        public List<FairyGroup> FairyGroups = new()
        {
            new FairyGroup("Default", 0)
        };

        public List<FairyForm> FairyForms = new();

        public FairyGroup GetFairyGroup(int uiFormId)
        {
            var form = GetFairyForm(uiFormId);
            return form == null ? null : FairyGroups.Find(o => o.Name == form.UIGroupName);
        }

        
        public FairyGroup GetFairyGroup(string packageName)
        {
            var form = GetFairyForm(packageName);
            return form == null ? null : FairyGroups.Find(o => o.Name == form.UIGroupName);
        }

        public FairyForm GetFairyForm(int uiFormId)
        {
            return FairyForms.Find(o => o.Id == uiFormId);
        }

        public FairyForm GetFairyForm(string packageName)
        {
            return FairyForms.Find(o => o.PackageName == packageName);
        }
    }

    [Serializable]
    public class FairyGroup
    {
        public string Name;
        public int Depth;

        public FairyGroup(string name, int depth)
        {
            Name = name;
            Depth = depth;
        }
    }

    [Serializable]
    public class FairyForm
    {
        /// <summary>
        /// 获取界面包裹名
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 获取界面编号。
        /// </summary>
        public int Id;

        /// <summary>
        /// 获取界面描述资源名称。
        /// </summary>
        public string AssetName;
        
        /// <summary>
        /// 获取或者设置预制体资源
        /// </summary>
        public string ObjectAssetName;

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string UIGroupName;

#if UNITY_EDITOR
        [NonSerialized]
        /// <summary>
        /// 分组索引
        /// </summary>
        public int GroupIndex;
#endif

        /// <summary>
        /// 获取是否允许多个界面实例。
        /// </summary>
        public bool AllowMultiInstance;

        /// <summary>
        /// 获取是否暂停被其覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm;

        /// <summary>
        /// 获取本包依赖的包体
        /// </summary>
        public List<string> DependencyPackages = new();

        /// <summary>
        /// 获取本包依赖的资源
        /// </summary>
        public List<string> DependencyAssets = new();
    }
}