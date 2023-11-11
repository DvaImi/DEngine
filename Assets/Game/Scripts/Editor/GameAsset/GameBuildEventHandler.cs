using System.IO;
using DEngine.Editor.ResourceTools;
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
            string platformPath = BuildPipeline.GameBuildPipeline.GetPlatformPath(platform);
            VersionInfo versionInfo = new()
            {
                ForceUpdateGame = GameSetting.Instance.ForceUpdateGame,
                UpdatePrefixUri = BuildPipeline.GameBuildPipeline.GetUpdatePrefixUri(platform),
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
            //差异化打包特殊处理
            if (!outputPackageSelected || GameSetting.Instance.Difference)
            {
                return;
            }
            if (GameSetting.Instance.AutoCopyToFileServer)
            {
                BuildPipeline.GameBuildPipeline.PutToLocalFileServer(platform, outputPackedPath);
            }
            int resourceMode = GameSetting.Instance.ResourceModeIndex;
            string sourcePath = resourceMode <= 1 ? outputPackagePath : outputPackedPath;
            BuildPipeline.GameBuildPipeline.CopyFileToStreamingAssets(sourcePath);
        }
    }
}
