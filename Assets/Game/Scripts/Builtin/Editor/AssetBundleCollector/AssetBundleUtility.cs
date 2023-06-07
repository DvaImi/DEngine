// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-05-30 20:25:44
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GameFramework;
using HybridCLR.Editor;
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
            IOUtility.CreateDirectoryIfNotExists(Application.streamingAssetsPath);
            AssetDatabase.Refresh();
            RefreshResourceCollection();
            HybridCLRBuilderController builderController = new HybridCLRBuilderController();
            Platform platform = (Platform)Enum.Parse(typeof(Platform), builderController.PlatformNames[EditorPrefs.GetInt("BuildPlatform")]);
            ResourceBuildHelper.StartBuild(platform, OnComplete);
            AssetDatabase.Refresh();
        }

        private static void OnComplete()
        {
            if (ScriptableObject.CreateInstance<AssetBundleCollector>().EnableAddress())
            {
                WriteAddress();
            }
        }

        public static void WriteAddress()
        {
            ResourceBuildHelper.AnalyzeAddress(out Dictionary<string, Dictionary<Type, string>> addressInfo);
            GameAddressSerializer serializer = new();
            serializer.RegisterSerializeCallback(0, GameAddressSerializerCallback.Serializer);
            string address = "Assets/Game/Builtin/Address.bytes";
            using (FileStream fileStream = new FileStream(address, FileMode.Create, FileAccess.Write))
            {
                if (serializer.Serialize(fileStream, addressInfo))
                {
                    Debug.Log("Write address success");
                }
                else
                {
                    throw new GameFrameworkException("Serialize read-only version list failure.");
                }
            }
        }

        public static void GenerateBuiltin()
        {
            string buildInfo = "Assets/Game/Builtin/BuildInfo.bytes";
            using (FileStream stream = new(buildInfo, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
                {
                    binaryWriter.Write(GameSetting.Instance.CheckVersionUrl);
                    binaryWriter.Write(GameSetting.Instance.WindowsAppUrl);
                    binaryWriter.Write(GameSetting.Instance.MacOSAppUrl);
                    binaryWriter.Write(GameSetting.Instance.IOSAppUrl);
                    binaryWriter.Write(GameSetting.Instance.AndroidAppUrl);
                    binaryWriter.Write(GameSetting.Instance.UpdatePrefixUri);
                }
            }

            List<string> dataTables = new List<string>();
            List<string> configs = new List<string>();

            string[] dataTableGuids = AssetDatabase.FindAssets("", new[] { DataTableSetting.Instance.DataTableFolderPath });
            string[] configGuids = AssetDatabase.FindAssets("", new[] { DataTableSetting.Instance.ConfigPath });

            foreach (string guid in dataTableGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.IsValidFolder(assetPath))
                {
                    dataTables.Add(Path.GetFileNameWithoutExtension(assetPath));
                }
            }

            foreach (string guid in configGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!AssetDatabase.IsValidFolder(assetPath))
                {
                    configs.Add(Path.GetFileNameWithoutExtension(assetPath));
                }
            }

            const string baseData = "Assets/Game/Builtin/BaseData.bytes";
            using (FileStream stream = new(baseData, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
                {
                    binaryWriter.Write(dataTables.Count);
                    foreach (var dataTable in dataTables)
                    {
                        binaryWriter.Write(dataTable);
                    }
                    binaryWriter.Write(configs.Count);
                    foreach (var config in configs)
                    {
                        binaryWriter.Write(config);
                    }
                }
            }

            const string dllInfo = "Assets/Game/Builtin/DllInfo.bytes";
            using (FileStream stream = new(dllInfo, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(stream, Encoding.UTF8))
                {
                    binaryWriter.Write(HybridCLRSettings.Instance.enable);
                    binaryWriter.Write(GameSetting.Instance.HotfixDllNameMain);
                    int aot = GameSetting.Instance.AOTDllNames.Length;
                    binaryWriter.Write(aot);
                    for (int i = 0; i < aot; i++)
                    {
                        binaryWriter.Write(GameSetting.Instance.AOTDllNames[i]);
                    }
                    int preserve = GameSetting.Instance.PreserveHotfixDllNames.Length;
                    binaryWriter.Write(preserve);
                    for (int i = 0; i < preserve; i++)
                    {
                        binaryWriter.Write(GameSetting.Instance.PreserveHotfixDllNames[i]);
                    }
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}