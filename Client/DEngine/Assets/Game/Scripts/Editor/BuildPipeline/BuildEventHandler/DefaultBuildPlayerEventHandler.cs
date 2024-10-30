using System;
using DEngine.Editor.ResourceTools;
using Game.Editor.DataTableTools;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public class DefaultBuildPlayerEventHandler : IBuildPlayerEventHandler
    {
        public bool ContinueOnFailure { get; } = true;

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, Platform platforms, string outputDirectory)
        {
            GeneratorDataTableCommonLine.GenerateAll();
            GameBuildPipeline.SaveHybridCLR();
            GameBuildPipeline.SaveBuildSetting();
            DEngineSetting.Save();
            Fantasy.LinkXmlGenerator.GenerateLinkXml();
            AssetDatabase.Refresh();
        }

        public void OnPreprocessPlatform(Platform platform)
        {
            BuildTarget target = GameBuildPipeline.GetBuildTarget(platform);
            if (target != EditorUserBuildSettings.activeBuildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(target), target);
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
            }

            AssetDatabase.Refresh();
            //TODO 根据平台判断是否需要ALL
            try
            {
#if ENABLE_HYBRIDCLR
                Debug.Log("====================编译代码========================");
                PrebuildCommand.GenerateAll();
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

        public void OnPostprocessPlatform(Platform platform, bool isSuccess)
        {
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, Platform platforms, string outputDirectory)
        {
        }
    }
}