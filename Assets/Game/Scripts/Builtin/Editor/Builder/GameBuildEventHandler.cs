//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Game.Editor.ResourceTools;
using GameFramework;
using GameFramework.Resource;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor
{
    public sealed class GameBuildEventHandler : IBuildEventHandler
    {
        private string m_GameVersion;
        private int m_InternalResourceVersion;
        private string m_OutputDirectory;

        public bool ContinueOnFailure
        {
            get { return false; }
        }

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName,
            bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName,
            string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions,
            string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected,
            string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            m_GameVersion = applicableGameVersion;
            m_InternalResourceVersion = internalResourceVersion;
            m_OutputDirectory = outputDirectory;

            GameAssetBuilder.OnPreprocess();
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier,
            string gameFrameworkVersion, string unityVersion, string applicableGameVersion, int internalResourceVersion,
            Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName,
            bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName,
            string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions,
            string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected,
            string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected,
            string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected,
            string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength,
            int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
        {
            string platformPath = GetPlatformPath(platform);
            VersionInfo versionInfo = new()
            {
                ForceUpdateGame = GameSetting.Instance.ForceUpdateGame,
                UpdatePrefixUri = Utility.Text.Format(GameSetting.Instance.UpdatePrefixUri, m_GameVersion, m_InternalResourceVersion, platformPath),
                LatestGameVersion = m_GameVersion,
                InternalGameVersion = 1,
                InternalResourceVersion = m_InternalResourceVersion,
                VersionListLength = versionListLength,
                VersionListHashCode = versionListHashCode,
                VersionListCompressedLength = versionListCompressedLength,
                VersionListCompressedHashCode = versionListCompressedHashCode
            };
            string versionJson = Newtonsoft.Json.JsonConvert.SerializeObject(versionInfo);
            IOUtility.SaveFileSafe(m_OutputDirectory, platformPath + "Version.bytes", versionJson);

            Debug.LogFormat(
                "Version save success. \n length is {0} , hash code is {1} . \n compressed length is {2} , compressed hash code is {3} . \n list path is {4} \n ",
                versionListLength, versionListHashCode, versionListCompressedLength, versionListCompressedHashCode,
                versionListPath);
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected,
            string outputPackedPath, bool isSuccess)
        {
            if (!outputPackageSelected)
            {
                return;
            }

            if (platform != Platform.Windows && platform != Platform.Windows64)
            {
                return;
            }

            ResourceMode resourceMode = (ResourceMode)GameSetting.Instance.ResourceModeIndex;

            #region StreamingAssets
            string fileSourcePath = outputPackagePath;
            //非单机模式下只拷贝outputPackedPath下的文件
            if (resourceMode != ResourceMode.Package)
            {
                fileSourcePath = outputPackedPath;
            }

            string streamingAssetsPath = Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "StreamingAssets"));
            string[] fileNames = Directory.GetFiles(fileSourcePath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string destFileName = Utility.Path.GetRegularPath(Path.Combine(streamingAssetsPath, fileName[fileSourcePath.Length..]));
                FileInfo destFileInfo = new FileInfo(destFileName);
                if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                {
                    destFileInfo.Directory.Create();
                }

                File.Copy(fileName, destFileName);
            }

            #endregion

            #region Simulator

            string virtualServerAddress = GameSetting.Instance.VirtualServerAddress;

            if (!Directory.Exists(virtualServerAddress))
            {
                Debug.LogWarning("VirtualServerAddress is invalid");
                return;
            }

            DirectoryInfo directoryInfo = new(virtualServerAddress);
            FileInfo[] fileInfos = directoryInfo.GetFiles("*", SearchOption.AllDirectories);
            DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

            foreach (var file in fileInfos)
            {
                file.Delete();
            }

            foreach (var item in directoryInfos)
            {
                item.Delete(true);
            }

            if (resourceMode != ResourceMode.Package && GameSetting.Instance.AutoCopyToVirtualServer)
            {
                string versionJson = Path.Combine(m_OutputDirectory, GetPlatformPath(platform) + "Version.bytes");
                if (File.Exists(versionJson))
                {
                    File.Copy(versionJson, Path.Combine(virtualServerAddress, GetPlatformPath(platform) + "Version.bytes"));

                    fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);
                    foreach (string fileName in fileNames)
                    {
                        string destFileName = Utility.Path.GetRegularPath(Path.Combine(virtualServerAddress, m_GameVersion + "." + m_InternalResourceVersion.ToString(), GetPlatformPath(platform), fileName[outputFullPath.Length..]));
                        FileInfo destFileInfo = new FileInfo(destFileName);
                        if (destFileInfo.Directory != null && !destFileInfo.Directory.Exists)
                        {
                            destFileInfo.Directory.Create();
                        }

                        File.Copy(fileName, destFileName);
                    }

                    Debug.Log("Copy Bundles to virtualServer success");
                }

            }
            #endregion
        }

        /// <summary>
        /// 由 UnityGameFramework.Editor.ResourceTools.Platform 得到 平台标识符。
        /// </summary>
        /// <param name="platform">UnityGameFramework.Editor.ResourceTools.Platform。</param>
        /// <returns>平台标识符。</returns>
        public string GetPlatformPath(Platform platform)
        {
            // 这里和 ProcedureVersionCheck.GetPlatformPath() 对应。
            // 使用 平台标识符 关联 UnityEngine.RuntimePlatform 和 UnityGameFramework.Editor.ResourceTools.Platform
            switch (platform)
            {
                case Platform.Windows:
                    return "Windows";

                case Platform.Windows64:
                    return "Windows64";

                case Platform.MacOS:
                    return "MacOS";

                case Platform.IOS:
                    return "IOS";

                case Platform.Android:
                    return "Android";

                case Platform.WindowsStore:
                    return "WSA";

                case Platform.WebGL:
                    return "WebGL";

                case Platform.Linux:
                    return "Linux";

                default:
                    throw new GameFrameworkException("Platform is invalid.");
            }
        }
    }
}