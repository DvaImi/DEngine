using System;
using DEngine;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        [EditorToolbarMenu("一键打包", ToolBarMenuAlign.Right, 100)]
        internal static void AutomatedBuild()
        {
            //等待编译器任务完成
            if (IsEditorBusy())
            {
                Debug.LogWarning("Waiting for editor...");
                return;
            }

            if (DEngineSetting.Instance.ResourceMode == ResourceMode.Unspecified)
            {
                Debug.LogWarning("ResourceMode is Unspecified");
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            EditorTools.CloseAllCustomEditorWindows();
            IBuildPlayerEventHandler eventHandler = GetBuildPlayerEventHandler();
            Platform                 platform     = GetCurrentPlatform();
            Debug.Log("开始一键打包任务");
            if (eventHandler != null)
            {
                Debug.LogFormat("Execute build event handler 'OnPreprocessPlatform for '{0}''...", platform.ToString());
                eventHandler.OnPreprocessPlatform(PlayerSettings.productName, PlayerSettings.companyName, PlayerSettings.applicationIdentifier, Application.unityVersion, Application.version, GetCurrentPlatform(), DEngineSetting.AppOutput);
            }

            Debug.LogFormat("====================打包{0}资源========================", platform.ToString());
            var isSuccess = BuildResource(platform);
            Debug.LogFormat("====================打包{0}资源结束========================", platform.ToString());
            if (eventHandler != null && !isSuccess)
            {
                Debug.Log("Execute build event handler 'OnPostprocessPlatform'...");
                eventHandler.OnPostprocessPlatform(PlayerSettings.productName, PlayerSettings.companyName, PlayerSettings.applicationIdentifier, Application.unityVersion, Application.version, platform, DEngineSetting.AppOutput, false);
                return;
            }

            Debug.LogFormat("====================打包{0}工程========================", platform.ToString());
            isSuccess = BuildPlayer(platform);
            Debug.LogFormat("====================打包{0}工程结束========================", platform.ToString());

            if (eventHandler != null)
            {
                Debug.Log("Execute build event handler 'OnPostprocessPlatform'...");
                eventHandler.OnPostprocessPlatform(PlayerSettings.productName, PlayerSettings.companyName, PlayerSettings.applicationIdentifier, Application.unityVersion, Application.version, platform, DEngineSetting.AppOutput, isSuccess);
                return;
            }

            if (isSuccess)
            {
                Debug.Log($"Build {platform} complete. ");
            }
        }

        private static IBuildPlayerEventHandler GetBuildPlayerEventHandler()
        {
            Type buildEventHandlerType = Utility.Assembly.GetType(DEngineSetting.Instance.BuildPlayerEventHandlerTypeName);
            if (buildEventHandlerType != null)
            {
                return (IBuildPlayerEventHandler)Activator.CreateInstance(buildEventHandlerType);
            }

            return null;
        }
    }
}