using System;
using System.IO;
using System.Xml;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
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
        [EditorToolbarMenu("BuildPlayer", ToolBarMenuAlign.Right, 10)]
        public static bool BuildPlayer()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Cannot build player because editor is compiling.");
                return false;
            }

            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.Refresh();
            var isSuccess = BuildPlayer(GetCurrentPlatform());
            if (isSuccess)
            {
                Debug.Log($"Build {GetCurrentPlatform()} complete. ");
            }

            return isSuccess;
        }

        private static bool BuildPlayer(Platform platform)
        {
            return BuildPlayer(platform, DEngineSetting.AppOutput);
        }

        private static bool BuildPlayer(Platform platform, string outputDirectory)
        {
            if (DEngineSetting.Instance.ResourceMode is ResourceMode.Updatable or ResourceMode.UpdatableWhilePlaying)
            {
                SaveBuiltinData(platform);
            }


            BuildReport report = BuildApplication(platform, outputDirectory);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + GameUtility.String.GetByteLengthString((long)summary.totalSize));
                DEngineSetting.Instance.InternalGameVersion += 1;
                DEngineSetting.Save();
                return true;
            }
            else
            {
                Debug.Log("Build failed ");
                return false;
            }
        }

        private static BuildReport BuildApplication(Platform platform, string outputDirectory)
        {
            BuildTarget target = GetBuildTarget(platform);
            string outputExtension = GetFileExtensionForPlatform(target);
            if (!Directory.Exists(outputDirectory))
            {
                GameUtility.IO.CreateDirectoryIfNotExists(outputDirectory);
            }

            string locationPath = Path.Combine(outputDirectory, Application.version, platform.ToString());
            GameUtility.IO.CreateDirectoryIfNotExists(locationPath);
            BuildPlayerOptions buildPlayerOptions = new()
            {
                scenes = DEngineSetting.Instance.DefaultSceneNames,
                locationPathName = Path.Combine(locationPath, Application.productName + outputExtension),
                target = target,
                options = BuildOptions.CompressWithLz4 | BuildOptions.ShowBuiltPlayer
            };

            return UnityEditor.BuildPipeline.BuildPlayer(buildPlayerOptions);
        }

        private static string GetFileExtensionForPlatform(BuildTarget platform)
        {
            return platform switch
            {
                BuildTarget.StandaloneWindows   => ".exe",
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.StandaloneOSX       => ".app",
                BuildTarget.StandaloneLinux64   => ".x86_64",
                BuildTarget.Android             => ".apk",
                BuildTarget.iOS                 => ".ipa",
                BuildTarget.WebGL               => "",
                _                               => ""
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

                xmlDocument.Save(DEngineSetting.Instance.BuildSettingsConfig);
                AssetDatabase.Refresh();
                Debug.Log("Saved successfully.");
            }
            catch (Exception e)
            {
                Debug.LogError("Error generating XML file: " + e.Message);
            }
        }

        public static void SaveBuiltinData()
        {
            SaveBuiltinData(GetCurrentPlatform());
        }

        private static void SaveBuiltinData(Platform platform)
        {
            var builtinData = EditorTools.LoadScriptableObject<BuiltinData>();
            builtinData.BuildInfo ??= new BuildInfo();
            builtinData.BuildInfo.LatestGameVersion = DEngineSetting.Instance.LatestGameVersion;
            builtinData.BuildInfo.CheckVersionUrl = GetCheckVersionUrl(platform);
            EditorTools.SaveAsset(builtinData);
        }
    }
}