using System.IO;
using Game.Editor.ResourceTools;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static void CompileHotfixDll()
        {
            BuildTarget buildTarget = GameBuildPipeline.GetBuildTarget(GameSetting.Instance.BuildPlatform);
            CompileDllCommand.CompileDll(buildTarget);
            CopyDllAssets(buildTarget);
        }

        private static void CopyDllAssets(BuildTarget buildTarget)
        {
            if (string.IsNullOrEmpty(GameSetting.Instance.HotupdateDllPath))
            {
                Debug.LogError("Directory path is null.");
                return;
            }

            if (Directory.Exists(GameSetting.Instance.HotupdateDllPath))
            {
                IOUtility.Delete(GameSetting.Instance.HotupdateDllPath);
            }
            else
            {
                Directory.CreateDirectory(GameSetting.Instance.HotupdateDllPath);
            }

            if (Directory.Exists(GameSetting.Instance.AOtDllPath))
            {
                IOUtility.Delete(GameSetting.Instance.AOtDllPath);
            }
            else
            {
                Directory.CreateDirectory(GameSetting.Instance.AOtDllPath);
            }


            string hotUpdateAssemblyDefinitionFullName = GameSetting.Instance.HotUpdateAssemblyDefinition.name + ".dll";
            //Copy Hotfix Dll
            string oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), hotUpdateAssemblyDefinitionFullName);

            //加bytes 后缀让Unity识别为TextAsset 文件
            string desFileName = Path.Combine(GameSetting.Instance.HotupdateDllPath, hotUpdateAssemblyDefinitionFullName + ".bytes");
            File.Copy(oriFileName, desFileName, true);
            string updataDllMainfest = Path.Combine(GameSetting.Instance.HotupdateDllPath, "UpdataDllMainfest" + ".bytes");
            GameMainfestUitlity.CreatMainfest(hotUpdateAssemblyDefinitionFullName, updataDllMainfest);
            Debug.Log("Copy hotfix dll success.");

            // Copy AOT Dll
            string aotDllPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            foreach (var dllName in GameSetting.Instance.AOTDllNames)
            {
                oriFileName = Path.Combine(aotDllPath, dllName);
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOT dll.");
                    continue;
                }
                desFileName = Path.Combine(GameSetting.Instance.AOtDllPath, dllName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
            }

            Debug.Log("Copy Aot dll success.");

            string aotMainfest = Path.Combine(GameSetting.Instance.AOtDllPath, "AOTMetadataMainfest" + ".bytes");
            GameMainfestUitlity.CreatMainfest(GameSetting.Instance.AOTDllNames, aotMainfest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
