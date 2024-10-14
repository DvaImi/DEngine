using System;
using System.IO;
using System.Linq;
using DEngine;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using DEngine.Runtime;
using Game.Editor.ResourceTools;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Game.Editor.BuildPipeline
{
    /// <summary>
    /// 资源生成器。
    /// </summary>
    public static partial class GameBuildPipeline
    {
        [EditorToolbarMenu("Build Resource", 1, 3)]
        public static void BuildResource()
        {
            BuildResource(DEngineSetting.BundlesOutput);
        }

        public static void BuildResource(string output, bool difference = false)
        {
            OnPreprocess();
            BuildResource(DEngineSetting.Instance.BuildPlatform, output, difference);
            OnPostprocess();
        }

        public static void BuildResource(Platform platform, string outputDirectory, bool forceRebuild = false, bool difference = false)
        {
            ResourceBuilderController builderController = new();
            if (builderController.Load())
            {
                builderController.Platforms = platform;
                builderController.OutputDirectory = outputDirectory;
                builderController.CompressionHelperTypeName = typeof(DefaultCompressionHelper).FullName;
                builderController.RefreshCompressionHelper();
                builderController.BuildEventHandlerTypeName = typeof(GameBuildEventHandler).FullName;
                builderController.RefreshBuildEventHandler();
                builderController.AdditionalCompressionSelected = true;
                builderController.Difference = difference;
                builderController.ForceRebuildAssetBundleSelected = forceRebuild;
                builderController.OutputPackageSelected = DEngineSetting.Instance.ResourceMode == ResourceMode.Package;
                builderController.OutputFullSelected = DEngineSetting.Instance.ResourceMode == ResourceMode.Updatable || DEngineSetting.Instance.ResourceMode == ResourceMode.UpdatableWhilePlaying;
                builderController.OutputPackedSelected = DEngineSetting.Instance.ResourceMode == ResourceMode.Updatable || DEngineSetting.Instance.ResourceMode == ResourceMode.UpdatableWhilePlaying;
                builderController.OnLoadingResource += OnLoadingResource;
                builderController.OnLoadingAsset += OnLoadingAsset;
                builderController.OnLoadCompleted += OnLoadCompleted;
                builderController.OnAnalyzingAsset += OnAnalyzingAsset;
                builderController.OnAnalyzeCompleted += OnAnalyzeCompleted;
                builderController.ProcessingAssetBundle += OnProcessingAssetBundle;
                builderController.ProcessingBinary += OnProcessingBinary;
                builderController.ProcessResourceComplete += OnProcessResourceComplete;
                builderController.BuildResourceError += OnBuildResourceError;
                builderController.ProcessDifferenceComplete += OnPostprocessDifference;
                GetBuildMessage(builderController, out var buildMessage, out var buildMessageType);
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

            builderController.Save();
        }

        public static void ClearResource()
        {
            GameUtility.IO.Delete(DEngineSetting.BundlesOutput);
            ResourceBuilderController controller = new ResourceBuilderController();
            if (controller.Load())
            {
                controller.InternalResourceVersion = DEngineSetting.Instance.InternalResourceVersion = 0;
                controller.Save();
                DEngineSetting.Save();
            }

            if (EditorUtility.DisplayDialog("Clear", "Clear StreamingAssetsPath ?", "Clear", "Cancel"))
            {
                GameUtility.IO.Delete(Application.streamingAssetsPath);
            }

            AssetDatabase.Refresh();
            Debug.Log("Clear success");
        }

        public static void SaveResource()
        {
            ResourceBuilderController builderController = new();
            if (builderController.Load())
            {
                builderController.Platforms = DEngineSetting.Instance.BuildPlatform;
                builderController.OutputDirectory = DEngineSetting.BundlesOutput;
                builderController.CompressionHelperTypeName = typeof(DefaultCompressionHelper).FullName;
                builderController.BuildEventHandlerTypeName = typeof(GameBuildEventHandler).FullName;
                builderController.AdditionalCompressionSelected = true;
                builderController.Difference = DEngineSetting.Instance.Difference;
                builderController.ForceRebuildAssetBundleSelected = DEngineSetting.Instance.ForceRebuildAssetBundle;
            }

            builderController.Save();
        }

        public static void RefreshPackages()
        {
            PackagesNames = ResourcePackagesCollector.Instance.PackagesCollector.Select(x => x.PackageName).ToArray();
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

        public static void RemoveUnknownAssets()
        {
            ResourceEditorController resourceEditorController = new ResourceEditorController();
            if (resourceEditorController.Load())
            {
                int unknownAssetCount = resourceEditorController.RemoveUnknownAssets();
                int unusedResourceCount = resourceEditorController.RemoveUnusedResources();
                Debug.Log(Utility.Text.Format("Clean complete, {0} unknown assets and {1} unused resources has been removed.", unknownAssetCount, unusedResourceCount));
            }
        }

        public static int CleanUnknownAssets()
        {
            var unknownAssetCount = ResourcePackagesCollector.GetResourceGroupsCollector().Groups.Sum(resourceGroup => resourceGroup.AssetCollectors.RemoveAll(o => !IsValidAssetPath(o.AssetPath)));
            if (unknownAssetCount > 0)
            {
                Debug.Log(Utility.Text.Format("Clean complete, {0} unknown assets  has been removed.", unknownAssetCount));
            }

            return unknownAssetCount;
        }

        private static bool IsValidAssetPath(string assetPath)
        {
            return AssetDatabase.IsValidFolder(assetPath) ? Directory.Exists(assetPath) : File.Exists(assetPath);
        }

        private static void OnPreprocess()
        {
            if (DEngineSetting.Instance.ResourceMode == DEngine.Resource.ResourceMode.Unspecified)
            {
                DEngineSetting.Instance.ResourceMode = DEngine.Resource.ResourceMode.Package;
            }

            CleanUnknownAssets();
            ResourceCollectorEditorUtility.RefreshResourceCollection();
            DEngineSetting.Save();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.BundlesOutput);
            GameUtility.IO.CreateDirectoryIfNotExists(Application.streamingAssetsPath);
        }

        private static void OnPostprocess()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
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
                if (builderController.Save())
                {
                    Debug.Log("Save configuration success.");
                }
                else
                {
                    Debug.LogWarning("Save configuration failure.");
                }

                if (DEngineSetting.Instance.ForceUpdateGame)
                {
                    Debug.Log($"<color=#1E90FF>[DEngine] ►</color> " + "强制更新资源版本构建完成,打包新版本app时，务必更新版本号，避免冲突!!!");
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

                File.Copy(fileName, destFileName, true);
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