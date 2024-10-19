using System;
using System.IO;
using System.Xml;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using Game.Editor.Toolbar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        public static bool BuildPlayer()
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

            var isSuccess = BuildPlayer(Platform.Windows);

            if (isSuccess)
            {
                isSuccess = BuildPlayer(Platform.Windows64);
            }

            if (isSuccess)
            {
                isSuccess = BuildPlayer(Platform.MacOS);
            }

            if (isSuccess)
            {
                isSuccess = BuildPlayer(Platform.Linux);
            }

            if (isSuccess)
            {
                isSuccess = BuildPlayer(Platform.IOS);
            }

            if (isSuccess)
            {
                isSuccess = BuildPlayer(Platform.Android);
            }

            if (isSuccess)
            {
                isSuccess = BuildPlayer(Platform.WindowsStore);
            }

            if (isSuccess)
            {
                isSuccess = BuildPlayer(Platform.WebGL);
            }

            if (isSuccess)
            {
                Debug.Log($"Build {DEngineSetting.Instance.BuildPlatforms} complete. ");
            }

            return isSuccess;
        }

        private static bool BuildPlayer(Platform platform)
        {
            if (!IsPlatformSelected(platform))
            {
                return true;
            }

            if (DEngineSetting.Instance.ResourceMode is ResourceMode.Updatable or ResourceMode.UpdatableWhilePlaying)
            {
                BuiltinData builtinData = EditorTools.LoadScriptableObject<BuiltinData>();
                string platformPath = GetPlatformPath(platform);
                string versionFilePath = Path.Combine(DEngineSetting.BundlesOutput, platformPath + "CheckVersion.json");
                if (File.Exists(versionFilePath))
                {
                    JObject checkVersionInfo = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(versionFilePath));
                    builtinData.BuildInfo = new BuildInfo()
                    {
                        LatestGameVersion = DEngineSetting.Instance.LatestGameVersion,
                        CheckVersionUrl = checkVersionInfo["CheckVersionUrl"]!.Value<string>()
                    };
                    EditorUtility.SetDirty(builtinData);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }


            BuildReport report = BuildApplication(platform);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + GameUtility.String.GetByteLengthString((long)summary.totalSize));
                return true;
            }
            else
            {
                Debug.Log("Build failed ");
                return false;
            }
        }

        private static BuildReport BuildApplication(Platform platform)
        {
            BuildTarget target = GetBuildTarget(platform);
            string outputExtension = GetFileExtensionForPlatform(target);
            if (!Directory.Exists(DEngineSetting.AppOutput))
            {
                GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.AppOutput);
            }

            string locationPath = Path.Combine(DEngineSetting.AppOutput, platform.ToString());
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