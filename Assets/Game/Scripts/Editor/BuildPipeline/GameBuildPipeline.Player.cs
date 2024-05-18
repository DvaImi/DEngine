using System.IO;
using System.IO.Compression;
using DEngine.Editor.ResourceTools;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        public static void SaveBuildInfo()
        {
            BuiltinData builtinData = GameEditorUtility.GetScriptableObject<BuiltinData>();
            builtinData.BuildInfo = GameSetting.Instance.BuildInfo;
            EditorUtility.SetDirty(builtinData);
            AssetDatabase.SaveAssets();
            Debug.Log("Save builtinData success");
        }

        public static void BuildPlayer(bool aotGeneric)
        {
            SaveBuildInfo();
            BuildTarget target = GetBuildTarget(GameSetting.Instance.BuildPlatform);
            if (target != EditorUserBuildSettings.activeBuildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(target), target);
            }
            BuildReport report = BuildApplication(target, aotGeneric);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + GameUtility.String.GetByteLengthString((long)summary.totalSize));
            }
            else
            {
                Debug.Log("Build failed ");
            }
        }

        public static BuildReport BuildApplication(BuildTarget platform, bool aotGeneric)
        {
            string outputExtension = GetFileExtensionForPlatform(platform);
            if (!Directory.Exists(GameSetting.Instance.AppOutput))
            {
                GameUtility.IO.CreateDirectoryIfNotExists(GameSetting.Instance.AppOutput);
                GameSetting.Instance.SaveSetting();
            }

            string locationPath = Path.Combine(GameSetting.Instance.AppOutput, PlatformNames[GameSetting.Instance.BuildPlatform]);
            GameUtility.IO.CreateDirectoryIfNotExists(locationPath);
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = new string[] { EditorBuildSettings.scenes[0].path },
                locationPathName = Path.Combine(locationPath, Application.productName + outputExtension),
                target = platform,
                options = BuildOptions.CompressWithLz4 | BuildOptions.ShowBuiltPlayer
            };

            if (aotGeneric)
            {
                buildPlayerOptions.options = BuildOptions.CompressWithLz4;
            }
            return UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        public static string GetFileExtensionForPlatform(BuildTarget platform)
        {
            return platform switch
            {
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.StandaloneOSX => ".app",
                BuildTarget.Android => ".apk",
                BuildTarget.iOS => ".ipa",
                BuildTarget.WebGL => "",
                _ => ".exe",
            };
        }

        public static string GetBuildAppFullName()
        {
            return Path.Combine(GameSetting.Instance.AppOutput, PlatformNames[GameSetting.Instance.BuildPlatform], Application.productName + GetFileExtensionForPlatform(GetBuildTarget((int)m_OriginalPlatform)));
        }
    }
}