using System;
using System.IO;
using System.Xml;
using DEngine.Editor.ResourceTools;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Editor.BuildPipeline
{
    public static partial class GameBuildPipeline
    {
        [EditorToolbarMenu("BuildPlayer", 1, 10)]
        public static void BuildPlayer()
        {
            SaveBuildInfo();
            BuildTarget target = GetBuildTarget(DEngineSetting.Instance.BuildPlatform);
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
            builtinData.BuildInfo = DEngineSetting.Instance.BuildInfo;
            EditorUtility.SetDirty(builtinData);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }

        public static BuildReport BuildApplication(BuildTarget platform)
        {
            string outputExtension = GetFileExtensionForPlatform(platform);
            if (!Directory.Exists(DEngineSetting.AppOutput))
            {
                GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.AppOutput);
                DEngineSetting.Save();
            }

            string locationPath = Path.Combine(DEngineSetting.AppOutput, DEngineSetting.Instance.BuildPlatform.ToString());
            GameUtility.IO.CreateDirectoryIfNotExists(locationPath);
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = DEngineSetting.Instance.DefaultSceneNames,
                locationPathName = Path.Combine(locationPath, Application.productName + outputExtension),
                target = platform,
                options = BuildOptions.CompressWithLz4 | BuildOptions.ShowBuiltPlayer
            };

            return UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        public static BuildReport BuildApplicationV2(BuildTarget platform, string channel)
        {
            string outputExtension = GetFileExtensionForPlatform(platform);
            if (!Directory.Exists(DEngineSetting.AppOutput))
            {
                GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.AppOutput);
                DEngineSetting.Save();
            }

            string locationPath = Path.Combine(DEngineSetting.AppOutput, GetPlatformPath(platform), channel);
            GameUtility.IO.CreateDirectoryIfNotExists(locationPath);
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = DEngineSetting.Instance.DefaultSceneNames,
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

                foreach (string sceneName in DEngineSetting.Instance.DefaultSceneNames)
                {
                    XmlElement xmlScene = xmlDocument.CreateElement("DefaultScene");
                    xmlScene.SetAttribute("Name", sceneName);
                    xmlDefaultScenes.AppendChild(xmlScene);
                }

                XmlElement xmlSearchScenePaths = xmlDocument.CreateElement("SearchScenePaths");
                xmlBuildSettings.AppendChild(xmlSearchScenePaths);

                // 添加 SearchScenePath 节点
                foreach (string path in DEngineSetting.Instance.SearchScenePaths)
                {
                    XmlElement xmlPath = xmlDocument.CreateElement("SearchScenePath");
                    xmlPath.SetAttribute("Path", path);
                    xmlSearchScenePaths.AppendChild(xmlPath);
                }

                // 保存 XML 文件到指定路径
                xmlDocument.Save(DEngineSetting.Instance.BuildSettingsConfig);
                AssetDatabase.Refresh();
                Debug.Log("XML file generated and saved successfully at: " + DEngineSetting.Instance.BuildSettingsConfig);
            }
            catch (Exception e)
            {
                Debug.LogError("Error generating XML file: " + e.Message);
            }
        }
    }
}