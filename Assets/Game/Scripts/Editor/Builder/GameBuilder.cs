using System;
using System.Linq;
using DEngine;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using UnityEditor;

namespace Game.Editor.Builder
{
    public static partial class GameBuilder
    {
        private static Platform m_OriginalPlatform;
        public static string[] ResourceMode { get; }
        public static string[] PlatformNames { get; }

        static GameBuilder()
        {
            ResourceMode = Enum.GetNames(typeof(ResourceMode)).Skip(1).ToArray();
            PlatformNames = Enum.GetNames(typeof(Platform)).Skip(1).ToArray();
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

        /// <summary>
        /// 由 UnityGameFramework.Editor.ResourceTools.Platform 得到 平台标识符。
        /// </summary>
        /// <param name="platform">UnityGameFramework.Editor.ResourceTools.Platform。</param>
        /// <returns>平台标识符。</returns>
        public static string GetPlatformPath(Platform platform)
        {
            // 这里和 ProcedureVersionCheck.GetPlatformPath() 对应。
            // 使用 平台标识符 关联 UnityEngine.RuntimePlatform 和 UnityGameFramework.Editor.ResourceTools.Platform
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
