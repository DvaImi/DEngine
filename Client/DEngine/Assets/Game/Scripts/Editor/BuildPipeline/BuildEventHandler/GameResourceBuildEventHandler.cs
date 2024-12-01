using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DEngine;
using DEngine.Editor.ResourceTools;
using DEngine.Resource;
using Game.Editor.BuildPipeline;
using Game.Editor.ResourceTools;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public sealed class GameResourceBuildEventHandler : IBuildEventHandler
    {
        public bool ContinueOnFailure => true;

        private readonly Dictionary<string, string> m_WaitUploadFileNames = new();
        private const string FullPartToFind = "Full/";
        private VersionInfo m_VersionInfo;

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
            m_VersionInfo = null;
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

            m_VersionInfo = new VersionInfo
            {
                ForceUpdateGame = DEngineSetting.Instance.ForceUpdateGame,
                UpdatePrefixUri = GameBuildPipeline.GetUpdatePrefixUri(platform),
                LatestGameVersion = DEngineSetting.Instance.LatestGameVersion,
                InternalGameVersion = DEngineSetting.Instance.InternalGameVersion,
                InternalResourceVersion = DEngineSetting.Instance.InternalResourceVersion,
                ResourceVersionInfo =
                {
                    VersionListLength = versionListLength,
                    VersionListHashCode = versionListHashCode,
                    VersionListCompressedLength = versionListCompressedLength,
                    VersionListCompressedHashCode = versionListCompressedHashCode
                }
            };
        }

        public void OnPostprocessPlatform(Platform platform, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, bool isSuccess)
        {
            string sourcePath = DEngineSetting.Instance.ResourceMode switch
            {
                ResourceMode.Unspecified or ResourceMode.Package             => outputPackagePath,
                ResourceMode.Updatable or ResourceMode.UpdatableWhilePlaying => outputPackedPath,
                _                                                            => outputPackagePath
            };

            GameBuildPipeline.CopyFileToStreamingAssets(sourcePath);

            if (DEngineSetting.Instance.ResourceMode <= ResourceMode.Package)
            {
                return;
            }

            if (m_VersionInfo == null)
            {
                Debug.LogWarning("Version info is null");
                return;
            }

            string platformPath = GameBuildPipeline.GetPlatformPath(platform);
            string sourceVersion = DEngineSetting.Instance.SourceVersion;
            if (DEngineSetting.Instance.BuildResourcePack && GameBuildPipeline.BuildResourcePack(platform, sourceVersion, out string outputPath, out string targetVersion))
            {
                Debug.LogFormat("Build Resource Pack {0}==>{1} Success", sourceVersion, targetVersion);
                string searchPattern = Utility.Text.Format("{0}-{1}-{2}.*.block", "ResourcePack", sourceVersion, targetVersion);
                string[] files = Directory.GetFiles(outputPath, searchPattern);
                if (files.Length > 0)
                {
                    FileInfo fileInfo = new(files[0]);
                    m_VersionInfo.IsCompressedMode = true;
                    m_VersionInfo.ResourcePackInfo.ResourcePackName = fileInfo.Name;
                    m_VersionInfo.ResourcePackInfo.ResourcePackLength = fileInfo.Length;
                    m_VersionInfo.ResourcePackInfo.Version = Convert.ToInt32(targetVersion.Split('.')[^1]);
                    string destFileName = Utility.Path.GetRegularCombinePath(targetVersion, platformPath, fileInfo.FullName[outputPath.Length..]);
                    m_WaitUploadFileNames.Add(fileInfo.FullName, destFileName);
                }

                var targetDirectoryInfo = new DirectoryInfo(Utility.Text.Format("{0}/{1}-{2}-{3}", outputPath, "ResourcePack", sourceVersion, targetVersion));
                var targetVersionListFiles = targetDirectoryInfo.GetFiles("RemoteVersionList.*.block", SearchOption.TopDirectoryOnly);
                if (targetVersionListFiles.Length > 0)
                {
                    string destFileName = Utility.Path.GetRegularCombinePath(targetVersion, platformPath, targetVersionListFiles[0].Name);
                    m_WaitUploadFileNames.Add(targetVersionListFiles[0].FullName, destFileName);
                }
            }

            PrecessLocalHost(outputFullPath);
            GameBuildPipeline.SaveBuiltinData();
            m_WaitUploadFileNames.Add(SaveVersionInfo(platformPath), platformPath + "Version.json");
        }

        public void OnPostprocessAllPlatforms(string productName, string companyName, string gameIdentifier, string unityVersion, string applicableGameVersion, int internalResourceVersion, Platform platforms, AssetBundleCompressionType assetBundleCompression, string compressionHelperTypeName, bool additionalCompressionSelected, bool forceRebuildAssetBundleSelected, string buildEventHandlerTypeName, string outputDirectory, BuildAssetBundleOptions buildAssetBundleOptions, string workingPath, bool outputPackageSelected, string outputPackagePath, bool outputFullSelected, string outputFullPath, bool outputPackedSelected, string outputPackedPath, string buildReportPath)
        {
            EditorUtility.UnloadUnusedAssetsImmediate();

            if (!DEngineSetting.Instance.EnableHostingService)
            {
                EditorUtility.RevealInFinder(DEngineSetting.BundlesOutput);
                return;
            }

            if (DEngineSetting.Instance.ResourceMode <= ResourceMode.Package)
            {
                return;
            }

            if (m_WaitUploadFileNames.Count <= 0)
            {
                return;
            }

            InternalUpload().Forget();
        }

        private string SaveVersionInfo(string platformPath)
        {
            string versionJson = JsonConvert.SerializeObject(m_VersionInfo);
            string versionFolder = Utility.Path.GetRegularPath(new DirectoryInfo(Utility.Text.Format("{0}/Full/{1}.{2}/", DEngineSetting.BundlesOutput, m_VersionInfo.LatestGameVersion, m_VersionInfo.InternalResourceVersion)).FullName);
            string versionFilePath = Utility.Path.GetRegularCombinePath(versionFolder, platformPath + "Version.json");
            File.WriteAllText(versionFilePath, versionJson);
            return versionFilePath;
        }

        private void PrecessLocalHost(string outputFullPath)
        {
            if (DEngineSetting.Instance.EnableHostingService)
            {
                string[] fileNames = Directory.GetFiles(outputFullPath, "*", SearchOption.AllDirectories);
                foreach (string fileName in fileNames)
                {
                    string bundlesOutput = DEngineSetting.BundlesOutput;
                    string relativePath = fileName[bundlesOutput.Length..];

                    int indexOfFull = relativePath.IndexOf(FullPartToFind, StringComparison.Ordinal);

                    if (indexOfFull >= 0)
                    {
                        string destFileName = relativePath[(indexOfFull + FullPartToFind.Length)..];
                        m_WaitUploadFileNames.Add(fileName, destFileName);
                    }
                    else
                    {
                        Debug.LogError("'Full/' not found in the path.");
                    }
                }
            }
        }

        private async UniTask InternalUpload()
        {
            Debug.Log($"Ready to Upload To {DEngineSetting.Instance.HostURL}...");
            EditorUtility.DisplayProgressBar("Uploading Files", "Starting Upload...", 0);
            if (!HostingServiceManager.IsListening)
            {
                HostingServiceManager.StartService();
            }

            int totalFiles = m_WaitUploadFileNames.Count;
            int currentFileIndex = 0;

            try
            {
                foreach (var fileName in m_WaitUploadFileNames)
                {
                    await HostingServiceManager.UploadFile(fileName.Key, fileName.Value);
                    currentFileIndex++;
                    float progress = (float)currentFileIndex / totalFiles;
                    EditorUtility.DisplayProgressBar("Uploading Files", $"Uploading {fileName.Key}...", progress);
                }

                EditorUtility.DisplayProgressBar("Uploading Files", "Upload Complete", 1);
                Debug.Log($"Upload To {DEngineSetting.Instance.HostURL} complete...");
            }
            finally
            {
                HostingServiceManager.StopService();
                m_WaitUploadFileNames.Clear();
                EditorUtility.ClearProgressBar();
            }
        }
    }
}