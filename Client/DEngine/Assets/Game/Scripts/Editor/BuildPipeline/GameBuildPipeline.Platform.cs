using DEngine;
using DEngine.Editor.ResourceTools;
using UnityEditor;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static Platform GetCurrentPlatform()
        {
            return GetPlatform(EditorUserBuildSettings.activeBuildTarget);
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
            return platform switch
            {
                Platform.Windows => BuildTarget.StandaloneWindows,
                Platform.Windows64 => BuildTarget.StandaloneWindows64,
                Platform.MacOS => BuildTarget.StandaloneOSX,
                Platform.Linux => BuildTarget.StandaloneLinux64,
                Platform.IOS => BuildTarget.iOS,
                Platform.Android => BuildTarget.Android,
                Platform.WindowsStore => BuildTarget.WSAPlayer,
                Platform.WebGL => BuildTarget.WebGL,
                Platform.Undefined => throw new DEngineException("Platform is invalid."),
                _ => throw new DEngineException("Platform is invalid.")
            };
        }

        public static string GetCheckVersionUrlFormat()
        {
            return Utility.Text.Format("{0}:{1}/{{Platform}}Version.json", DEngineSetting.Instance.HostURL, DEngineSetting.Instance.HostingServicePort);
        }

        public static string GetCheckVersionUrl(Platform platform)
        {
            return Utility.Text.Format("{0}:{1}/{2}Version.json", DEngineSetting.Instance.HostURL, DEngineSetting.Instance.HostingServicePort, GetPlatformPath(platform));
        }

        public static string GetUpdatePrefixUriFormat()
        {
            return Utility.Text.Format("{0}:{1}/{2}.{3}/{{Platform}}", DEngineSetting.Instance.HostURL, DEngineSetting.Instance.HostingServicePort, DEngineSetting.Instance.LatestGameVersion, DEngineSetting.Instance.InternalResourceVersion);
        }

        public static string GetUpdatePrefixUri(Platform platform)
        {
            return Utility.Text.Format("{0}:{1}/{2}.{3}/{4}", DEngineSetting.Instance.HostURL, DEngineSetting.Instance.HostingServicePort, DEngineSetting.Instance.LatestGameVersion, DEngineSetting.Instance.InternalResourceVersion, GetPlatformPath(platform));
        }

        public static string GetPlatformPath(Platform platform)
        {
            return platform switch
            {
                Platform.Windows => "Windows",
                Platform.Windows64 => "Windows64",
                Platform.MacOS => "MacOS",
                Platform.IOS => "IOS",
                Platform.Android => "Android",
                Platform.WindowsStore => "WSA",
                Platform.WebGL => "WebGL",
                Platform.Linux => "Linux",
                _ => ""
            };
        }

        public static string GetPlatformPath(BuildTarget target)
        {
            return GetPlatformPath(GetPlatform(target));
        }
    }
}