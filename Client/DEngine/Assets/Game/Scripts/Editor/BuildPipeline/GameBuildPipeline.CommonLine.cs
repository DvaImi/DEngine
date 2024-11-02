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
        [MenuItem("Game/Build Pipeline/Automated Build", false, 100)]
        [EditorToolbarMenu("一键打包", 1, 100)]
        private static void AutomatedBuild()
        {
            if (DEngineSetting.Instance.ResourceMode == ResourceMode.Unspecified)
            {
                Debug.LogWarning("ResourceMode is Unspecified");
                return;
            }

            EditorTools.CloseAllCustomEditorWindows();
            IBuildPlayerEventHandler eventHandler = GetBuildPlayerEventHandler();
            bool watchResult = eventHandler is not { ContinueOnFailure: true };
            Debug.Log("开始一键打包任务");
            if (eventHandler != null)
            {
                Debug.Log("Execute build event handler 'OnPreprocessAllPlatforms'...");
                eventHandler.OnPreprocessAllPlatforms(PlayerSettings.productName, PlayerSettings.companyName, PlayerSettings.applicationIdentifier, Application.unityVersion, Application.version, DEngineSetting.Instance.BuildPlatforms, DEngineSetting.AppOutput);
            }

            var isSuccess = InternalAutomatedBuild(Platform.Windows, eventHandler);

            if (!watchResult && isSuccess)
            {
                isSuccess = InternalAutomatedBuild(Platform.Windows64, eventHandler);
            }

            if (!watchResult && isSuccess)
            {
                isSuccess = InternalAutomatedBuild(Platform.MacOS, eventHandler);
            }

            if (!watchResult && isSuccess)
            {
                isSuccess = InternalAutomatedBuild(Platform.Linux, eventHandler);
            }

            if (!watchResult && isSuccess)
            {
                isSuccess = InternalAutomatedBuild(Platform.IOS, eventHandler);
            }

            if (!watchResult && isSuccess)
            {
                isSuccess = InternalAutomatedBuild(Platform.Android, eventHandler);
            }

            if (!watchResult && isSuccess)
            {
                isSuccess = InternalAutomatedBuild(Platform.WindowsStore, eventHandler);
            }

            if (!watchResult && isSuccess)
            {
                isSuccess = InternalAutomatedBuild(Platform.WebGL, eventHandler);
            }

            if (eventHandler != null)
            {
                Debug.Log("Execute build event handler 'OnPostprocessAllPlatforms'...");
                eventHandler.OnPostprocessAllPlatforms(PlayerSettings.productName, PlayerSettings.companyName, PlayerSettings.applicationIdentifier, Application.unityVersion, Application.version, DEngineSetting.Instance.BuildPlatforms, DEngineSetting.AppOutput);
            }

            if (isSuccess)
            {
                Debug.Log($"Build {DEngineSetting.Instance.BuildPlatforms} complete. ");
            }
        }

        private static bool InternalAutomatedBuild(Platform platform, IBuildPlayerEventHandler eventHandler)
        {
            if (!IsPlatformSelected(platform))
            {
                return true;
            }

            Debug.LogFormat("Execute build event handler 'OnPreprocessPlatform for '{0}''...", platform.ToString());
            eventHandler?.OnPreprocessPlatform(platform);
            Debug.LogFormat("====================打包{0}资源========================", platform.ToString());
            var isSuccess = BuildResource(platform);
            Debug.LogFormat("====================打包{0}资源结束========================", platform.ToString());
            if (eventHandler != null && !isSuccess)
            {
                Debug.Log("Execute build event handler 'OnPostprocessPlatform'...");
                eventHandler.OnPostprocessPlatform(platform, false);
                return false;
            }

            Debug.LogFormat("====================打包{0}工程========================", platform.ToString());
            isSuccess = BuildPlayer(platform);
            Debug.LogFormat("====================打包{0}工程结束========================", platform.ToString());

            if (eventHandler != null)
            {
                Debug.Log("Execute build event handler 'OnPostprocessPlatform'...");
                eventHandler.OnPostprocessPlatform(platform, isSuccess);
                return isSuccess;
            }

            return true;
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