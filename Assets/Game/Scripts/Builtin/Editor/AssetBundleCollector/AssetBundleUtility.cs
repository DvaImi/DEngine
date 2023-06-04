// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-05-30 20:25:44
// 版 本：1.0
// ========================================================
using System;
using GameFramework;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;
using System.Collections.Generic;

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
            HybridCLRBuilderController builderController = new HybridCLRBuilderController();
            Platform platform = (Platform)Enum.Parse(typeof(Platform), builderController.PlatformNames[EditorPrefs.GetInt("BuildPlatform")]);
            AssetBundleCollector ruleEditor = ScriptableObject.CreateInstance<AssetBundleCollector>();
            ruleEditor.RefreshResourceCollection();
            bool enableAddress = ruleEditor.EnableAddress();
            if (enableAddress)
            {
                ResourceBuildHelper.AnalyzeAddress();
            }
            ResourceBuildHelper.StartBuild(platform);
            if (enableAddress)
            {
                GameAddressSerializer serializer = new GameAddressSerializer();
                serializer.RegisterSerializeCallback(0, GameAddressSerializerCallback.Serializer);
                Dictionary<string, string> address = new Dictionary<string, string>();

                ResourceCollection collection = new ResourceCollection();

                if (collection.Load())
                {
                    foreach (var asset in collection.GetAssets())
                    {
                        address.Add(Path.GetFileNameWithoutExtension(asset.Name), asset.Name);
                    }
                }

                using (FileStream fileStream = new FileStream(AssetUtility.AddressPath, FileMode.Create, FileAccess.Write))
                {
                    if (!serializer.Serialize(fileStream, address))
                    {
                        throw new GameFrameworkException("Serialize read-only version list failure.");
                    }
                }

                AssetDatabase.Refresh();
            }
        }
    }
}