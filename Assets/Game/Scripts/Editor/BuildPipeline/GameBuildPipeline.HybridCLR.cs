using System.Collections.Generic;
using System.IO;
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

            List<string> assembliesMainfest = new List<string>();
            //Copy HotUpdateAssemblies
            foreach (string hotUpdateAssemblyFullName in GameSetting.Instance.HotUpdateAssemblies)
            {
                oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), hotUpdateAssemblyFullName + ".dll");
                //加bytes 后缀让Unity识别为TextAsset 文件
                desFileName = Path.Combine(GameSetting.Instance.HotupdateAssembliesPath, hotUpdateAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
                assembliesMainfest.Add(hotUpdateAssemblyFullName);
            }

            if (assembliesMainfest.Count > 0)
            {
                string hotUpdateAssembliesMainfest = Path.Combine(GameSetting.Instance.HotupdateAssembliesPath, "HotUpdateAssembliesMainfest" + ".bytes");
                GameMainfestUitlity.CreatMainfest(assembliesMainfest.ToArray(), hotUpdateAssembliesMainfest);
                Debug.Log("Copy HotUpdateAssemblies success.");
            }
            assembliesMainfest.Clear();

            //Copy PreserveAssemblies
            foreach (string preserveAssemblyFullName in GameSetting.Instance.PreserveAssemblies)
            {
                oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), preserveAssemblyFullName + ".dll");
                //加bytes 后缀让Unity识别为TextAsset 文件
                desFileName = Path.Combine(GameSetting.Instance.PreserveAssembliesPath, preserveAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
                assembliesMainfest.Add(preserveAssemblyFullName);
            }
            if (assembliesMainfest.Count > 0)
            {
                string preserveAssembliesMainfest = Path.Combine(GameSetting.Instance.PreserveAssembliesPath, "PreserveAssembliesMainfest" + ".bytes");
                GameMainfestUitlity.CreatMainfest(assembliesMainfest.ToArray(), preserveAssembliesMainfest);
                Debug.Log("Copy PreserveAssemblies success.");
            }
            assembliesMainfest.Clear();

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
                assembliesMainfest.Add(aotAssemblyFullName);
            }

            if (assembliesMainfest.Count > 0)
            {
                string aotAssembliesMainfest = Path.Combine(GameSetting.Instance.AOTAssembliesPath, "AOTMetadataMainfest" + ".bytes");
                GameMainfestUitlity.CreatMainfest(assembliesMainfest.ToArray(), aotAssembliesMainfest);
                Debug.Log("Copy AOTAssemblies success.");
            }
            AssetDatabase.Refresh();
        }
    }
}
