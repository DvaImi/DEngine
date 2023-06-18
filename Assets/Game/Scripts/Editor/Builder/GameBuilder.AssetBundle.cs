using System;
using System.IO;
using DEngine;
using DEngine.Editor.ResourceTools;
using Game.Editor.ResourceTools;
using UnityEditor;
using UnityEngine;

namespace Game.Editor.Builder
{
    /// <summary>
    /// 资源生成器。
    /// </summary>
    public static partial class GameBuilder
    {
        public static string GetUpdatePrefixUri(Platform platform)
        {
            return Utility.Text.Format(GameSetting.Instance.UpdatePrefixUri, GameSetting.Instance.LatestGameVersion, GameSetting.Instance.InternalResourceVersion, GetPlatformPath(platform));
        }

        public static void BuildBundle()
        {
            IOUtility.CreateDirectoryIfNotExists(GameSetting.Instance.BundlesOutput);
            IOUtility.CreateDirectoryIfNotExists(Application.streamingAssetsPath);
            Platform platform = (Platform)Enum.Parse(typeof(Platform), PlatformNames[GameSetting.Instance.BuildPlatform]);
            BuildBundle(platform, GameSetting.Instance.BundlesOutput);
            if (GameSetting.Instance.ForceUpdateGame)
            {
                Debug.Log($"<color=#1E90FF>[Dvim] ►</color> " + "强制更新资源版本构建完成,打包新版本app时，务必更新版本号，避免冲突!!!");
            }
        }

        public static void ClearBundles()
        {
            IOUtility.Delete(GameSetting.Instance.BundlesOutput);
            ResourceBuilderController controller = new ResourceBuilderController();
            if (controller.Load())
            {
                controller.InternalResourceVersion = 0;
                controller.Save();
                GameSetting.Instance.InternalResourceVersion = 0;
                GameSetting.Instance.SaveSetting();
            }

            if (Directory.Exists(GameSetting.Instance.VirtualServerAddress))
            {
                IOUtility.Delete(GameSetting.Instance.VirtualServerAddress);
            }
            Debug.Log("Clear success");
        }

        public static void BuildBundle(Platform platform, string outputDirectory)
        {
            ResourceBuilderController builderController = new();
            builderController.OnLoadingResource += OnLoadingResource;
            builderController.OnLoadingAsset += OnLoadingAsset;
            builderController.OnLoadCompleted += OnLoadCompleted;
            builderController.OnAnalyzingAsset += OnAnalyzingAsset;
            builderController.OnAnalyzeCompleted += OnAnalyzeCompleted;
            builderController.ProcessingAssetBundle += OnProcessingAssetBundle;
            builderController.ProcessingBinary += OnProcessingBinary;
            builderController.ProcessResourceComplete += OnProcessResourceComplete;
            builderController.BuildResourceError += OnBuildResourceError;

            if (builderController.Load())
            {
                m_OriginalPlatform = builderController.Platforms;
                if (platform != Platform.Undefined)
                {
                    builderController.Platforms = platform;
                }
                if (builderController.OutputDirectory != outputDirectory)
                {
                    builderController.OutputDirectory = outputDirectory;
                }
                Debug.Log("Load configuration success.");

                builderController.RefreshCompressionHelper();
                builderController.BuildEventHandlerTypeName = typeof(GameBuildEventHandler).FullName;
                builderController.RefreshBuildEventHandler();
            }
            else
            {
                Debug.LogWarning("Load configuration failure.");
            }

            string buildMessage = string.Empty;
            MessageType buildMessageType = MessageType.None;
            GetBuildMessage(builderController, out buildMessage, out buildMessageType);
            switch (buildMessageType)
            {
                case MessageType.None:
                case MessageType.Info:
                    Debug.Log(buildMessage);
                    BuildResources(builderController);
                    break;
                case MessageType.Warning:
                    Debug.LogWarning(buildMessage);
                    BuildResources(builderController);
                    break;
                case MessageType.Error:
                    Debug.LogError(buildMessage);
                    break;
            }
        }

        public static void RefreshResourceCollection()
        {
            AssetBundleCollectorWindow ruleEditor = ScriptableObject.CreateInstance<AssetBundleCollectorWindow>();
            ruleEditor.RefreshResourceCollection();
        }

        public static void RefreshResourceCollection(string configPath)
        {
            AssetBundleCollectorWindow ruleEditor = ScriptableObject.CreateInstance<AssetBundleCollectorWindow>();
            ruleEditor.RefreshResourceCollection(configPath);
        }

        public static void SaveOutputDirectory(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                throw new DEngineException($"OutputDirectory: {outputDirectory}  is invalid.");
            }
            ResourceBuilderController controller = new();
            if (controller.Load())
            {
                controller.OutputDirectory = outputDirectory;
                controller.Save();
            }
        }

        private static void GetBuildMessage(ResourceBuilderController builderController, out string message, out MessageType messageType)
        {
            message = string.Empty;
            messageType = MessageType.Error;
            if (builderController.Platforms == Platform.Undefined)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    message += Environment.NewLine;
                }

                message += $"Platform {builderController.Platforms} is invalid.";
            }

            if (string.IsNullOrEmpty(builderController.CompressionHelperTypeName))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    message += Environment.NewLine;
                }

                message += "Compression helper is invalid.";
            }

            if (!builderController.IsValidOutputDirectory)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    message += Environment.NewLine;
                }

                message += $"Output directory {builderController.OutputDirectory} is invalid.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                return;
            }

            messageType = MessageType.Info;
            if (Directory.Exists(builderController.OutputPackagePath))
            {
                message += Utility.Text.Format("{0} will be overwritten.", builderController.OutputPackagePath);
                messageType = MessageType.Warning;
            }

            if (Directory.Exists(builderController.OutputFullPath))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += Utility.Text.Format("{0} will be overwritten.", builderController.OutputFullPath);
                messageType = MessageType.Warning;
            }

            if (Directory.Exists(builderController.OutputPackedPath))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += Utility.Text.Format("{0} will be overwritten.", builderController.OutputPackedPath);
                messageType = MessageType.Warning;
            }

            if (messageType == MessageType.Warning)
            {
                return;
            }

            message = "Ready to build.";
        }

        private static void BuildResources(ResourceBuilderController builderController)
        {
            if (builderController.BuildResources())
            {
                Debug.Log("Build resources success.");
                builderController.Platforms = m_OriginalPlatform;
                if (builderController.Save())
                {
                    Debug.Log("Save configuration success.");
                }
                else
                {
                    Debug.LogWarning("Save configuration failure.");
                }
            }
            else
            {
                Debug.LogError($"Build resources failure. <a href=\"file:///{Utility.Path.GetRegularPath(Path.Combine(builderController.BuildReportPath, "BuildLog.txt"))}\" line=\"0\">[ Open BuildLog.txt ]</a>");
            }
        }

        public static void CopyFileToStreamingAssets(string sourcePath)
        {
            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(streamingAssetsPath, fileName[sourcePath.Length..]));
                FileInfo destFileInfo = new(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(fileName, destFileName);
            }
        }

        private static void OnLoadingResource(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Loading Resources", Utility.Text.Format("Loading resources, {0}/{1} loaded.", index, count), (float)index / count);
        }

        private static void OnLoadingAsset(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Loading Assets", Utility.Text.Format("Loading assets, {0}/{1} loaded.", index, count), (float)index / count);
        }

        private static void OnLoadCompleted()
        {
            EditorUtility.ClearProgressBar();
        }

        private static void OnAnalyzingAsset(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Analyzing Assets", Utility.Text.Format("Analyzing assets, {0}/{1} analyzed.", index, count), (float)index / count);
        }

        private static void OnAnalyzeCompleted()
        {
            EditorUtility.ClearProgressBar();
        }

        private static bool OnProcessingAssetBundle(string assetBundleName, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Processing AssetBundle", Utility.Text.Format("Processing '{0}'...", assetBundleName), progress))
            {
                EditorUtility.ClearProgressBar();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool OnProcessingBinary(string binaryName, float progress)
        {
            if (EditorUtility.DisplayCancelableProgressBar("Processing Binary", Utility.Text.Format("Processing '{0}'...", binaryName), progress))
            {
                EditorUtility.ClearProgressBar();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void OnProcessResourceComplete(Platform platform)
        {
            EditorUtility.ClearProgressBar();
            Debug.Log(Utility.Text.Format("Build resources {0} complete.", platform));
        }

        private static void OnBuildResourceError(string errorMessage)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogWarning(Utility.Text.Format("Build resources error with error message '{0}'.", errorMessage));
        }
    }
}

