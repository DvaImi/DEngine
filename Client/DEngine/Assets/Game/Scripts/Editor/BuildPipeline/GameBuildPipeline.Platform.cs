using DEngine;
using DEngine.Editor.ResourceTools;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        [EditorToolbarMenu(nameof(SwitchPlatform), 0, 300, true)]
        public static void SwitchPlatform()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Space(400);
                var hotfixPlatformIndex = (Platform)EditorGUILayout.EnumPopup(DEngineSetting.Instance.BuildPlatform, EditorStyles.toolbarPopup, GUILayout.Width(120));
                if (!hotfixPlatformIndex.Equals(DEngineSetting.Instance.BuildPlatform))
                {
                    DEngineSetting.Instance.BuildPlatform = hotfixPlatformIndex;
                    BuildTarget target = GetBuildTarget(DEngineSetting.Instance.BuildPlatform);
                    if (target != EditorUserBuildSettings.activeBuildTarget)
                    {
                        EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(target), target);
                        DEngineSetting.Save();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        public static void CheckPlatform()
        {
            BuildTarget buildTarget = GetBuildTarget(DEngineSetting.Instance.BuildPlatform);
            if (buildTarget != EditorUserBuildSettings.activeBuildTarget)
            {
                if (EditorUtility.DisplayDialog("", "当前平台与目标平台不符是否切换 ? ", "确定", "取消"))
                {
                    if (EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget))
                    {
                        CompilationPipeline.RequestScriptCompilation();
                    }
                }
            }
        }

        public static Platform GetPlatform(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneOSX => Platform.MacOS,
                BuildTarget.StandaloneWindows => Platform.Windows,
                BuildTarget.iOS => Platform.IOS,
                BuildTarget.Android => Platform.Android,
                BuildTarget.StandaloneWindows64 => Platform.Windows64,
                BuildTarget.WebGL => Platform.WebGL,
                BuildTarget.WSAPlayer => Platform.WindowsStore,
                BuildTarget.StandaloneLinux64 => Platform.Linux,
                _ => throw new DEngineException("Platform is invalid."),
            };
        }

        public static BuildTarget GetBuildTarget(Platform platform)
        {
            switch (platform)
            {
                case Platform.Windows:
                    return BuildTarget.StandaloneWindows;

                case Platform.Windows64:
                    return BuildTarget.StandaloneWindows64;

                case Platform.MacOS:
#if UNITY_2017_3_OR_NEWER
                    return BuildTarget.StandaloneOSX;
#else
                    return BuildTarget.StandaloneOSXUniversal;
#endif
                case Platform.Linux:
                    return BuildTarget.StandaloneLinux64;

                case Platform.IOS:
                    return BuildTarget.iOS;

                case Platform.Android:
                    return BuildTarget.Android;

                case Platform.WindowsStore:
                    return BuildTarget.WSAPlayer;

                case Platform.WebGL:
                    return BuildTarget.WebGL;

                case Platform.Undefined:
                default:
                    throw new DEngineException("Platform is invalid.");
            }
        }

        public static string GetUpdatePrefixUri(Platform platform)
        {
            return Utility.Text.Format(DEngineSetting.Instance.UpdatePrefixUri, DEngineSetting.Instance.LatestGameVersion, DEngineSetting.Instance.InternalResourceVersion, GetPlatformPath(platform));
        }

        public static string GetPlatformPath(Platform platform)
        {
            switch (platform)
            {
                case Platform.Windows:
                    return "Windows";

                case Platform.Windows64:
                    return "Windows64";

                case Platform.MacOS:
                    return "MacOS";

                case Platform.IOS:
                    return "IOS";

                case Platform.Android:
                    return "Android";

                case Platform.WindowsStore:
                    return "WSA";

                case Platform.WebGL:
                    return "WebGL";

                case Platform.Linux:
                    return "Linux";

                default:
                    throw new DEngineException("Platform is invalid.");
            }
        }

        public static string GetPlatformPath(BuildTarget target)
        {
            Platform platform = GetPlatform(target);
            switch (platform)
            {
                case Platform.Windows:
                    return "Windows";

                case Platform.Windows64:
                    return "Windows64";

                case Platform.MacOS:
                    return "MacOS";

                case Platform.IOS:
                    return "IOS";

                case Platform.Android:
                    return "Android";

                case Platform.WindowsStore:
                    return "WSA";

                case Platform.WebGL:
                    return "WebGL";

                case Platform.Linux:
                    return "Linux";

                default:
                    throw new DEngineException("Platform is invalid.");
            }
        }
    }
}