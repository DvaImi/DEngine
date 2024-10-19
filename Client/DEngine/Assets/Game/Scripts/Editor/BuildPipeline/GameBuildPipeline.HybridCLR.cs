using System.IO;
using DEngine.Editor;
using Game.Editor.Toolbar;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        private const string EnableHybridCLRDefineSymbol = "ENABLE_HYBRIDCLR";

        [EditorToolbarMenu("AOT Generic", 1, 0)]
        public static void GenerateStripedAOT()
        {
            StripAOTDllCommand.GenerateStripedAOTDlls();
            AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
        }

        [EditorToolbarMenu("Compile", 1, 1)]
        public static void CompileHotfixDll()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(buildTarget);
            CopyDllAssets(buildTarget);
        }

        public static void SaveHybridCLR()
        {
            HybridCLRSettings.Instance.hotUpdateAssemblies = DEngineSetting.Instance.UpdateAssemblies;
            HybridCLRSettings.Instance.preserveHotUpdateAssemblies = DEngineSetting.Instance.PreserveAssemblies;
            HybridCLRSettings.Save();
        }

        private static void CopyDllAssets()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            CopyDllAssets(buildTarget);
        }

        public static void CopyDllAssets(BuildTarget buildTarget)
        {
            if (string.IsNullOrEmpty(DEngineSetting.Instance.UpdateAssembliesPath))
            {
                Debug.LogError("Directory path is null.");
                return;
            }

            if (Directory.Exists(DEngineSetting.Instance.UpdateAssembliesPath))
            {
                GameUtility.IO.Delete(DEngineSetting.Instance.UpdateAssembliesPath);
            }
            else
            {
                Directory.CreateDirectory(DEngineSetting.Instance.UpdateAssembliesPath);
            }

            if (Directory.Exists(DEngineSetting.Instance.PreserveAssembliesPath))
            {
                GameUtility.IO.Delete(DEngineSetting.Instance.PreserveAssembliesPath);
            }
            else
            {
                Directory.CreateDirectory(DEngineSetting.Instance.PreserveAssembliesPath);
            }

            if (Directory.Exists(DEngineSetting.Instance.AOTAssembliesPath))
            {
                GameUtility.IO.Delete(DEngineSetting.Instance.AOTAssembliesPath);
            }
            else
            {
                Directory.CreateDirectory(DEngineSetting.Instance.AOTAssembliesPath);
            }


            string desFileName;
            string oriFileName;

            //Copy HotUpdateAssemblies
            foreach (var hotUpdateAssemblyFullName in DEngineSetting.Instance.UpdateAssemblies)
            {
                oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), hotUpdateAssemblyFullName + ".dll");
                //加bytes 后缀让Unity识别为TextAsset 文件
                desFileName = Path.Combine(DEngineSetting.Instance.UpdateAssembliesPath, hotUpdateAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
            }


            //Copy PreserveAssemblies
            foreach (string preserveAssemblyFullName in DEngineSetting.Instance.PreserveAssemblies)
            {
                oriFileName = Path.Combine(SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget), preserveAssemblyFullName + ".dll");
                //加bytes 后缀让Unity识别为TextAsset 文件
                desFileName = Path.Combine(DEngineSetting.Instance.PreserveAssembliesPath, preserveAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
            }


            // Copy AOTAssemblies
            string aotDllPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            foreach (var aotAssemblyFullName in DEngineSetting.Instance.AOTAssemblies)
            {
                oriFileName = Path.Combine(aotDllPath, aotAssemblyFullName + ".dll");
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOTAssemblies.");
                    continue;
                }

                desFileName = Path.Combine(DEngineSetting.Instance.AOTAssembliesPath, aotAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
            }

            AssetDatabase.Refresh();
        }

        public static void CheckEnableHybridCLR()
        {
            if (SettingsUtil.Enable)
            {
                if (ScriptingDefineSymbols.HasScriptingDefineSymbol(EnableHybridCLRDefineSymbol))
                {
                    return;
                }

                EnableHybridCLR();
            }
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