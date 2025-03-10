﻿using System;
using DEngine.Editor.ResourceTools;
using Game.Editor.DataTableTools;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public class DefaultBuildPlayerEventHandler : IBuildPlayerEventHandler
    {
        public void OnPreprocessPlatform(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, Platform platform, string outputDirectory)
        {
            DEngine.Editor.BuildSettings.DefaultScenes();
            GeneratorDataTableCommonLine.GenerateAll();
            Fantasy.LinkXmlGenerator.GenerateLinkXml();
            GameBuildPipeline.SaveHybridCLR();
            GameBuildPipeline.SaveBuildSetting();
            DEngineSetting.Save();
            PreprocessPlatform(platform);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        public void OnPostprocessPlatform(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, Platform platform, string outputDirectory, bool isSuccess)
        {
        }

        private static void PreprocessPlatform(Platform platform)
        {
            BuildTarget target = GameBuildPipeline.GetBuildTarget(platform);
            if (target != EditorUserBuildSettings.activeBuildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(target), target);
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            try
            {
#if ENABLE_HYBRIDCLR
                Debug.Log("====================编译代码========================");
                if (GameBuildPipeline.GetProjectMissAOTAssemblies().Length > 0)
                {
                    PrebuildCommand.GenerateAll();
                }
                else
                {
                    CompileDllCommand.CompileDll(target);
                }

                GameBuildPipeline.CopyAOTDllAssets(target);
                GameBuildPipeline.CopyUpdateDllAssets(target);
                Debug.Log("====================编译代码结束========================");
#endif

                Debug.Log("====================处理文件系统========================");
                GameBuildPipeline.ProcessFileSystem();
                Debug.Log("====================处理文件系统结束========================");
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}