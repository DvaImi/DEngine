﻿using System;
using System.IO;
using System.Linq;
using System.Reflection;
using DEngine;
using DEngine.Editor;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using DEngine.Runtime;
using Game.Editor.ResourceTools;
using Game.Editor.Toolbar;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Game.Editor.BuildPipeline
{
    /// <summary>
    /// 资源生成器。
    /// </summary>
    public static partial class GameBuildPipeline
    {
        private static readonly string[] ResourceModeNames =
        {
            "EditorMode (编辑器模式)",
            "Package (单机模式)",
            "Updatable (预下载的可更新模式)",
            "UpdatableWhilePlaying (使用时下载的可更新模式)"
        };

        [EditorToolbarMenu(nameof(SwitchResourceMode), ToolBarMenuAlign.Right, -1, true)]
        public static void SwitchResourceMode()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                var playModeIndex = (int)DEngineSetting.Instance.ResourceMode;
                int selectedIndex = EditorGUILayout.Popup(playModeIndex, ResourceModeNames, GUILayout.Width(200));
                if (selectedIndex != playModeIndex)
                {
                    Debug.Log($"更改编辑器资源运行模式 : {ResourceModeNames[selectedIndex]}");
                    playModeIndex = selectedIndex;
                    var baseComponent = Object.FindFirstObjectByType<BaseComponent>();
                    var resourcesComponent = Object.FindFirstObjectByType<ResourceComponent>();
                    if (baseComponent)
                    {
                        baseComponent.EditorResourceMode = selectedIndex <= 0;
                        if (baseComponent.EditorResourceMode)
                        {
                            BuildSettings.AllScenes();
                        }

                        EditorTools.SaveAsset(baseComponent);
                    }

                    if (resourcesComponent)
                    {
                        if (selectedIndex > 0)
                        {
                            var fieldInfo = typeof(ResourceComponent).GetField("m_ResourceMode", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (fieldInfo != null)
                            {
                                fieldInfo.SetValue(resourcesComponent, (ResourceMode)playModeIndex);
                                EditorTools.SaveAsset(resourcesComponent);
                            }
                        }
                    }

                    DEngineSetting.Instance.ResourceMode = (ResourceMode)playModeIndex;
                    DEngineSetting.Save();
                    EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        [EditorToolbarMenu("Build Resource", ToolBarMenuAlign.Right, 3)]
        public static bool BuildResource()
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Can't build resource because editor is compiling.");
                return false;
            }

            AssetDatabase.Refresh();
            bool isSuccess = BuildResource(GetCurrentPlatform(), DEngineSetting.Instance.ForceRebuildAssetBundle);
            if (isSuccess && DEngineSetting.Instance.EnableHostingService == false)
            {
                EditorUtility.RevealInFinder(DEngineSetting.BundlesOutput);
            }

            return isSuccess;
        }

        private static bool BuildResource(Platform platform, bool forceRebuild = false)
        {
            return BuildResource(platform, DEngineSetting.BundlesOutput, forceRebuild);
        }

        private static bool BuildResource(Platform platform, string outputDirectory, bool forceRebuild = false)
        {
            if (DEngineSetting.Instance.ResourceMode == ResourceMode.Unspecified)
            {
                Debug.LogWarning("ResourceMode is Unspecified");
                return false;
            }

            GameUtility.IO.CreateDirectoryIfNotExists(Application.streamingAssetsPath);
            GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.BundlesOutput);
            GameUtility.IO.CreateDirectoryIfNotExists(outputDirectory);

            var builderController = new ResourceBuilderController();
            if (builderController.Load() && BuildResource(builderController, platform, outputDirectory, forceRebuild))
            {
                if (builderController.Save())
                {
                    Debug.Log("Save configuration success.");
                }
                else
                {
                    Debug.LogWarning("Save configuration failure.");
                }

                return true;
            }

            return false;
        }

        private static bool BuildResource(ResourceBuilderController builderController, Platform platforms, string outputDirectory, bool forceRebuild = false)
        {
            if (builderController == null)
            {
                return false;
            }

            GameUtility.IO.CreateDirectoryIfNotExists(Application.streamingAssetsPath);
            GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.BundlesOutput);
            GameUtility.IO.CreateDirectoryIfNotExists(outputDirectory);

            bool isSuccess = false;
            builderController.Platforms = platforms;
            builderController.OutputDirectory = outputDirectory;
            builderController.CompressionHelperTypeName = typeof(DefaultCompressionHelper).FullName;
            builderController.RefreshCompressionHelper();
            builderController.BuildEventHandlerTypeName = typeof(GameResourceBuildEventHandler).FullName;
            builderController.RefreshBuildEventHandler();
            builderController.AdditionalCompressionSelected = true;
            builderController.ForceRebuildAssetBundleSelected = forceRebuild;
            builderController.OutputPackageSelected = DEngineSetting.Instance.ResourceMode == ResourceMode.Package;
            builderController.OutputFullSelected = true;
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
            GetBuildMessage(builderController, out var buildMessage, out var buildMessageType);

            switch (buildMessageType)
            {
                case MessageType.None:
                case MessageType.Info:
                    Debug.Log(buildMessage);
                    isSuccess = builderController.BuildResources();
                    break;
                case MessageType.Warning:
                    Debug.LogWarning(buildMessage);
                    isSuccess = builderController.BuildResources();
                    break;
                case MessageType.Error:
                    Debug.LogError(buildMessage);
                    break;
            }

            if (isSuccess)
            {
                Debug.Log("Build resources success.");
            }

            else
            {
                Debug.LogError($"Build resources failure. <a href=\"file:///{Utility.Path.GetRegularPath(Path.Combine(builderController.BuildReportPath, "BuildLog.txt"))}\" line=\"0\">[ Open BuildLog.txt ]</a>");
            }

            return isSuccess;
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

        public static void RefreshPackages()
        {
            PackagesNames = ResourcePackagesCollector.Instance.PackagesCollector.Select(x => x.PackageName).ToArray();
            VariantNames = VariantHelper.GetVariantArray();
        }

        public static string GetCurrentPackageName()
        {
            return PackagesNames[DEngineSetting.Instance.AssetBundleCollectorIndex];
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

        public static void CopyFileToStreamingAssets(string sourcePath)
        {
            GameUtility.IO.CreateDirectoryIfNotExists(Application.streamingAssetsPath);
            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(streamingAssetsPath, fileName[sourcePath.Length..]));
                FileInfo destFileInfo = new(destFileName);
                if (destFileInfo.Directory is { Exists: false })
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

        public static bool BuildResourcePack(Platform platform, string sourceVersion, out string outputPath, out string targetVersion)
        {
            outputPath = targetVersion = string.Empty;
            if (DEngineSetting.Instance.ResourceMode >= ResourceMode.Updatable && DEngineSetting.Instance.BuildResourcePack)
            {
                ResourcePackBuilderController controller = new();
                controller.OnBuildResourcePacksStarted += OnBuildResourcePacksStarted;
                controller.OnBuildResourcePacksCompleted += OnBuildResourcePacksCompleted;
                controller.OnBuildResourcePackSuccess += OnBuildResourcePackSuccess;
                controller.OnBuildResourcePackFailure += OnBuildResourcePackFailure;
                if (controller.Load())
                {
                    controller.Platform = platform;
                    var versionNames = controller.GetVersionNames();
                    if (versionNames.Length < 2)
                    {
                        Debug.LogWarning("No version was found in the specified working directory and platform.");
                        DEngineSetting.Instance.BuildResourcePack = false;
                        DEngineSetting.Save();
                        return false;
                    }

                    controller.BackupDiff = controller.BackupVersion = true;
                    controller.CompressionHelperTypeName = typeof(DefaultCompressionHelper).FullName;
                    controller.RefreshCompressionHelper();
                    outputPath = controller.OutputPath;
                    targetVersion = versionNames[^1];
                    return controller.BuildResourcePack(sourceVersion, targetVersion);
                }
            }

            return false;
        }

        private static void OnBuildResourcePacksStarted(int count)
        {
            Debug.Log(Utility.Text.Format("Build resource packs started, '{0}' items to be built.", count));
            EditorUtility.DisplayProgressBar("Build Resource Packs", Utility.Text.Format("Build resource packs, {0} items to be built.", count), 0f);
        }

        private static void OnBuildResourcePacksCompleted(int successCount, int count)
        {
            int failureCount = count - successCount;
            string str = Utility.Text.Format("Build resource packs completed, '{0}' items, '{1}' success, '{2}' failure.", count, successCount, failureCount);
            if (failureCount > 0)
            {
                Debug.LogWarning(str);
            }
            else
            {
                Debug.Log(str);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void OnBuildResourcePackSuccess(int index, int count, string sourceVersion, string targetVersion)
        {
            Debug.Log(Utility.Text.Format("Build resource packs success, source version '{0}', target version '{1}'.", GetVersionNameForDisplay(sourceVersion), GetVersionNameForDisplay(targetVersion)));
            EditorUtility.DisplayProgressBar("Build Resource Packs", Utility.Text.Format("Build resource packs, {0}/{1} completed.", index + 1, count), (float)index / count);
        }

        private static void OnBuildResourcePackFailure(int index, int count, string sourceVersion, string targetVersion)
        {
            Debug.LogWarning(Utility.Text.Format("Build resource packs failure, source version '{0}', target version '{1}'.", GetVersionNameForDisplay(sourceVersion), GetVersionNameForDisplay(targetVersion)));
            EditorUtility.DisplayProgressBar("Build Resource Packs", Utility.Text.Format("Build resource packs, {0}/{1} completed.", index + 1, count), (float)index / count);
        }

        private static string GetVersionNameForDisplay(string versionName)
        {
            if (string.IsNullOrEmpty(versionName))
            {
                return "<None>";
            }

            string[] splitVersionNames = versionName.Split('.');
            if (splitVersionNames.Length < 2)
            {
                return null;
            }

            string text = splitVersionNames[0];
            for (int i = 1; i < splitVersionNames.Length - 1; i++)
            {
                text += "." + splitVersionNames[i];
            }

            return Utility.Text.Format("{0} ({1})", text, splitVersionNames[^1]);
        }
    }
}