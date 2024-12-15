using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DEngine.Editor;
using Game.Editor.FileSystem;
using Game.Editor.Toolbar;
using HybridCLR.Editor;
using HybridCLR.Editor.Commands;
using HybridCLR.Editor.HotUpdate;
using HybridCLR.Editor.Settings;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        private const string EnableHybridCLRDefineSymbol = "ENABLE_HYBRIDCLR";

        [EditorToolbarMenu("AOT Generic", ToolBarMenuAlign.Right, 0)]
        public static void GenerateStripedAOT()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Cannot generate striped aot because editor is compiling.");
                return;
            }

            AssetDatabase.Refresh();
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            StripAOTDllCommand.GenerateStripedAOTDlls();
            AOTReferenceGeneratorCommand.CompileAndGenerateAOTGenericReference();
            CopyAOTDllAssets(buildTarget);

            var aot = FileSystemCollector.Instance.Get("aot");
            if (aot == null)
            {
                return;
            }

            ProcessFileSystem(aot);
        }

        [EditorToolbarMenu("Compile", ToolBarMenuAlign.Right, 1)]
        public static void CompileUpdateDll()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Cannot compile updated assemblies because editor is compiling.");
                return;
            }

            AssetDatabase.Refresh();
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            CompileDllCommand.CompileDll(buildTarget);
            CopyUpdateDllAssets(buildTarget);
            var patch = FileSystemCollector.Instance.Get("patch");
            if (patch == null)
            {
                return;
            }

            ProcessFileSystem(patch);
        }

        public static void SaveHybridCLR()
        {
            HybridCLRSettings.Instance.hotUpdateAssemblies         = DEngineSetting.Instance.UpdateAssemblies;
            HybridCLRSettings.Instance.preserveHotUpdateAssemblies = DEngineSetting.Instance.PreserveAssemblies;
            HybridCLRSettings.Save();
        }

        public static void CopyAOTDllAssets(BuildTarget buildTarget)
        {
            if (Directory.Exists(DEngineSetting.Instance.AOTAssembliesPath))
            {
                GameUtility.IO.Delete(DEngineSetting.Instance.AOTAssembliesPath);
            }
            else
            {
                Directory.CreateDirectory(DEngineSetting.Instance.AOTAssembliesPath);
            }

            if (Directory.Exists(DEngineSetting.Instance.CheckAccessMissingMetadataPath))
            {
                GameUtility.IO.Delete(DEngineSetting.Instance.CheckAccessMissingMetadataPath);
            }
            else
            {
                Directory.CreateDirectory(DEngineSetting.Instance.CheckAccessMissingMetadataPath);
            }

            // Copy AOTAssemblies
            string aotDllPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(buildTarget);
            foreach (var aotAssemblyFullName in DEngineSetting.Instance.AOTAssemblies)
            {
                var oriFileName = Path.Combine(aotDllPath, aotAssemblyFullName + ".dll");
                if (!File.Exists(oriFileName))
                {
                    Debug.LogError($"AOT 补充元数据 dll: {oriFileName} 文件不存在。需要构建一次主包后才能生成裁剪后的 AOTAssemblies.");
                    continue;
                }

                var desFileName = Path.Combine(DEngineSetting.Instance.CheckAccessMissingMetadataPath, aotAssemblyFullName + ".dll");
                File.Copy(oriFileName, desFileName, true);
                desFileName = Path.Combine(DEngineSetting.Instance.AOTAssembliesPath, aotAssemblyFullName + ".bytes");
                File.Copy(oriFileName, desFileName, true);
            }
        }

        public static void CopyUpdateDllAssets(BuildTarget buildTarget)
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

        /// <summary>
        /// 获取项目未生成的AOT程序集
        /// </summary>
        /// <returns></returns>
        public static string[] GetProjectMissAOTAssemblies()
        {
            string aotDllPath = SettingsUtil.GetAssembliesPostIl2CppStripDir(EditorUserBuildSettings.activeBuildTarget);
            return !Directory.Exists(aotDllPath) ? Array.Empty<string>() : (from aotAssemblyFullName in DEngineSetting.Instance.AOTAssemblies let oriFileName = Path.Combine(aotDllPath, aotAssemblyFullName + ".dll") where !File.Exists(oriFileName) select aotAssemblyFullName).ToArray();
        }

        /// <summary>
        /// TODO 
        /// </summary>
        /// <param name="buildTarget"></param>
        private static void CheckAccessMissingMetadata(BuildTarget buildTarget)
        {
            // aotDir指向 构建主包时生成的裁剪aot dll目录，而不是最新的SettingsUtil.GetAssembliesPostIl2CppStripDir(target)目录。
            // 一般来说，发布热更新包时，由于中间可能调用过generate/all，SettingsUtil.GetAssembliesPostIl2CppStripDir(target)目录中包含了最新的aot dll，
            // 肯定无法检查出类型或者函数裁剪的问题。
            // 需要在构建完主包后，将当时的aot dll保存下来，供后面补充元数据或者裁剪检查。
            string aotDir = DEngineSetting.Instance.CheckAccessMissingMetadataPath;
            if (!Directory.Exists(aotDir))
            {
                return;
            }

            // 第2个参数excludeDllNames为要排除的aot dll。一般取空列表即可。对于旗舰版本用户，
            // excludeDllNames需要为dhe程序集列表，因为dhe 程序集会进行热更新，热更新代码中
            // 引用的dhe程序集中的类型或函数肯定存在。
            var checker = new MissingMetadataChecker(aotDir, new List<string>());

            string hotUpdateDir = SettingsUtil.GetHotUpdateDllsOutputDirByTarget(buildTarget);
            foreach (var dll in SettingsUtil.HotUpdateAssemblyFilesExcludePreserved)
            {
                string dllPath       = $"{hotUpdateDir}/{dll}";
                bool   notAnyMissing = checker.Check(dllPath);
                if (notAnyMissing)
                {
                    continue;
                }
                
            }
        }
    }
}