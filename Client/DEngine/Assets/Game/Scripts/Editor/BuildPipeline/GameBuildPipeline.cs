using System;
using System.Linq;
using DEngine;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using HybridCLR.Editor.Commands;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        private static Platform m_OriginalPlatform;
        public static string[] ResourceMode { get; private set; }
        public static string[] PlatformNames { get; private set; }
        public static string[] PackagesNames { get; private set; }

        static GameBuildPipeline()
        {
            ResourceMode = Enum.GetNames(typeof(ResourceMode)).Skip(1).ToArray();
            PlatformNames = Enum.GetNames(typeof(Platform)).Skip(1).ToArray();
            RefreshPackages();
        }

        [MenuItem("Game/Build Pipeline/ Build Current APP", false, 100)]
        private static void RunCurrentBuild()
        {
            Debug.Log("开始一键打包任务");
            CheckPlatform();
            {
                Debug.Log("====================目标平台切换成功========================");
                Debug.Log("====================保存配置文件========================");
                SaveHybridCLR();
                SaveResource();
                SaveBuildInfo();
                SaveBuildSetting();
                GameSetting.Save();
                Debug.Log("====================保存配置文件结束========================");

                Debug.Log("====================编译代码========================");
                PrebuildCommand.GenerateAll();
                CompileHotfixDll();
                Debug.Log("====================编译代码结束========================");

                Debug.Log("====================打包资源========================");
                BuildResource(GameSetting.Instance.BundlesOutput, false);
                Debug.Log("====================打包资源结束========================");

                Debug.Log("====================打包工程========================");
                BuildPlayer();
                Debug.Log("====================打包工程结束========================");
            }
        }

        public static void CheckPlatform()
        {
            BuildTarget buildTarget = GetBuildTarget(GameSetting.Instance.BuildPlatform);
            if (buildTarget != EditorUserBuildSettings.activeBuildTarget)
            {
                if (EditorUtility.DisplayDialog("", "当前平台与目标平台不符是否切换 ? ", "确定", "取消"))
                {
                    if (EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(buildTarget), buildTarget))
                    {
                        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                    }
                }
            }
        }

        public static BuildTarget GetBuildTarget(int platformIndex)
        {
            Platform platform = GetPlatform(platformIndex);
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

                default:
                    throw new DEngineException("Platform is invalid.");
            }
        }

        public static Platform GetPlatform(BuildTarget editorPlatform)
        {
            return editorPlatform switch
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

        public static Platform GetPlatform(int platformIndex)
        {
            return (Platform)Enum.Parse(typeof(Platform), PlatformNames[platformIndex]);
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
    }
}