using System.IO;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using Game.Editor.BuildPipeline;
using Game.Editor.ResourceTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public sealed class GameResourceBuildEventHandler : IBuildEventHandler
    {
        public bool ContinueOnFailure
        {
            get { return true; }
        }

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.BundlesOutput);
            GameUtility.IO.CreateDirectoryIfNotExists(Application.streamingAssetsPath);

            if (DEngineSetting.Instance.ResourceMode == ResourceMode.Unspecified)
            {
                DEngineSetting.Instance.ResourceMode = ResourceMode.Package;
            }

            GameBuildPipeline.CleanUnknownAssets();
            ResourceCollectorEditorUtility.RefreshResourceCollection();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameUtility.IO.Delete(Application.streamingAssetsPath);
            DEngineSetting.Instance.LatestGameVersion = applicableGameVersion;
            DEngineSetting.Instance.InternalResourceVersion = internalResourceVersion;
            DEngineSetting.Save();
        }


        public void OnPreprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath)
        {
        }

        public void OnBuildAssetBundlesComplete(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, AssetBundleManifest assetBundleManifest)
        {
        }

        public void OnOutputUpdatableVersionListData(Platform platform, string versionListPath, int versionListLength, int versionListHashCode, int versionListCompressedLength, int versionListCompressedHashCode)
        {
            if (DEngineSetting.Instance.ResourceMode is not (ResourceMode.Updatable or ResourceMode.UpdatableWhilePlaying))
            {
                return;
            }

            string platformPath = GameBuildPipeline.GetPlatformPath(platform);
            VersionInfo versionInfo = new()
            {
                ForceUpdateGame = DEngineSetting.Instance.ForceUpdateGame,
                UpdatePrefixUri = DEngineSetting.Instance.UpdatePrefixUri,
                LatestGameVersion = DEngineSetting.Instance.LatestGameVersion,
                InternalGameVersion = DEngineSetting.Instance.InternalGameVersion,
                InternalResourceVersion = DEngineSetting.Instance.InternalResourceVersion,
                VersionListLength = versionListLength,
                VersionListHashCode = versionListHashCode,
                VersionListCompressedLength = versionListCompressedLength,
                VersionListCompressedHashCode = versionListCompressedHashCode
            };
            string versionJson = JsonConvert.SerializeObject(versionInfo);
            string versionFilePath = Path.Combine(DEngineSetting.BundlesOutput, platformPath + "Version.json");
            File.WriteAllText(versionFilePath, versionJson);

            JObject checkVersionInfo = new JObject()
            {
                ["CheckVersionUrl"] = GameBuildPipeline.GetCheckVersionUrl(platform)
            };
            versionJson = JsonConvert.SerializeObject(checkVersionInfo);
            versionFilePath = Path.Combine(DEngineSetting.BundlesOutput, platformPath + "CheckVersion.json");
            File.WriteAllText(versionFilePath, versionJson);
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            string sourcePath = DEngineSetting.Instance.ResourceMode switch
            {
                ResourceMode.Unspecified or ResourceMode.Package => outputPackagePath,
                ResourceMode.Updatable or ResourceMode.UpdatableWhilePlaying => outputPackedPath,
                _ => outputPackagePath
            };

            GameBuildPipeline.CopyFileToStreamingAssets(sourcePath);
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
        }
    }
}