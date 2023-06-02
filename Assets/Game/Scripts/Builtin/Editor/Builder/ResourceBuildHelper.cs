// ========================================================
// 描述：
// 作者：GeminiLion 
// 创建时间：2023-04-29 10:50:09
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using System.IO;
using GameFramework;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor.ResourceTools
{
    /// <summary>
    /// 资源生成器。
    /// </summary>
    public static class ResourceBuildHelper
    {
        private static ResourceBuilderController m_Controller = null;
        private static Platform m_OriginalPlatform;

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

        /// <summary>
        /// build resource
        /// </summary>
        /// <param name="specificPlatform">为Undefined使用设置的平台</param>
        public static void StartBuild(Platform platform)
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

            if (m_Controller.Load())
            {
                m_OriginalPlatform = m_Controller.Platforms;
                if (platform != Platform.Undefined)
                {
                    m_Controller.Platforms = platform;
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

        internal static void AnalyzeAddress()
        {
            ResourceCollection controller = new ResourceCollection();
            if (controller.Load())
            {
                HashSet<string> temeper = new HashSet<string>();

                foreach (var asset in controller.GetAssets())
                {
                    string address = Path.GetFileNameWithoutExtension(asset.Name);
                    if (temeper.Contains(address))
                    {
                        throw new GameFrameworkException($"The address is existed : {address} in collector : {asset.Name}");
                    }
                    else
                    {
                        temeper.Add(address);
                    }
                }
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
                message += GameFramework.Utility.Text.Format("{0} will be overwritten.",
                    m_Controller.OutputPackagePath);
                messageType = MessageType.Warning;
            }

            if (Directory.Exists(m_Controller.OutputFullPath))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += GameFramework.Utility.Text.Format("{0} will be overwritten.", m_Controller.OutputFullPath);
                messageType = MessageType.Warning;
            }

            if (Directory.Exists(m_Controller.OutputPackedPath))
            {
                if (message.Length > 0)
                {
                    message += " ";
                }

                message += GameFramework.Utility.Text.Format("{0} will be overwritten.", m_Controller.OutputPackedPath);
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
                Debug.LogError($"Build resources failure. <a href=\"file:///{GameFramework.Utility.Path.GetRegularPath(Path.Combine(m_Controller.BuildReportPath, "BuildLog.txt"))}\" line=\"0\">[ Open BuildLog.txt ]</a>");
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
            EditorUtility.DisplayProgressBar("Loading Resources",
                GameFramework.Utility.Text.Format("Loading resources, {0}/{1} loaded.", index, count),
                (float)index / count);
        }

        private static void OnLoadingAsset(int index, int count)
        {
            EditorUtility.DisplayProgressBar("Loading Assets",
                GameFramework.Utility.Text.Format("Loading assets, {0}/{1} loaded.", index, count),
                (float)index / count);
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
            if (EditorUtility.DisplayCancelableProgressBar("Processing AssetBundle",
                    GameFramework.Utility.Text.Format("Processing '{0}'...", assetBundleName), progress))
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
            if (EditorUtility.DisplayCancelableProgressBar("Processing Binary",
                    GameFramework.Utility.Text.Format("Processing '{0}'...", binaryName), progress))
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
            Debug.Log(GameFramework.Utility.Text.Format("Build resources {0}({1}) for '{2}' complete.",
                m_Controller.ApplicableGameVersion, m_Controller.InternalResourceVersion, platform));
        }

        private static void OnBuildResourceError(string errorMessage)
        {
            EditorUtility.ClearProgressBar();
            Debug.LogWarning(Utility.Text.Format("Build resources error with error message '{0}'.", errorMessage));
        }


    }
}

