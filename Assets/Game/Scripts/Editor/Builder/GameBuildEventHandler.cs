//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.IO;
using Game.Editor.ResourceTools;
using GameFramework;
using GameFramework.Resource;
using UnityEditor;
using UnityEngine;
using UnityGameFramework.Editor.ResourceTools;

namespace Game.Editor
{
    public sealed class GameBuildEventHandler : IBuildEventHandler
    {

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
            GameSetting.Instance.InternalResourceVersion = internalResourceVersion;
            GameSetting.Instance.LatestGameVersion = applicableGameVersion;
            GameSetting.Instance.SaveSetting();
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
            string platformPath = GameAssetBuilder.GetPlatformPath(platform);
            VersionInfo versionInfo = new()
            {
                ForceUpdateGame = GameSetting.Instance.ForceUpdateGame,
                UpdatePrefixUri = GameAssetBuilder.GetUpdatePrefixUri(platform),
                LatestGameVersion = GameSetting.Instance.LatestGameVersion,
                InternalGameVersion = 1,
                InternalResourceVersion = GameSetting.Instance.InternalResourceVersion,
                VersionListLength = versionListLength,
                VersionListHashCode = versionListHashCode,
                VersionListCompressedLength = versionListCompressedLength,
                VersionListCompressedHashCode = versionListCompressedHashCode
            };
            string versionJson = Newtonsoft.Json.JsonConvert.SerializeObject(versionInfo);
            string versionFilePath = Path.Combine(GameSetting.Instance.BundlesOutput, platformPath + "Version.bytes");
            IOUtility.SaveFileSafe(versionFilePath, versionJson);
            Debug.LogFormat("Version save success. \n length is {0} , hash code is {1} . \n compressed length is {2} , compressed hash code is {3} . \n list path is {4} \n ", versionListLength, versionListHashCode, versionListCompressedLength, versionListCompressedHashCode, versionListPath);
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected,
            string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected,
            string outputPackedPath, bool isSuccess)
        {
            if (!outputPackageSelected)
            {
                return;
            }

            ResourceMode resourceMode = (ResourceMode)GameSetting.Instance.ResourceModeIndex;
            if (platform == Platform.Windows || platform == Platform.Windows64)
            {
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

            }
            #region Simulator

            if (!GameSetting.Instance.AutoCopyToVirtualServer)
            {
                return;
            }

            string virtualServerAddress = GameSetting.Instance.VirtualServerAddress;

            if (!Directory.Exists(virtualServerAddress))
            {
                Debug.LogWarning("VirtualServerAddress is invalid");
                return;
            }

            if (resourceMode != ResourceMode.Package && GameSetting.Instance.AutoCopyToVirtualServer)
            {
                string versionJson = Path.Combine(GameSetting.Instance.BundlesOutput, GameAssetBuilder.GetPlatformPath(platform) + "Version.bytes");
                if (File.Exists(versionJson))
                {
                    File.Copy(versionJson, Path.Combine(virtualServerAddress, GameAssetBuilder.GetPlatformPath(platform) + "Version.bytes"), true);

                    var fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);
                    foreach (string fileName in fileNames)
                    {
                        string destFileName = Utility.Path.GetRegularPath(Path.Combine(virtualServerAddress, GameSetting.Instance.LatestGameVersion + "." + GameSetting.Instance.InternalResourceVersion.ToString(), GameAssetBuilder.GetPlatformPath(platform), fileName[outputFullPath.Length..]));
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
    }
}
