// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-29 10:50:09
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using GameFramework;
using GameFramework.Resource;
using HybridCLR.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityGameFramework.Editor.ResourceTools;
using Object = UnityEngine.Object;

namespace Game.Editor.ResourceTools
{
    /// <summary>
    /// 资源生成器。
    /// </summary>
    public static class GameAssetBuilder
    {
        private static ResourceBuilderController m_Controller = null;
        private static Platform m_OriginalPlatform;
        private static GameFrameworkAction m_Complete;
        public static string[] ResourceMode { get; }
        public static string[] PlatformNames { get; }

        static GameAssetBuilder()
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
                    throw new GameFrameworkException("Platform is invalid.");
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
                _ => throw new GameFrameworkException("Platform is invalid."),
            };
        }

        public static Platform GetPlatform(int platformIndex)
        {
            return (Platform)Enum.Parse(typeof(Platform), PlatformNames[platformIndex]);
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

        public static void BuildBundle()
        {
            Platform platform = (Platform)Enum.Parse(typeof(Platform), PlatformNames[GameSetting.Instance.BuildPlatform]);
            StartBuild(platform, GameSetting.Instance.BundlesOutput);
        }

        public static void OnPreprocess()
        {
            IOUtility.CreateDirectoryIfNotExists(GameSetting.Instance.BundlesOutput);
            IOUtility.CreateDirectoryIfNotExists(Application.streamingAssetsPath);

            //清空StreamingAssets
            IOUtility.Delete(Application.streamingAssetsPath);

            RefreshResourceCollection();
            //绑定到内置对象
            BuiltinDataComponent builtinDataComponent = Object.FindObjectOfType<BuiltinDataComponent>();
            if (builtinDataComponent != null)
            {
                Type type = typeof(BuiltinDataComponent);
                FieldInfo buildInfo = type.GetField("m_BuildInfo", BindingFlags.NonPublic | BindingFlags.Instance);
                buildInfo?.SetValue(builtinDataComponent, GameSetting.Instance.BuildInfo);
                EditorUtility.SetDirty(builtinDataComponent);
                EditorSceneManager.SaveOpenScenes();
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        public static void ClearBundles()
        {
            IOUtility.Delete(GameSetting.Instance.BundlesOutput);
            ResourceBuilderController controller = new ResourceBuilderController();
            if (controller.Load())
            {
                controller.InternalResourceVersion = 0;
                controller.Save();
            }
            Debug.Log("Clear success");
        }

        public static void SaveOutputDirectory(string outputDirectory)
        {
            if (!Directory.Exists(outputDirectory))
            {
                throw new GameFrameworkException($"OutputDirectory: {outputDirectory}  is invalid.");
            }
            m_Controller = new ResourceBuilderController();
            m_Controller.OnLoadingResource += OnLoadingResource;
            m_Controller.OnLoadingAsset += OnLoadingAsset;
            m_Controller.OnLoadCompleted += OnLoadCompleted;
            m_Controller.OnAnalyzingAsset += OnAnalyzingAsset;
            m_Controller.OnAnalyzeCompleted += OnAnalyzeCompleted;
            m_Controller.ProcessingAssetBundle += OnProcessingAssetBundle;
            m_Controller.ProcessingBinary += OnProcessingBinary;
            m_Controller.ProcessResourceComplete += OnProcessResourceComplete;
            m_Controller.BuildResourceError += OnBuildResourceError;
            if (m_Controller.Load())
            {
                m_Controller.OutputDirectory = outputDirectory;
                m_Controller.RefreshCompressionHelper();
                m_Controller.RefreshBuildEventHandler();
                m_Controller.Save();
            }
        }

        public static void StartBuild(Platform platform, string outputDirectory, GameFrameworkAction complete = null)
        {
            m_Controller = new ResourceBuilderController();
            m_Controller.OnLoadingResource += OnLoadingResource;
            m_Controller.OnLoadingAsset += OnLoadingAsset;
            m_Controller.OnLoadCompleted += OnLoadCompleted;
            m_Controller.OnAnalyzingAsset += OnAnalyzingAsset;
            m_Controller.OnAnalyzeCompleted += OnAnalyzeCompleted;
            m_Controller.ProcessingAssetBundle += OnProcessingAssetBundle;
            m_Controller.ProcessingBinary += OnProcessingBinary;
            m_Controller.ProcessResourceComplete += OnProcessResourceComplete;
            m_Controller.BuildResourceError += OnBuildResourceError;
            m_Complete = complete;

            if (m_Controller.Load())
            {
                m_OriginalPlatform = m_Controller.Platforms;
                if (platform != Platform.Undefined)
                {
                    m_Controller.Platforms = platform;
                }
                if (m_Controller.OutputDirectory != outputDirectory)
                {
                    m_Controller.OutputDirectory = outputDirectory;
                }
                Debug.Log("Load configuration success.");

                m_Controller.RefreshCompressionHelper();

                m_Controller.RefreshBuildEventHandler();
            }
            else
            {
                Debug.LogWarning("Load configuration failure.");
            }

            string buildMessage = string.Empty;
            MessageType buildMessageType = MessageType.None;
            GetBuildMessage(out buildMessage, out buildMessageType);
            switch (buildMessageType)
            {
                case MessageType.None:
                case MessageType.Info:
                    Debug.Log(buildMessage);
                    BuildResources();
                    break;
                case MessageType.Warning:
                    Debug.LogWarning(buildMessage);
                    BuildResources();
                    break;
                case MessageType.Error:
                    Debug.LogError(buildMessage);
                    break;
            }
        }

        private static void GetBuildMessage(out string message, out MessageType messageType)
        {
            message = string.Empty;
            messageType = MessageType.Error;
            if (m_Controller.Platforms == Platform.Undefined)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    message += Environment.NewLine;
                }

                message += $"Platform {m_Controller.Platforms} is invalid.";
            }

            if (string.IsNullOrEmpty(m_Controller.CompressionHelperTypeName))
            {
                if (!string.IsNullOrEmpty(message))
                {
                    message += Environment.NewLine;
                }

                message += "Compression helper is invalid.";
            }

            if (!m_Controller.IsValidOutputDirectory)
            {
                if (!string.IsNullOrEmpty(message))
                {
                    message += Environment.NewLine;
                }

                message += $"Output directory {m_Controller.OutputDirectory} is invalid.";
            }

            if (!string.IsNullOrEmpty(message))
            {
                return;
            }

            messageType = MessageType.Info;
            if (Directory.Exists(m_Controller.OutputPackagePath))
            {
                message += Utility.Text.Format("{0} will be overwritten.", m_Controller.OutputPackagePath);
                messageType = MessageType.Warning;
            }

            if (Directory.Exists(m_Controller.OutputFullPath))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += Utility.Text.Format("{0} will be overwritten.", m_Controller.OutputFullPath);
                messageType = MessageType.Warning;
            }

            if (Directory.Exists(m_Controller.OutputPackedPath))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += Utility.Text.Format("{0} will be overwritten.", m_Controller.OutputPackedPath);
                messageType = MessageType.Warning;
            }

            if (messageType == MessageType.Warning)
            {
                return;
            }

            message = "Ready to build.";
        }

        private static void BuildResources()
        {
            if (m_Controller.BuildResources())
            {
                Debug.Log("Build resources success.");
                SaveConfiguration();
            }
            else
            {
                Debug.LogError($"Build resources failure. <a href=\"file:///{Utility.Path.GetRegularPath(Path.Combine(m_Controller.BuildReportPath, "BuildLog.txt"))}\" line=\"0\">[ Open BuildLog.txt ]</a>");
            }
        }

        private static void SaveConfiguration()
        {
            m_Controller.Platforms = m_OriginalPlatform;
            if (m_Controller.Save())
            {
                Debug.Log("Save configuration success.");
            }
            else
            {
                Debug.LogWarning("Save configuration failure.");
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
            Debug.Log(Utility.Text.Format("Build resources {0}({1}) for '{2}' complete.", m_Controller.ApplicableGameVersion, m_Controller.InternalResourceVersion, platform));
            m_Complete?.Invoke();
            m_Complete = null;
        }

        private static void OnBuildResourceError(string errorMessage)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogWarning(Utility.Text.Format("Build resources error with error message '{0}'.", errorMessage));
        }

    }
}

