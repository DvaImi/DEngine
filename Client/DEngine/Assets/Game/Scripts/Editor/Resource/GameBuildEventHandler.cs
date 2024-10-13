using System.IO;
using DEngine;
using DEngine.Editor.ResourceTools;
using Newtonsoft.Json;
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
            GameUtility.IO.Delete(Application.streamingAssetsPath);
            DEngineSetting.Instance.LatestGameVersion = applicableGameVersion;
            DEngineSetting.Instance.InternalResourceVersion = internalResourceVersion;
            DEngineSetting.Save();
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
                ForceUpdateGame = DEngineSetting.Instance.ForceUpdateGame,
                UpdatePrefixUri = BuildPipeline.GameBuildPipeline.GetUpdatePrefixUri(platform),
                LatestGameVersion = DEngineSetting.Instance.LatestGameVersion,
                InternalGameVersion = 1,
                InternalResourceVersion = DEngineSetting.Instance.InternalResourceVersion,
                VersionListLength = versionListLength,
                VersionListHashCode = versionListHashCode,
                VersionListCompressedLength = versionListCompressedLength,
                VersionListCompressedHashCode = versionListCompressedHashCode
            };
            string versionJson = JsonConvert.SerializeObject(versionInfo);
            string versionFilePath = Path.Combine(DEngineSetting.BundlesOutput, platformPath + "Version.json");
            File.WriteAllText(versionFilePath, versionJson);
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            //差异化打包特殊处理
            if (!outputPackageSelected || DEngineSetting.Instance.Difference)
            {
                return;
            }

            string sourcePath = outputPackagePath;

            switch (DEngineSetting.Instance.ResourceMode)
            {
                case DEngine.Resource.ResourceMode.Unspecified:
                case DEngine.Resource.ResourceMode.Package:
                    break;
                case DEngine.Resource.ResourceMode.Updatable:
                case DEngine.Resource.ResourceMode.UpdatableWhilePlaying:
                    sourcePath = outputPackedPath;
                    break;
                default:
                    sourcePath = outputPackagePath;
                    break;
            }

            BuildPipeline.GameBuildPipeline.CopyFileToStreamingAssets(sourcePath);
        }
    }
}