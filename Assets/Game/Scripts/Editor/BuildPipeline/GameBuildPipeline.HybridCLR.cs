using System.Collections.Generic;
using System.IO;
using DEngine.Editor;
using Game.Editor.ResourceTools;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        private const string EnableHybridCLRDefineSymbol = "ENABLE_HybridCLR";

        public static void SaveHybridCLR()
        {
            HybridCLRSettings.Instance.hotUpdateAssemblies = GameSetting.Instance.HotUpdateAssemblies;
            HybridCLRSettings.Instance.preserveHotUpdateAssemblies = GameSetting.Instance.PreserveAssemblies;
            HybridCLRSettings.Save();
            Debug.Log("Save HybridCLR success");
        }

        public static void CompileHotfixDll()
        {
            BuildTarget buildTarget = GetBuildTarget(GameSetting.Instance.BuildPlatform);
            CompileDllCommand.CompileDll(buildTarget);
            CopyDllAssets(buildTarget);
        }

        private static void CopyDllAssets(BuildTarget buildTarget)
        {
            if (string.IsNullOrEmpty(GameSetting.Instance.HotupdateAssembliesPath))
            {
                Debug.LogError("Directory path is null.");
                return;
            }

            if (Directory.Exists(GameSetting.Instance.HotupdateAssembliesPath))
            {
                IOUtility.Delete(GameSetting.Instance.HotupdateAssembliesPath);
            }
            else
            {
                Directory.CreateDirectory(GameSetting.Instance.HotupdateAssembliesPath);
            }

            if (Directory.Exists(GameSetting.Instance.PreserveAssembliesPath))
            {
                IOUtility.Delete(GameSetting.Instance.PreserveAssembliesPath);
            }
            else
            {
                Directory.CreateDirectory(GameSetting.Instance.PreserveAssembliesPath);
            }

            if (Directory.Exists(GameSetting.Instance.AOTAssembliesPath))
            {
                IOUtility.Delete(GameSetting.Instance.AOTAssembliesPath);
            }
            else
            {
                Directory.CreateDirectory(GameSetting.Instance.AOTAssembliesPath);
            }


            string desFileName;
            string oriFileName;

            List<string> assembliesVersion = new List<string>();
            //Copy HotUpdateAssemblies
            foreach (string hotUpdateAssemblyFullName in GameSetting.Instance.HotUpdateAssemblies)
            {
                oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), hotUpdateAssemblyFullName + ".dll");
                //加bytes 后缀让Unity识别为TextAsset 文件
                desFileName = Path.Combine(GameSetting.Instance.HotupdateAssembliesPath, hotUpdateAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
                assembliesVersion.Add(hotUpdateAssemblyFullName);
            }

            if (assembliesVersion.Count > 0)
            {
                string hotUpdateAssembliesVersion = Path.Combine(GameSetting.Instance.HotupdateAssembliesPath, Constant.AssetVersion.HotUpdateAssembliesVersion + ".bytes");
                GameAssetVersionUitlity.CreatAssetVersion(assembliesVersion.ToArray(), hotUpdateAssembliesVersion);
                Debug.Log($"Copy {buildTarget} HotUpdateAssemblies success.");
            }
            assembliesVersion.Clear();

            //Copy PreserveAssemblies
            foreach (string preserveAssemblyFullName in GameSetting.Instance.PreserveAssemblies)
            {
                oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), preserveAssemblyFullName + ".dll");
                //加bytes 后缀让Unity识别为TextAsset 文件
                desFileName = Path.Combine(GameSetting.Instance.PreserveAssembliesPath, preserveAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
                assembliesVersion.Add(preserveAssemblyFullName);
            }
            if (assembliesVersion.Count > 0)
            {
                string preserveAssembliesVersion = Path.Combine(GameSetting.Instance.PreserveAssembliesPath, Constant.AssetVersion.PreserveAssembliesVersion + ".bytes");
                GameAssetVersionUitlity.CreatAssetVersion(assembliesVersion.ToArray(), preserveAssembliesVersion);
                Debug.Log($"Copy {buildTarget} PreserveAssemblies success.");
            }
            assembliesVersion.Clear();

            // Copy AOTAssemblies
            string aotDllPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            foreach (var aotAssemblyFullName in GameSetting.Instance.AOTAssemblies)
            {
                oriFileName = Path.Combine(aotDllPath, aotAssemblyFullName + ".dll");
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOTAssemblies.");
                    continue;
                }
                desFileName = Path.Combine(GameSetting.Instance.AOTAssembliesPath, aotAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
                assembliesVersion.Add(aotAssemblyFullName);
            }

            if (assembliesVersion.Count > 0)
            {
                string aotAssembliesVersion = Path.Combine(GameSetting.Instance.AOTAssembliesPath, Constant.AssetVersion.AOTMetadataVersion + ".bytes");
                GameAssetVersionUitlity.CreatAssetVersion(assembliesVersion.ToArray(), aotAssembliesVersion);
                Debug.Log($"Copy {buildTarget} AOTAssemblies success.");
            }
            AssetDatabase.Refresh();
        }

        public static void EnableHybridCLR()
        {
            DisableHybridCLR();
            ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableHybridCLRDefineSymbol);
            SettingsUtil.Enable = true;
        } 

        public static void DisableHybridCLR()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableHybridCLRDefineSymbol);
            SettingsUtil.Enable = false;
        }
    }
}
