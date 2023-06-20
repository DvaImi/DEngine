using System.IO;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using Game.Editor.Builder;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public sealed class GameBuildEventHandler : IBuildEventHandler
    {

        public bool ContinueOnFailure
        {
            get { return false; }
        }

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            IOUtility.Delete(Application.streamingAssetsPath);
            GameSetting.Instance.InternalResourceVersion = internalResourceVersion;
            GameSetting.Instance.SaveSetting();
            GameBuilder.RefreshResourceCollection();
            GameBuilder.SaveBuildInfo();
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
        }

        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
        {
            string platformPath = GameBuilder.GetPlatformPath(platform);
            VersionInfo versionInfo = new()
            {
                ForceUpdateGame = GameSetting.Instance.ForceUpdateGame,
                UpdatePrefixUri = GameBuilder.GetUpdatePrefixUri(platform),
                LatestGameVersion = GameSetting.Instance.LatestGameVersion,
                InternalGameVersion = 1,
                InternalResourceVersion = GameSetting.Instance.InternalResourceVersion,
                VersionListLength = versionListLength,
                VersionListHashCode = versionListHashCode,
                VersionListCompressedLength = versionListCompressedLength,
                VersionListCompressedHashCode = versionListCompressedHashCode
            };
            string versionJson = Newtonsoft.Json.JsonConvert.SerializeObject(versionInfo);
            string versionFilePath = Path.Combine(GameSetting.Instance.BundlesOutput, platformPath + "Version.json");
            File.WriteAllText(versionFilePath, versionJson);
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            if (!outputPackageSelected)
            {
                return;
            }

            if (GameSetting.Instance.ResourceModeIndex == (int)ResourceMode.Package)
            {
                GameBuilder.CopyFileToStreamingAssets(outputPackagePath);
            }
            else if (GameSetting.Instance.ResourceModeIndex == (int)ResourceMode.Updatable || GameSetting.Instance.ResourceModeIndex == (int)ResourceMode.UpdatableWhilePlaying)
            {
                GameBuilder.CopyFileToStreamingAssets(outputPackedPath);
            }

            if (GameSetting.Instance.AutoCopyToVirtualServer)
            {
                GameBuilder.PutToLocalSimulator(platform, outputFullPath);
            }
        }
    }
}
