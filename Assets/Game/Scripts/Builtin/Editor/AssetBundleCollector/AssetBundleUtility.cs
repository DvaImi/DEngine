// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-05-30 20:25:44
// 版 本：1.0
// ========================================================
using System;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor.ResourceTools
{
    public static class AssetBundleUtility
    {
        public static void RefreshResourceCollection()
        {
            AssetBundleCollector ruleEditor = ScriptableObject.CreateInstance<AssetBundleCollector>();
            ruleEditor.RefreshResourceCollection();
        }

        public static void RefreshResourceCollection(string configPath)
        {
            AssetBundleCollector ruleEditor = ScriptableObject.CreateInstance<AssetBundleCollector>();
            ruleEditor.RefreshResourceCollection(configPath);
        }

        public static void StartBuild()
        {
            Platform platform = (Platform)EditorPrefs.GetInt("BuildPlatform");
            AssetBundleCollector ruleEditor = ScriptableObject.CreateInstance<AssetBundleCollector>();
            ruleEditor.RefreshResourceCollection();
            if (ruleEditor.EnableAddress())
            {
                ResourceBuildHelper.AnalyzeAddress();
            }
            ResourceBuildHelper.StartBuild(platform);
        }
    }
}