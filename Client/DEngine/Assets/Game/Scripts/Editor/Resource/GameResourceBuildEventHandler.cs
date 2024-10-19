using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
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

        private Dictionary<string, string> m_WaitUploadFileNames = new();

        public void OnPreprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            GameUtility.IO.CreateDirectoryIfNotExists(DEngineSetting.BundlesOutput);
            GameUtility.IO.CreateDirectoryIfNotExists(Application.streamingAssetsPath);

            if (DEngineSetting.Instance.ResourceMode == ResourceMode.Unspecified)
            {
                DEngineSetting.Instance.ResourceMode = ResourceMode.Package;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameUtility.IO.Delete(Application.streamingAssetsPath);
            DEngineSetting.Instance.LatestGameVersion = applicableGameVersion;
            DEngineSetting.Instance.InternalResourceVersion = internalResourceVersion;
            DEngineSetting.Save();
            m_WaitUploadFileNames.Clear();
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
                UpdatePrefixUri = GameBuildPipeline.GetUpdatePrefixUri(platform),
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
            if (DEngineSetting.Instance.EnableHostingService)
            {
                m_WaitUploadFileNames.Add(versionFilePath, platformPath + "Version.json");
            }

            JObject checkVersionInfo = new JObject()
            {
                ["CheckVersionUrl"] = GameBuildPipeline.GetCheckVersionUrl(platform)
            };
            versionJson = JsonConvert.SerializeObject(checkVersionInfo);
            versionFilePath = Path.Combine(DEngineSetting.BundlesOutput, platformPath + "CheckVersion.json");
            File.WriteAllText(versionFilePath, versionJson);
            GameBuildPipeline.SaveBuiltinData(platform);
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

            if (!DEngineSetting.Instance.EnableHostingService)
            {
                return;
            }

            const string partToFind = "Full/";

            string[] fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                string bundlesOutput = DEngineSetting.BundlesOutput;
                string relativePath = fileName[bundlesOutput.Length..];

                int indexOfFull = relativePath.IndexOf(partToFind, StringComparison.Ordinal);

                if (indexOfFull >= 0)
                {
                    string destFileName = relativePath[(indexOfFull + partToFind.Length)..];
                    m_WaitUploadFileNames.Add(fileName, destFileName);
                }
                else
                {
                    Debug.LogError("'Full/' not found in the path.");
                }
            }
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            EditorUtility.UnloadUnusedAssetsImmediate();

            if (!DEngineSetting.Instance.EnableHostingService)
            {
                return;
            }

            InternalUpload().Forget();
        }


        private async UniTask InternalUpload()
        {
            Debug.Log($"Ready to Upload To {DEngineSetting.Instance.HostURL}...");
            await UniTask.Delay(2000);
            if (!HostingServiceManager.IsListening)
            {
                HostingServiceManager.StartService();
            }

            foreach (var fileName in m_WaitUploadFileNames)
            {
                await HostingServiceManager.UploadFile(fileName.Key, fileName.Value);
            }

            Debug.Log($"Upload To {DEngineSetting.Instance.HostURL} complete...");
            HostingServiceManager.StopService();
        }
    }
}