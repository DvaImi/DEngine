using System;
using System.IO;
using System.Xml;
using DEngine.Editor.ResourceTools;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        [EditorToolbarMenu("BuildPlayer", 1, 10)]
        public static void BuildPlayer()
        {
            SaveBuildInfo();
            BuildTarget target = GetBuildTarget(GameSetting.Instance.BuildPlatform);
            if (target != EditorUserBuildSettings.activeBuildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(target), target);
            }

            BuildReport report = BuildApplication(target);
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

        public static void BuildPlayerV2(string channel, Platform platform)
        {
            BuildTarget target = GetBuildTarget(platform);
            if (target != EditorUserBuildSettings.activeBuildTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(UnityEditor.BuildPipeline.GetBuildTargetGroup(target), target);
            }

            BuildReport report = BuildApplicationV2(target, channel);
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

        public static void SaveBuildInfo()
        {
            BuiltinData builtinData = EditorTools.LoadScriptableObject<BuiltinData>();
            builtinData.BuildInfo = GameSetting.Instance.BuildInfo;
            EditorUtility.SetDirty(builtinData);
            AssetDatabase.SaveAssets();
        }

        public static BuildReport BuildApplication(BuildTarget platform)
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
                scenes = GameSetting.Instance.DefaultSceneNames,
                locationPathName = Path.Combine(locationPath, Application.productName + outputExtension),
                target = platform,
                options = BuildOptions.CompressWithLz4 | BuildOptions.ShowBuiltPlayer
            };

            return UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        public static BuildReport BuildApplicationV2(BuildTarget platform, string channel)
        {
            string outputExtension = GetFileExtensionForPlatform(platform);
            if (!Directory.Exists(GameSetting.Instance.AppOutput))
            {
                GameUtility.IO.CreateDirectoryIfNotExists(GameSetting.Instance.AppOutput);
                GameSetting.Instance.SaveSetting();
            }

            string locationPath = Path.Combine(GameSetting.Instance.AppOutput, GetPlatformPath(platform), channel);
            GameUtility.IO.CreateDirectoryIfNotExists(locationPath);
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = GameSetting.Instance.DefaultSceneNames,
                locationPathName = Path.Combine(locationPath, Application.productName + outputExtension),
                target = platform,
                options = BuildOptions.CompressWithLz4
            };

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
            return Path.Combine(GameSetting.Instance.AppOutput, PlatformNames[GameSetting.Instance.BuildPlatform], Application.productName + GetFileExtensionForPlatform(GetBuildTarget((int)s_OriginalPlatform)));
        }

        public static void SaveBuildSetting()
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();

                XmlElement xmlRoot = xmlDocument.CreateElement("DEngine");
                xmlDocument.AppendChild(xmlRoot);

                XmlElement xmlBuildSettings = xmlDocument.CreateElement("BuildSettings");
                xmlRoot.AppendChild(xmlBuildSettings);

                XmlElement xmlDefaultScenes = xmlDocument.CreateElement("DefaultScenes");
                xmlBuildSettings.AppendChild(xmlDefaultScenes);

                foreach (string sceneName in GameSetting.Instance.DefaultSceneNames)
                {
                    XmlElement xmlScene = xmlDocument.CreateElement("DefaultScene");
                    xmlScene.SetAttribute("Name", sceneName);
                    xmlDefaultScenes.AppendChild(xmlScene);
                }

                XmlElement xmlSearchScenePaths = xmlDocument.CreateElement("SearchScenePaths");
                xmlBuildSettings.AppendChild(xmlSearchScenePaths);

                // 添加 SearchScenePath 节点
                foreach (string path in GameSetting.Instance.SearchScenePaths)
                {
                    XmlElement xmlPath = xmlDocument.CreateElement("SearchScenePath");
                    xmlPath.SetAttribute("Path", path);
                    xmlSearchScenePaths.AppendChild(xmlPath);
                }

                // 保存 XML 文件到指定路径
                xmlDocument.Save(GameSetting.Instance.BuildSettingsConfig);
                AssetDatabase.Refresh();
                Debug.Log("XML file generated and saved successfully at: " + GameSetting.Instance.BuildSettingsConfig);
            }
            catch (Exception e)
            {
                Debug.LogError("Error generating XML file: " + e.Message);
            }
        }
    }
}