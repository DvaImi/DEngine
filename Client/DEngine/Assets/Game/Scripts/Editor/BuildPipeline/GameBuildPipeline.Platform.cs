using System.Reflection;
using DEngine;
using DEngine.Editor;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        #region GUI

        public static void GUIPlatform()
        {
            GUILayout.Space(5f);
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Platforms", EditorStyles.boldLabel);
                    EditorGUILayout.BeginHorizontal("box");
                    {
                        EditorGUILayout.BeginVertical();
                        {
                            DrawPlatform(Platform.Windows, "Windows");
                            DrawPlatform(Platform.Windows64, "Windows x64");
                            DrawPlatform(Platform.MacOS, "macOS");
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginVertical();
                        {
                            DrawPlatform(Platform.Linux, "Linux");
                            DrawPlatform(Platform.IOS, "iOS");
                            DrawPlatform(Platform.Android, "Android");
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.BeginVertical();
                        {
                            DrawPlatform(Platform.WindowsStore, "Windows Store");
                            DrawPlatform(Platform.WebGL, "WebGL");
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void DrawPlatform(Platform platform, string platformName)
        {
            SelectPlatform(platform, EditorGUILayout.ToggleLeft(platformName, IsPlatformSelected(platform)));
        }

        private static readonly string[] ResourceModeNames =
        {
            "EditorMode (编辑器模式)",
            "Package (单机模式)",
            "Updatable (预下载的可更新模式)",
            "UpdatableWhilePlaying (使用时下载的可更新模式)"
        };

        [EditorToolbarMenu("SwitchResourceMode", 0, 1000, true)]
        private static void SwitchResourceMode()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.Space(100);
                var playModeIndex = (int)DEngineSetting.Instance.ResourceMode;
                int selectedIndex = EditorGUILayout.Popup(playModeIndex, ResourceModeNames, GUILayout.Width(200));
                if (selectedIndex != playModeIndex)
                {
                    Debug.Log($"更改编辑器资源运行模式 : {ResourceModeNames[selectedIndex]}");
                    playModeIndex = selectedIndex;
                    var baseComponent = Object.FindFirstObjectByType<DEngine.Runtime.BaseComponent>();
                    var resourcesComponent = Object.FindFirstObjectByType<DEngine.Runtime.ResourceComponent>();
                    if (baseComponent)
                    {
                        baseComponent.EditorResourceMode = selectedIndex <= 0;
                        if (baseComponent.EditorResourceMode)
                        {
                            BuildSettings.AllScenes();
                        }

                        EditorTools.SaveAsset(baseComponent);
                    }

                    if (resourcesComponent)
                    {
                        if (selectedIndex > 0)
                        {
                            var fieldInfo = typeof(DEngine.Runtime.ResourceComponent).GetField("m_ResourceMode", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (fieldInfo != null)
                            {
                                fieldInfo.SetValue(resourcesComponent, (ResourceMode)playModeIndex);
                                EditorTools.SaveAsset(resourcesComponent);
                            }
                        }
                    }

                    DEngineSetting.Instance.ResourceMode = (ResourceMode)playModeIndex;
                    DEngineSetting.Save();
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        #endregion

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

        public static string GetCheckVersionUrl()
        {
            return Utility.Text.Format("{0}:{1}/{{Platform}}Version.json", DEngineSetting.Instance.HostURL, DEngineSetting.Instance.HostingServicePort);
        }

        public static string GetCheckVersionUrl(Platform platform)
        {
            return Utility.Text.Format("{0}:{1}/{2}Version.json", DEngineSetting.Instance.HostURL, DEngineSetting.Instance.HostingServicePort, GetPlatformPath(platform));
        }

        public static string GetUpdatePrefixUri()
        {
            return Utility.Text.Format("{0}:{1}/{2}.{3}/{{Platform}}", DEngineSetting.Instance.HostURL, DEngineSetting.Instance.HostingServicePort, DEngineSetting.Instance.LatestGameVersion, DEngineSetting.Instance.InternalResourceVersion);
        }

        public static string GetUpdatePrefixUri(Platform platform)
        {
            return Utility.Text.Format("{0}:{1}/{2}.{3}/{4}", DEngineSetting.Instance.HostURL, DEngineSetting.Instance.HostingServicePort, DEngineSetting.Instance.LatestGameVersion, DEngineSetting.Instance.InternalResourceVersion, GetPlatformPath(platform));
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
                    return "";
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
                    return "";
            }
        }
    }
}